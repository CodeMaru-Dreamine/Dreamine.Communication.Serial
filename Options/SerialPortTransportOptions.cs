using System.IO.Ports;

namespace Dreamine.Communication.Serial.Options;

/// <summary>
/// \if KO
/// <para>RS-232 시리얼 포트의 회선, 제한 시간 및 버퍼 설정을 구성합니다.</para>
/// \endif
/// \if EN
/// <para>Configures line, timeout, and buffer settings for an RS-232 serial port.</para>
/// \endif
/// </summary>
public sealed class SerialPortTransportOptions
{
    /// <summary>
    /// \if KO
    /// <para>열 시리얼 포트 이름을 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the serial port name to open.</para>
    /// \endif
    /// </summary>
    public string PortName { get; set; } = "COM1";

    /// <summary>
    /// \if KO
    /// <para>회선 통신 속도인 보드율을 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the line baud rate.</para>
    /// \endif
    /// </summary>
    public int BaudRate { get; set; } = 9600;

    /// <summary>
    /// \if KO
    /// <para>문자당 데이터 비트 수를 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the number of data bits per character.</para>
    /// \endif
    /// </summary>
    public int DataBits { get; set; } = 8;

    /// <summary>
    /// \if KO
    /// <para>패리티 검사 방식을 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the parity-checking scheme.</para>
    /// \endif
    /// </summary>
    public Parity Parity { get; set; } = Parity.None;

    /// <summary>
    /// \if KO
    /// <para>정지 비트 설정을 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the stop-bit setting.</para>
    /// \endif
    /// </summary>
    public StopBits StopBits { get; set; } = StopBits.One;

    /// <summary>
    /// \if KO
    /// <para>흐름 제어 Handshake 방식을 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the flow-control handshake mode.</para>
    /// \endif
    /// </summary>
    public Handshake Handshake { get; set; } = Handshake.None;

    /// <summary>
    /// \if KO
    /// <para>읽기 제한 시간(밀리초)을 가져오거나 설정합니다. 0은 즉시 반환하고 -1은 무한 대기입니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the read timeout in milliseconds; zero returns immediately and -1 waits indefinitely.</para>
    /// \endif
    /// </summary>
    public int ReadTimeoutMs { get; set; } = 3000;

    /// <summary>
    /// \if KO
    /// <para>쓰기 제한 시간(밀리초)을 가져오거나 설정합니다. 0은 즉시 반환하고 -1은 무한 대기입니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the write timeout in milliseconds; zero returns immediately and -1 waits indefinitely.</para>
    /// \endif
    /// </summary>
    public int WriteTimeoutMs { get; set; } = 3000;

    /// <summary>
    /// \if KO
    /// <para>수신 버퍼 크기(바이트)를 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the receive buffer size in bytes.</para>
    /// \endif
    /// </summary>
    public int ReadBufferSize { get; set; } = 4096;

    /// <summary>
    /// \if KO
    /// <para>송신 버퍼 크기(바이트)를 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the send buffer size in bytes.</para>
    /// \endif
    /// </summary>
    public int WriteBufferSize { get; set; } = 4096;
}
