using System;
using Dreamine.Communication.Abstractions.Exceptions;

namespace Dreamine.Communication.Serial.Exceptions;

/// <summary>
/// \brief 시리얼 통신 계층에서 발생하는 예외입니다.
/// </summary>
public sealed class SerialCommunicationException : CommunicationException
{
    /// <summary>
    /// \brief SerialCommunicationException 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    public SerialCommunicationException()
    {
    }

    /// <summary>
    /// \brief 지정한 오류 메시지를 사용하여 SerialCommunicationException 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="message">오류 메시지입니다.</param>
    public SerialCommunicationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// \brief 지정한 오류 메시지와 내부 예외를 사용하여 SerialCommunicationException 클래스의 새 인스턴스를 초기화합니다.
    /// </summary>
    /// <param name="message">오류 메시지입니다.</param>
    /// <param name="innerException">내부 예외입니다.</param>
    public SerialCommunicationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}