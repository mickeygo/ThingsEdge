using ThingsEdge.Communication.Profinet.Siemens;

namespace ThingsEdge.Communication.Tests.Profinet.Siemens;

/// <summary>
/// SiemensS7Net 测试
/// </summary>
public class SiemensS7NetTests
{
    /// <summary>
    /// S7 读写测试
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task Should_WriteAndRead_Test()
    {
        SiemensS7Net client = new(SiemensPLCS.S1500, "127.0.0.1");
        var connectResult = await client.ConnectServerAsync();
        Assert.True(connectResult.IsSuccess, connectResult.Message);

        // bool
        var shortAddress = "DB100.12";
        var shortValue = (short)254;
        var shortResult1 = await client.WriteAsync(shortAddress, shortValue);
        Assert.True(shortResult1.IsSuccess, shortResult1.Message);
        var shortResult2 = await client.ReadInt16Async(shortAddress);
        Assert.True(shortResult2.IsSuccess, shortResult2.Message);
        Assert.Equal(shortValue, shortResult2.Content);

        // short

        // int

        // float

        // long

        // double

        // string

        // WString
    }
}
