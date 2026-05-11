using System.IO.Ports;

namespace Dreamine.Communication.Serial.Options;

/// <summary>
/// \brief RS232 시리얼 포트 전송 계층 설정입니다.
/// </summary>
public sealed class SerialPortTransportOptions
{
    /// <summary>
    /// \brief 시리얼 포트 이름입니다.
    /// </summary>
    public string PortName { get; set; } = "COM1";

    /// <summary>
    /// \brief Baud rate입니다.
    /// </summary>
    public int BaudRate { get; set; } = 9600;

    /// <summary>
    /// \brief 데이터 비트 수입니다.
    /// </summary>
    public int DataBits { get; set; } = 8;

    /// <summary>
    /// \brief 패리티 설정입니다.
    /// </summary>
    public Parity Parity { get; set; } = Parity.None;

    /// <summary>
    /// \brief Stop bit 설정입니다.
    /// </summary>
    public StopBits StopBits { get; set; } = StopBits.One;

    /// <summary>
    /// \brief Handshake 설정입니다.
    /// </summary>
    public Handshake Handshake { get; set; } = Handshake.None;

    /// <summary>
    /// \brief 읽기 타임아웃(ms)입니다.
    /// </summary>
    public int ReadTimeoutMs { get; set; } = 3000;

    /// <summary>
    /// \brief 쓰기 타임아웃(ms)입니다.
    /// </summary>
    public int WriteTimeoutMs { get; set; } = 3000;

    /// <summary>
    /// \brief 수신 버퍼 크기입니다.
    /// </summary>
    public int ReadBufferSize { get; set; } = 4096;

    /// <summary>
    /// \brief 송신 버퍼 크기입니다.
    /// </summary>
    public int WriteBufferSize { get; set; } = 4096;
}