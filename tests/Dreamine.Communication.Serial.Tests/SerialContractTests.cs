using System.IO.Ports;
using Dreamine.Communication.Abstractions.Enums;
using Dreamine.Communication.Serial.Exceptions;
using Dreamine.Communication.Serial.Options;
using Dreamine.Communication.Serial.Ports;
using Xunit;

namespace Dreamine.Communication.Serial.Tests;

public sealed class SerialContractTests
{
    [Fact]
    public void OptionsExposeStableDefaults()
    {
        var options = new SerialPortTransportOptions();

        Assert.Equal("COM1", options.PortName);
        Assert.Equal(9600, options.BaudRate);
        Assert.Equal(8, options.DataBits);
        Assert.Equal(Parity.None, options.Parity);
        Assert.Equal(StopBits.One, options.StopBits);
        Assert.Equal(Handshake.None, options.Handshake);
        Assert.Equal(3000, options.ReadTimeoutMs);
        Assert.Equal(3000, options.WriteTimeoutMs);
        Assert.Equal(4096, options.ReadBufferSize);
        Assert.Equal(4096, options.WriteBufferSize);
    }

    [Fact]
    public void TransportStartsDisconnectedAndIdentifiesAsSerial()
    {
        var transport = new SerialPortTransport(new SerialPortTransportOptions());

        Assert.Equal(ConnectionState.Disconnected, transport.State);
        Assert.Equal(TransportKind.Serial, transport.Kind);
    }

    [Fact]
    public void TransportRejectsMissingPortName()
    {
        var options = new SerialPortTransportOptions { PortName = "" };

        Assert.Throws<ArgumentException>(() => new SerialPortTransport(options));
    }

    [Theory]
    [InlineData(0, 8)]
    [InlineData(9600, 4)]
    [InlineData(9600, 9)]
    public void TransportRejectsInvalidLineSettings(int baudRate, int dataBits)
    {
        var options = new SerialPortTransportOptions
        {
            BaudRate = baudRate,
            DataBits = dataBits
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => new SerialPortTransport(options));
    }

    [Fact]
    public void SerialExceptionPreservesMessageAndInnerException()
    {
        var inner = new IOException("port failed");
        var error = new SerialCommunicationException("serial failed", inner);

        Assert.Equal("serial failed", error.Message);
        Assert.Same(inner, error.InnerException);
    }
}
