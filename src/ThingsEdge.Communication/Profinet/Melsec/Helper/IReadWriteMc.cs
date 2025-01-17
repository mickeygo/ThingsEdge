using ThingsEdge.Communication.Core;
using ThingsEdge.Communication.Core.Address;

namespace ThingsEdge.Communication.Profinet.Melsec.Helper;

/// <summary>
/// 基于MC协议的标准的设备接口，适用任何基于MC协议的PLC设备，主要是三菱，基恩士，松下的PLC设备。
/// </summary>
public interface IReadWriteMc : IReadWriteDevice, IReadWriteNet
{
    /// <summary>
    /// 网络号，通常为0。
    /// </summary>
    /// <remarks>
    /// 依据PLC的配置而配置，如果PLC配置了1，那么此处也填0，如果PLC配置了2，此处就填2，测试不通的话，继续测试0。
    /// </remarks>
    byte NetworkNumber { get; set; }

    /// <summary>
    /// PLC编号，如果是本站信息，则是 0xFF 值，其他站则根据实际的情况指定。
    /// </summary>
    byte PLCNumber { get; set; }

    /// <summary>
    /// 请求目标模块的IO编号，默认是管理CPU，也就是 0x03FF 的值，如果需要访问其他的非管理CPU的时候，请参考手册进行配置相关的值。
    /// </summary>
    ushort TargetIOStation { get; set; }

    /// <summary>
    /// 网络站号，通常为0。
    /// </summary>
    /// <remarks>
    /// 依据PLC的配置而配置，如果PLC配置了1，那么此处也填0，如果PLC配置了2，此处就填2，测试不通的话，继续测试0。
    /// </remarks>
    byte NetworkStationNumber { get; set; }

    /// <summary>
    /// 是否开启支持写入位到字寄存器的功能，该功能先读取字寄存器的字数据，然后修改其中的位，再写入回去，可能存在脏数据的风险。
    /// </summary>
    /// <remarks>
    /// 关于脏数据风险：从读取数据，修改位，再次写入数据时，大概需要经过3ms~10ms不等的时间，
    /// 如果此期间内PLC修改了该字寄存器的其他位，再次写入数据时会恢复该点位的数据到读取时的初始值，可能引发设备故障，请谨慎开启此功能。
    /// </remarks>
    bool EnableWriteBitToWordRegister { get; set; }

    /// <summary>
    /// 当前的MC协议的格式类型。
    /// </summary>
    McType McType { get; }

    /// <summary>
    /// 当前MC协议的分析地址的方法，对传入的字符串格式的地址进行数据解析。
    /// </summary>
    /// <param name="address">地址信息</param>
    /// <param name="length">数据长度</param>
    /// <param name="isBit">当前是否读写bool操作</param>
    /// <returns>解析后的数据信息</returns>
    OperateResult<McAddressData> McAnalysisAddress(string address, ushort length, bool isBit);

    /// <summary>
    /// 从PLC反馈的数据中提取出实际的数据内容，需要传入反馈数据，是否位读取
    /// </summary>
    /// <param name="response">反馈的数据内容</param>
    /// <param name="isBit">是否位读取</param>
    /// <returns>解析后的结果对象</returns>
    byte[] ExtractActualData(byte[] response, bool isBit);

    /// <summary>
    /// 远程Run操作。
    /// </summary>
    /// <returns>是否成功</returns>
    Task<OperateResult> RemoteRunAsync();

    /// <summary>
    /// 远程Stop操作。
    /// </summary>
    /// <returns>是否成功</returns>
    Task<OperateResult> RemoteStopAsync();

    /// <summary>
    /// 远程Reset操作。
    /// </summary>
    /// <returns>是否成功</returns>
    Task<OperateResult> RemoteResetAsync();

    /// <summary>
    /// 读取PLC的型号信息，例如 Q02HCPU。
    /// </summary>
    /// <returns>返回型号的结果对象</returns>
    Task<OperateResult<string>> ReadPlcTypeAsync();

    /// <summary>
    /// LED 熄灭 出错代码初始化。
    /// </summary>
    /// <returns>是否成功</returns>
    Task<OperateResult> ErrorStateResetAsync();
}
