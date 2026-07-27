using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace Dreamine.Communication.Serial.Streams;

/// <summary>
/// \if KO
/// <para><see cref="T:System.IO.Ports.SerialPort" /> 기본 스트림의 비동기 읽기와 쓰기를 캡슐화합니다.</para>
/// \endif
/// \if EN
/// <para>Encapsulates asynchronous reads and writes on a <see cref="T:System.IO.Ports.SerialPort" /> base stream.</para>
/// \endif
/// </summary>
public sealed class SerialPortStreamAdapter
{
    /// <summary>
    /// \if KO
    /// <para>serial Port 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the serial port value.</para>
    /// \endif
    /// </summary>
    private readonly SerialPort _serialPort;

    /// <summary>
    /// \if KO
    /// <para>지정한 시리얼 포트로 스트림 어댑터를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes the stream adapter with the specified serial port.</para>
    /// \endif
    /// </summary>
    /// <param name="serialPort">
    /// \if KO
    /// <para>기본 스트림을 제공할 시리얼 포트입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The serial port that provides the base stream.</para>
    /// \endif
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para>시리얼 포트가 <see langword="null"/>인 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the serial port is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    public SerialPortStreamAdapter(SerialPort serialPort)
    {
        _serialPort = serialPort ?? throw new ArgumentNullException(nameof(serialPort));
    }

    /// <summary>
    /// \if KO
    /// <para>시리얼 포트의 기본 입출력 스트림을 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the serial port's underlying input/output stream.</para>
    /// \endif
    /// </summary>
    public Stream BaseStream => _serialPort.BaseStream;

    /// <summary>
    /// \if KO
    /// <para>버퍼를 기본 스트림에 비동기 기록하고 즉시 플러시합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Asynchronously writes a buffer to the base stream and flushes it.</para>
    /// \endif
    /// </summary>
    /// <param name="buffer">
    /// \if KO
    /// <para>기록할 데이터입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The data to write.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>쓰기와 플러시 취소 요청을 감시하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token used to observe write and flush cancellation.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>비동기 쓰기 및 플러시 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task representing the asynchronous write and flush.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para>버퍼가 <see langword="null"/>인 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the buffer is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    public async Task WriteAsync(
        byte[] buffer,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        await BaseStream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
        await BaseStream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// \if KO
    /// <para>기본 스트림에서 제공된 메모리 버퍼로 데이터를 비동기 읽습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Asynchronously reads data from the base stream into the supplied memory buffer.</para>
    /// \endif
    /// </summary>
    /// <param name="buffer">
    /// \if KO
    /// <para>읽은 데이터를 저장할 버퍼입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The buffer that receives the data.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>읽기 취소 요청을 감시하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token used to observe read cancellation.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>읽은 바이트 수를 결과로 제공하는 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task whose result is the number of bytes read.</para>
    /// \endif
    /// </returns>
    public Task<int> ReadAsync(
        Memory<byte> buffer,
        CancellationToken cancellationToken = default)
    {
        return BaseStream.ReadAsync(buffer, cancellationToken).AsTask();
    }
}
