using System.IO.Ports;
using Dreamine.Communication.Abstractions.Enums;
using Dreamine.Communication.Abstractions.Interfaces;
using Dreamine.Communication.Abstractions.Models;
using Dreamine.Communication.Core.Framing;
using Dreamine.Communication.Core.Protocols;
using Dreamine.Communication.Serial.Options;
using Dreamine.Communication.Serial.Streams;

namespace Dreamine.Communication.Serial.Ports;

/// <summary>
/// \brief RS232 SerialPort 기반 메시지 전송 계층입니다.
/// </summary>
public sealed class SerialPortTransport : IMessageTransport
{
    private readonly SerialPortTransportOptions _options;
    private readonly IMessageProtocolAdapter _protocolAdapter;
    private readonly IMessageFrameCodec _frameCodec;

    private SerialPort? _serialPort;
    private SerialPortStreamAdapter? _streamAdapter;
    private CancellationTokenSource? _receiveLoopCts;
    private Task? _receiveLoopTask;

    /// <summary>
    /// \brief SerialPortTransport 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="options">시리얼 포트 설정입니다.</param>
    public SerialPortTransport(SerialPortTransportOptions options)
        : this(
            options,
            new DreamineEnvelopeProtocolAdapter(),
            new LengthPrefixedMessageFrameCodec())
    {
    }

    /// <summary>
    /// \brief SerialPortTransport 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="options">시리얼 포트 설정입니다.</param>
    /// <param name="protocolAdapter">메시지 프로토콜 어댑터입니다.</param>
    /// <param name="frameCodec">메시지 프레임 코덱입니다.</param>
    public SerialPortTransport(
        SerialPortTransportOptions options,
        IMessageProtocolAdapter protocolAdapter,
        IMessageFrameCodec frameCodec)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _protocolAdapter = protocolAdapter ?? throw new ArgumentNullException(nameof(protocolAdapter));
        _frameCodec = frameCodec ?? throw new ArgumentNullException(nameof(frameCodec));

        ValidateOptions(_options);
    }

    /// <summary>
    /// \brief 현재 연결 상태를 가져옵니다.
    /// </summary>
    public ConnectionState State { get; private set; } = ConnectionState.Disconnected;

    /// <summary>
    /// \brief 전송 방식 종류를 가져옵니다.
    /// </summary>
    public TransportKind Kind => TransportKind.Serial;

    /// <summary>
    /// \brief 메시지를 수신했을 때 발생합니다.
    /// </summary>
    public event EventHandler<MessageEnvelope>? MessageReceived;

    /// <summary>
    /// \brief 시리얼 포트를 엽니다.
    /// </summary>
    /// <param name="cancellationToken">취소 토큰입니다.</param>
    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (State is ConnectionState.Connected or ConnectionState.Connecting)
        {
            return Task.CompletedTask;
        }

        cancellationToken.ThrowIfCancellationRequested();

        State = ConnectionState.Connecting;

        try
        {
            _serialPort = new SerialPort(
                _options.PortName,
                _options.BaudRate,
                _options.Parity,
                _options.DataBits,
                _options.StopBits)
            {
                Handshake = _options.Handshake,
                ReadTimeout = _options.ReadTimeoutMs,
                WriteTimeout = _options.WriteTimeoutMs,
                ReadBufferSize = _options.ReadBufferSize,
                WriteBufferSize = _options.WriteBufferSize
            };

            _serialPort.Open();
            _streamAdapter = new SerialPortStreamAdapter(_serialPort);

            State = ConnectionState.Connected;

            _receiveLoopCts = new CancellationTokenSource();
            _receiveLoopTask = Task.Run(
                () => ReceiveLoopAsync(_receiveLoopCts.Token),
                _receiveLoopCts.Token);

            return Task.CompletedTask;
        }
        catch
        {
            State = ConnectionState.Faulted;

            _serialPort?.Dispose();
            _serialPort = null;
            _streamAdapter = null;

            throw;
        }
    }

    /// <summary>
    /// \brief 시리얼 포트를 닫습니다.
    /// </summary>
    /// <param name="cancellationToken">취소 토큰입니다.</param>
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (State == ConnectionState.Disconnected)
        {
            return;
        }

        State = ConnectionState.Disconnecting;

        _receiveLoopCts?.Cancel();

        if (_serialPort is not null)
        {
            try
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }
            }
            finally
            {
                _serialPort.Dispose();
            }
        }

        if (_receiveLoopTask is not null)
        {
            try
            {
                await _receiveLoopTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (InvalidOperationException)
            {
            }
            catch (IOException)
            {
            }
        }

        _receiveLoopCts?.Dispose();
        _receiveLoopCts = null;
        _receiveLoopTask = null;
        _serialPort = null;
        _streamAdapter = null;

        cancellationToken.ThrowIfCancellationRequested();

        State = ConnectionState.Disconnected;
    }

    /// <summary>
    /// \brief 메시지를 시리얼 포트로 전송합니다.
    /// </summary>
    /// <param name="message">전송할 메시지입니다.</param>
    /// <param name="cancellationToken">취소 토큰입니다.</param>
    public async Task SendAsync(
        MessageEnvelope message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (_serialPort is null ||
            _streamAdapter is null ||
            !_serialPort.IsOpen ||
            State != ConnectionState.Connected)
        {
            throw new InvalidOperationException("Serial port is not connected.");
        }

        var payload = _protocolAdapter.Encode(message);

        await _frameCodec.WriteFrameAsync(
                _streamAdapter.BaseStream,
                payload,
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// \brief 시리얼 포트 리소스를 비동기로 해제합니다.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync().ConfigureAwait(false);
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        if (_streamAdapter is null)
        {
            return;
        }

        try
        {
            while (!cancellationToken.IsCancellationRequested &&
                   State == ConnectionState.Connected)
            {
                var payload = await _frameCodec.ReadFrameAsync(
                        _streamAdapter.BaseStream,
                        cancellationToken)
                    .ConfigureAwait(false);

                if (payload is null)
                {
                    break;
                }

                var message = _protocolAdapter.Decode(payload);
                MessageReceived?.Invoke(this, message);
            }

            if (!cancellationToken.IsCancellationRequested &&
                State == ConnectionState.Connected)
            {
                State = ConnectionState.Disconnected;
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        catch (InvalidOperationException)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                State = ConnectionState.Faulted;
            }
        }
        catch (IOException)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                State = ConnectionState.Faulted;
            }
        }
        catch
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                State = ConnectionState.Faulted;
            }

            throw;
        }
    }

    private static void ValidateOptions(SerialPortTransportOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(options.PortName);

        if (options.BaudRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options.BaudRate));
        }

        if (options.DataBits is < 5 or > 8)
        {
            throw new ArgumentOutOfRangeException(nameof(options.DataBits));
        }

        if (options.ReadTimeoutMs <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options.ReadTimeoutMs));
        }

        if (options.WriteTimeoutMs <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options.WriteTimeoutMs));
        }

        if (options.ReadBufferSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options.ReadBufferSize));
        }

        if (options.WriteBufferSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options.WriteBufferSize));
        }
    }
}