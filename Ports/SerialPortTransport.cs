using System.IO;
using System.IO.Ports;
using Dreamine.Communication.Abstractions.Enums;
using Dreamine.Communication.Abstractions.Interfaces;
using Dreamine.Communication.Abstractions.Models;
using Dreamine.Communication.Core.Framing;
using Dreamine.Communication.Core.Protocols;
using Dreamine.Communication.Serial.Options;

namespace Dreamine.Communication.Serial.Ports;

/// <summary>
/// \if KO
/// <para>.NET <see cref="T:System.IO.Ports.SerialPort" />와 구성 가능한 프레임·프로토콜 어댑터를 사용하는 RS-232 메시지 전송 계층입니다.</para>
/// \endif
/// \if EN
/// <para>Provides RS-232 message transport using .NET <see cref="T:System.IO.Ports.SerialPort" /> with configurable framing and protocol adapters.</para>
/// \endif
/// </summary>
public sealed class SerialPortTransport : IMessageTransport
{
    /// <summary>
    /// \if KO
    /// <para>options 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the options value.</para>
    /// \endif
    /// </summary>
    private readonly SerialPortTransportOptions _options;
    /// <summary>
    /// \if KO
    /// <para>protocol Adapter 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the protocol adapter value.</para>
    /// \endif
    /// </summary>
    private readonly IMessageProtocolAdapter _protocolAdapter;
    /// <summary>
    /// \if KO
    /// <para>frame Codec 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the frame codec value.</para>
    /// \endif
    /// </summary>
    private readonly IMessageFrameCodec _frameCodec;

    /// <summary>
    /// \if KO
    /// <para>serial Port 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the serial port value.</para>
    /// \endif
    /// </summary>
    private SerialPort? _serialPort;
    /// <summary>
    /// \if KO
    /// <para>receive Loop Cts 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the receive loop cts value.</para>
    /// \endif
    /// </summary>
    private CancellationTokenSource? _receiveLoopCts;
    /// <summary>
    /// \if KO
    /// <para>receive Loop Task 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the receive loop task value.</para>
    /// \endif
    /// </summary>
    private Task? _receiveLoopTask;
    /// <summary>
    /// \if KO
    /// <para>state 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the state value.</para>
    /// \endif
    /// </summary>
    private int _state = (int)ConnectionState.Disconnected;

    /// <summary>
    /// \if KO
    /// <para>기본 Dreamine JSON 프로토콜과 길이 접두사 프레임으로 전송 계층을 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes the transport with the default Dreamine JSON protocol and length-prefixed framing.</para>
    /// \endif
    /// </summary>
    /// <param name="options">
    /// \if KO
    /// <para>시리얼 회선 및 버퍼 설정입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The serial line and buffer options.</para>
    /// \endif
    /// </param>
    public SerialPortTransport(SerialPortTransportOptions options)
        : this(
            options,
            new DreamineEnvelopeProtocolAdapter(),
            new LengthPrefixedMessageFrameCodec())
    {
    }

    /// <summary>
    /// \if KO
    /// <para>시리얼 설정과 사용자 지정 프로토콜 및 프레임 코덱으로 전송 계층을 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes the transport with serial options and custom protocol and frame codecs.</para>
    /// \endif
    /// </summary>
    /// <param name="options">
    /// \if KO
    /// <para>시리얼 회선 및 버퍼 설정입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The serial line and buffer options.</para>
    /// \endif
    /// </param>
    /// <param name="protocolAdapter">
    /// \if KO
    /// <para>메시지와 외부 페이로드를 변환할 어댑터입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The adapter that converts messages and external payloads.</para>
    /// \endif
    /// </param>
    /// <param name="frameCodec">
    /// \if KO
    /// <para>스트림의 메시지 경계를 처리할 코덱입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The codec that handles message boundaries in the stream.</para>
    /// \endif
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="options"/>, <paramref name="protocolAdapter"/> 또는 <paramref name="frameCodec"/>이 <see langword="null"/>인 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="options"/>, <paramref name="protocolAdapter"/>, or <paramref name="frameCodec"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
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
    /// \if KO
    /// <para>스레드 안전하게 현재 시리얼 연결 상태를 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the current serial connection state in a thread-safe manner.</para>
    /// \endif
    /// </summary>
    public ConnectionState State => (ConnectionState)Volatile.Read(ref _state);

    /// <summary>
    /// \if KO
    /// <para>시리얼 전송 방식을 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the serial transport kind.</para>
    /// \endif
    /// </summary>
    public TransportKind Kind => TransportKind.Serial;

    /// <summary>
    /// \if KO
    /// <para>완전한 프레임을 메시지로 디코딩했을 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Occurs when a complete frame has been decoded into a message.</para>
    /// \endif
    /// </summary>
    public event EventHandler<MessageEnvelope>? MessageReceived;

    /// <summary>
    /// \if KO
    /// <para>시리얼 포트를 열고 백그라운드 수신 루프를 시작합니다. 포트 열기 자체는 동기 API입니다.</para>
    /// \endif
    /// \if EN
    /// <para>Opens the serial port and starts the background receive loop; opening the port itself is synchronous.</para>
    /// \endif
    /// </summary>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>포트를 열기 전 취소 요청을 확인하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token checked for cancellation before the port is opened.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>연결 시작 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task representing connection startup.</para>
    /// \endif
    /// </returns>
    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (State is ConnectionState.Connected or ConnectionState.Connecting)
        {
            return Task.CompletedTask;
        }

        cancellationToken.ThrowIfCancellationRequested();

        SetState(ConnectionState.Connecting);

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

            SetState(ConnectionState.Connected);

            _receiveLoopCts = new CancellationTokenSource();
            _receiveLoopTask = Task.Run(
                () => ReceiveLoopAsync(_receiveLoopCts.Token),
                _receiveLoopCts.Token);

            return Task.CompletedTask;
        }
        catch
        {
            SetState(ConnectionState.Faulted);

            _serialPort?.Dispose();
            _serialPort = null;

            throw;
        }
    }

    /// <summary>
    /// \if KO
    /// <para>수신 루프를 중지하고 시리얼 포트와 관련 리소스를 닫습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stops the receive loop and closes the serial port and related resources.</para>
    /// \endif
    /// </summary>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>정리 후 연결 해제 취소 여부를 확인하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token checked for cancellation after cleanup.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>비동기 연결 해제 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task representing asynchronous disconnection.</para>
    /// \endif
    /// </returns>
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (State == ConnectionState.Disconnected)
        {
            return;
        }

        SetState(ConnectionState.Disconnecting);

        if (_receiveLoopCts is not null)
            await _receiveLoopCts.CancelAsync().ConfigureAwait(false);

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
            catch (Exception ex) when (ex is OperationCanceledException
                                       or ObjectDisposedException
                                       or InvalidOperationException
                                       or IOException)
            {
                // The receive loop may observe the port closing while disconnect tears it down.
            }
        }

        _receiveLoopCts?.Dispose();
        _receiveLoopCts = null;
        _receiveLoopTask = null;
        _serialPort = null;

        cancellationToken.ThrowIfCancellationRequested();

        SetState(ConnectionState.Disconnected);
    }

    /// <summary>
    /// \if KO
    /// <para>메시지를 외부 프로토콜과 프레임 형식으로 인코딩해 시리얼 포트로 전송합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Encodes a message using the external protocol and frame format and sends it over the serial port.</para>
    /// \endif
    /// </summary>
    /// <param name="message">
    /// \if KO
    /// <para>전송할 메시지입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The message to send.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>프레임 쓰기 취소 요청을 감시하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token used to observe frame-write cancellation.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>비동기 메시지 전송 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task representing asynchronous message transmission.</para>
    /// \endif
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// \if KO
    /// <para>시리얼 포트가 열려 있지 않거나 전송 계층이 연결 상태가 아닌 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the serial port is not open or the transport is not connected.</para>
    /// \endif
    /// </exception>
    public async Task SendAsync(
        MessageEnvelope message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (_serialPort is null ||
            !_serialPort.IsOpen ||
            State != ConnectionState.Connected)
        {
            throw new InvalidOperationException("Serial port is not connected.");
        }

        try
        {
            var payload = _protocolAdapter.Encode(message);

            await _frameCodec.WriteFrameAsync(
                    _serialPort.BaseStream,
                    payload,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch
        {
            SetState(ConnectionState.Faulted);
            CleanupSerialPort();

            throw;
        }
    }

    /// <summary>
    /// \if KO
    /// <para>시리얼 포트 연결과 수신 루프 리소스를 비동기적으로 해제합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Asynchronously releases the serial connection and receive-loop resources.</para>
    /// \endif
    /// <returns>
    /// \if KO
    /// <para>비동기 리소스 해제 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A value task representing asynchronous disposal.</para>
    /// \endif
    /// </returns>
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync(CancellationToken.None).ConfigureAwait(false);
    }

    /// <summary>
    /// \if KO
    /// <para>시리얼 스트림에서 프레임을 계속 읽어 메시지로 디코딩하고 수신 이벤트를 발생시킵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Continuously reads frames from the serial stream, decodes messages, and raises receive events.</para>
    /// \endif
    /// </summary>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>수신 루프 종료를 요청하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token used to request receive-loop termination.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>백그라운드 수신 루프 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task representing the background receive loop.</para>
    /// \endif
    /// </returns>
    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        if (_serialPort is null)
        {
            return;
        }

        try
        {
            while (!cancellationToken.IsCancellationRequested &&
                   State == ConnectionState.Connected)
            {
                var payload = await _frameCodec.ReadFrameAsync(
                        _serialPort.BaseStream,
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
                SetState(ConnectionState.Disconnected);
            }
        }
        catch (Exception ex) when (ex is OperationCanceledException or ObjectDisposedException)
        {
            // Cancellation and disposal are expected while the receive loop is shutting down.
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException)
        {
            SetFaultedUnlessCancelled(cancellationToken);
        }
        catch
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                SetState(ConnectionState.Faulted);
                CleanupSerialPort();
            }
        }
    }

    private void SetFaultedUnlessCancelled(CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            SetState(ConnectionState.Faulted);
        }
    }

    /// <summary>
    /// \if KO
    /// <para>예외를 외부로 전파하지 않고 시리얼 포트를 닫고 해제합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Closes and disposes the serial port without propagating cleanup exceptions.</para>
    /// \endif
    /// </summary>
    private void CleanupSerialPort()
    {
        try
        {
            if (_serialPort is not null && _serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }
        catch
        {
            // Cleanup is best-effort; the transport is being discarded regardless.
        }

        try
        {
            _serialPort?.Dispose();
        }
        catch
        {
            // Cleanup is best-effort; disposal failure must not mask the original failure.
        }

        _serialPort = null;
    }

    /// <summary>
    /// \if KO
    /// <para>원자적 연산으로 현재 연결 상태를 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Sets the current connection state using an atomic operation.</para>
    /// \endif
    /// </summary>
    /// <param name="state">
    /// \if KO
    /// <para>저장할 새 연결 상태입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The new connection state to store.</para>
    /// \endif
    /// </param>
    private void SetState(ConnectionState state)
    {
        Interlocked.Exchange(ref _state, (int)state);
    }

    /// <summary>
    /// \if KO
    /// <para>포트 이름, 회선 속성, 제한 시간 및 버퍼 크기의 유효성을 검사합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Validates the port name, line settings, timeouts, and buffer sizes.</para>
    /// \endif
    /// </summary>
    /// <param name="options">
    /// \if KO
    /// <para>검증할 시리얼 포트 설정입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The serial-port options to validate.</para>
    /// \endif
    /// </param>
    /// <exception cref="ArgumentException">
    /// \if KO
    /// <para>포트 이름이 비어 있는 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the port name is empty.</para>
    /// \endif
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// \if KO
    /// <para>수치 설정이 허용 범위를 벗어난 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when a numeric option is outside its allowed range.</para>
    /// \endif
    /// </exception>
    private static void ValidateOptions(SerialPortTransportOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(options.PortName);

        if (options.BaudRate <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                options.BaudRate,
                "BaudRate must be greater than zero.");
        }

        if (options.DataBits is < 5 or > 8)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                options.DataBits,
                "DataBits must be between 5 and 8.");
        }

        if (options.ReadTimeoutMs < System.IO.Ports.SerialPort.InfiniteTimeout)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                options.ReadTimeoutMs,
                "ReadTimeoutMs must be -1 or greater.");
        }

        if (options.WriteTimeoutMs < System.IO.Ports.SerialPort.InfiniteTimeout)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                options.WriteTimeoutMs,
                "WriteTimeoutMs must be -1 or greater.");
        }

        if (options.ReadBufferSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                options.ReadBufferSize,
                "ReadBufferSize must be greater than zero.");
        }

        if (options.WriteBufferSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                options.WriteBufferSize,
                "WriteBufferSize must be greater than zero.");
        }
    }
}
