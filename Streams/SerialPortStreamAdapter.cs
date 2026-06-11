using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace Dreamine.Communication.Serial.Streams;

/// <summary>
/// SerialPort의 BaseStream 접근을 캡슐화하는 어댑터입니다.
/// </summary>
public sealed class SerialPortStreamAdapter
{
    private readonly SerialPort _serialPort;

    /// <summary>
    /// SerialPortStreamAdapter 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="serialPort">대상 SerialPort입니다.</param>
    public SerialPortStreamAdapter(SerialPort serialPort)
    {
        _serialPort = serialPort ?? throw new ArgumentNullException(nameof(serialPort));
    }

    /// <summary>
    /// SerialPort의 기본 스트림을 가져옵니다.
    /// </summary>
    public Stream BaseStream => _serialPort.BaseStream;

    /// <summary>
    /// 스트림에 데이터를 비동기로 기록합니다.
    /// </summary>
    /// <param name="buffer">기록할 데이터입니다.</param>
    /// <param name="cancellationToken">취소 토큰입니다.</param>
    public async Task WriteAsync(
        byte[] buffer,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        await BaseStream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
        await BaseStream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 스트림에서 데이터를 비동기로 읽습니다.
    /// </summary>
    /// <param name="buffer">읽은 데이터를 저장할 버퍼입니다.</param>
    /// <param name="cancellationToken">취소 토큰입니다.</param>
    /// <returns>읽은 바이트 수입니다.</returns>
    public Task<int> ReadAsync(
        Memory<byte> buffer,
        CancellationToken cancellationToken = default)
    {
        return BaseStream.ReadAsync(buffer, cancellationToken).AsTask();
    }
}