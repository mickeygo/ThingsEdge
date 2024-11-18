using ThingsEdge.Communication.Common;

namespace ThingsEdge.Communication.Profinet.Delta.Helper;

/// <summary>
/// 台达的想关的辅助类
/// </summary>
public static class DeltaHelper
{
    internal static OperateResult<string> TranslateToModbusAddress(IDelta delta, string address, byte modbusCode)
    {
        return delta.Series switch
        {
            DeltaSeries.Dvp => DeltaDvpHelper.ParseDeltaDvpAddress(address, modbusCode),
            DeltaSeries.AS => DeltaASHelper.ParseDeltaASAddress(address, modbusCode),
            _ => new OperateResult<string>(StringResources.Language.NotSupportedDataType),
        };
    }

    internal static OperateResult<bool[]> ReadBool(IDelta delta, Func<string, ushort, OperateResult<bool[]>> readBoolFunc, string address, ushort length)
    {
        return delta.Series switch
        {
            DeltaSeries.Dvp => DeltaDvpHelper.ReadBool(readBoolFunc, address, length),
            DeltaSeries.AS => readBoolFunc(address, length),
            _ => new OperateResult<bool[]>(StringResources.Language.NotSupportedDataType),
        };
    }

    internal static OperateResult Write(IDelta delta, Func<string, bool[], OperateResult> writeBoolFunc, string address, bool[] values)
    {
        return delta.Series switch
        {
            DeltaSeries.Dvp => DeltaDvpHelper.Write(writeBoolFunc, address, values),
            DeltaSeries.AS => writeBoolFunc(address, values),
            _ => new OperateResult(StringResources.Language.NotSupportedDataType),
        };
    }

    internal static OperateResult<byte[]> Read(IDelta delta, Func<string, ushort, OperateResult<byte[]>> readFunc, string address, ushort length)
    {
        return delta.Series switch
        {
            DeltaSeries.Dvp => DeltaDvpHelper.Read(readFunc, address, length),
            DeltaSeries.AS => readFunc(address, length),
            _ => new OperateResult<byte[]>(StringResources.Language.NotSupportedDataType),
        };
    }

    internal static OperateResult Write(IDelta delta, Func<string, byte[], OperateResult> writeFunc, string address, byte[] value)
    {
        return delta.Series switch
        {
            DeltaSeries.Dvp => DeltaDvpHelper.Write(writeFunc, address, value),
            DeltaSeries.AS => writeFunc(address, value),
            _ => new OperateResult(StringResources.Language.NotSupportedDataType),
        };
    }

    internal static async Task<OperateResult<bool[]>> ReadBoolAsync(IDelta delta, Func<string, ushort, Task<OperateResult<bool[]>>> readBoolFunc, string address, ushort length)
    {
        return delta.Series switch
        {
            DeltaSeries.Dvp => await DeltaDvpHelper.ReadBoolAsync(readBoolFunc, address, length).ConfigureAwait(false),
            DeltaSeries.AS => await readBoolFunc(address, length).ConfigureAwait(false),
            _ => new OperateResult<bool[]>(StringResources.Language.NotSupportedDataType),
        };
    }

    internal static async Task<OperateResult> WriteAsync(IDelta delta, Func<string, bool[], Task<OperateResult>> writeBoolFunc, string address, bool[] values)
    {
        return delta.Series switch
        {
            DeltaSeries.Dvp => await DeltaDvpHelper.WriteAsync(writeBoolFunc, address, values).ConfigureAwait(false),
            DeltaSeries.AS => await writeBoolFunc(address, values).ConfigureAwait(false),
            _ => new OperateResult(StringResources.Language.NotSupportedDataType),
        };
    }

    internal static async Task<OperateResult<byte[]>> ReadAsync(IDelta delta, Func<string, ushort, Task<OperateResult<byte[]>>> readFunc, string address, ushort length)
    {
        return delta.Series switch
        {
            DeltaSeries.Dvp => await DeltaDvpHelper.ReadAsync(readFunc, address, length).ConfigureAwait(false),
            DeltaSeries.AS => await readFunc(address, length).ConfigureAwait(false),
            _ => new OperateResult<byte[]>(StringResources.Language.NotSupportedDataType),
        };
    }

    internal static async Task<OperateResult> WriteAsync(IDelta delta, Func<string, byte[], Task<OperateResult>> writeFunc, string address, byte[] value)
    {
        return delta.Series switch
        {
            DeltaSeries.Dvp => await DeltaDvpHelper.WriteAsync(writeFunc, address, value).ConfigureAwait(false),
            DeltaSeries.AS => await writeFunc(address, value).ConfigureAwait(false),
            _ => new OperateResult(StringResources.Language.NotSupportedDataType),
        };
    }
}