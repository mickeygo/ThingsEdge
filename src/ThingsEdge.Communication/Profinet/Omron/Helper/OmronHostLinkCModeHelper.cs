using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core;

namespace ThingsEdge.Communication.Profinet.Omron.Helper;

/// <summary>
/// 欧姆龙的OmronHostLinkCMode的辅助类方法
/// </summary>
public class OmronHostLinkCModeHelper
{
    /// <summary>
    /// 读取数据。
    /// </summary>
    /// <remarks>
    /// 地址里可以额外指定单元号信息，例如 s=2;D100。
    /// </remarks>
    public static async Task<OperateResult<byte[]>> ReadAsync(IReadWriteDevice omron, byte unitNumber, string address, ushort length)
    {
        var station = (byte)CommHelper.ExtractParameter(ref address, "s", unitNumber);
        var command = BuildReadCommand(address, length);
        if (!command.IsSuccess)
        {
            return OperateResult.CreateFailedResult<byte[]>(command);
        }
        var array = new List<byte>();
        for (var i = 0; i < command.Content.Count; i++)
        {
            var read = await omron.ReadFromCoreServerAsync(PackCommand(command.Content[i], station)).ConfigureAwait(false);
            if (!read.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(read);
            }
            var valid = ResponseValidAnalysis(read.Content, isRead: true);
            if (!valid.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(valid);
            }
            array.AddRange(valid.Content);
        }
        return OperateResult.CreateSuccessResult(array.ToArray());
    }

    /// <summary>
    /// 写入数据。
    /// </summary>
    /// <remarks>
    /// 地址里可以额外指定单元号信息，例如 s=2;D100。
    /// </remarks>
    public static async Task<OperateResult> WriteAsync(IReadWriteDevice omron, byte unitNumber, string address, byte[] value)
    {
        var station = (byte)CommHelper.ExtractParameter(ref address, "s", unitNumber);
        var command = BuildWriteWordCommand(address, value);
        if (!command.IsSuccess)
        {
            return command;
        }

        for (var i = 0; i < command.Content.Count; i++)
        {
            var read = await omron.ReadFromCoreServerAsync(PackCommand(command.Content[i], station)).ConfigureAwait(false);
            if (!read.IsSuccess)
            {
                return read;
            }
            var valid = ResponseValidAnalysis(read.Content, isRead: false);
            if (!valid.IsSuccess)
            {
                return valid;
            }
        }
        return OperateResult.CreateSuccessResult();
    }

    /// <summary>
    /// 读取PLC的当前的型号信息。
    /// </summary>
    /// <param name="omron">PLC连接对象</param>
    /// <param name="unitNumber">站号信息</param>
    /// <returns>型号</returns>
    public static async Task<OperateResult<string>> ReadPlcTypeAsync(IReadWriteDevice omron, byte unitNumber)
    {
        var operateResult = await omron.ReadFromCoreServerAsync(PackCommand(Encoding.ASCII.GetBytes("MM"), unitNumber)).ConfigureAwait(false);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<string>(operateResult);
        }
        var num = Convert.ToInt32(Encoding.ASCII.GetString(operateResult.Content, 5, 2), 16);
        if (num > 0)
        {
            return new OperateResult<string>(num, "Unknown Error");
        }
        var @string = Encoding.ASCII.GetString(operateResult.Content, 7, 2);
        return GetModelText(@string);
    }

    /// <summary>
    ///  读取PLC当前的操作模式，0: 编程模式  1: 运行模式  2: 监视模式。
    /// </summary>
    /// <param name="omron">PLC连接对象</param>
    /// <param name="unitNumber">站号信息</param>
    /// <returns>0: 编程模式  1: 运行模式  2: 监视模式</returns>
    public static async Task<OperateResult<int>> ReadPlcModeAsync(IReadWriteDevice omron, byte unitNumber)
    {
        OperateResult<byte[]> operateResult = await omron.ReadFromCoreServerAsync(PackCommand(Encoding.ASCII.GetBytes("MS"), unitNumber)).ConfigureAwait(false);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<int>(operateResult);
        }
        var num = Convert.ToInt32(Encoding.ASCII.GetString(operateResult.Content, 5, 2), 16);
        if (num > 0)
        {
            return new OperateResult<int>(num, "Unknown Error");
        }
        var array = Encoding.ASCII.GetString(operateResult.Content, 7, 4).ToHexBytes();
        return OperateResult.CreateSuccessResult(array[0] & 3);
    }

    /// <summary>
    /// 将当前PLC的模式变更为指定的模式，0: 编程模式  1: 运行模式  2: 监视模式。
    /// </summary>
    /// <param name="omron">PLC连接对象</param>
    /// <param name="unitNumber">站号信息</param>
    /// <param name="mode">0: 编程模式  1: 运行模式  2: 监视模式</param>
    /// <returns>是否变更成功</returns>
    public static async Task<OperateResult> ChangePlcModeAsync(IReadWriteDevice omron, byte unitNumber, byte mode)
    {
        var operateResult = await omron.ReadFromCoreServerAsync(PackCommand(Encoding.ASCII.GetBytes("SC" + mode.ToString("X2")), unitNumber)).ConfigureAwait(false);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<int>(operateResult);
        }
        var num = Convert.ToInt32(Encoding.ASCII.GetString(operateResult.Content, 5, 2), 16);
        if (num > 0)
        {
            return new OperateResult<int>(num, "Unknown Error");
        }
        return OperateResult.CreateSuccessResult();
    }

    private static OperateResult<string, int> GetEMAddress(string address, int start, bool isRead)
    {
        var array = address.SplitDot();
        var num = Convert.ToInt32(array[0][start..], 16);
        return OperateResult.CreateSuccessResult((isRead ? "RE" : "WE") + Encoding.ASCII.GetString(ByteExtensions.BuildAsciiBytesFrom((byte)num)), (int)ushort.Parse(array[1]));
    }

    /// <summary>
    /// 解析欧姆龙的数据地址，参考来源是Omron手册第188页，比如D100， E1.100。
    /// </summary>
    /// <param name="address">数据地址</param>
    /// <param name="isRead">是否读取</param>
    /// <returns>解析后的结果地址对象</returns>
    private static OperateResult<string, int> AnalysisAddress(string address, bool isRead)
    {
        try
        {
            if (address.StartsWith("DM", StringComparison.OrdinalIgnoreCase))
            {
                return OperateResult.CreateSuccessResult(isRead ? "RD" : "WD", (int)ushort.Parse(address[2..]));
            }
            if (address.StartsWith("LR", StringComparison.OrdinalIgnoreCase))
            {
                return OperateResult.CreateSuccessResult(isRead ? "RL" : "WL", (int)ushort.Parse(address[2..]));
            }
            if (address.StartsWith("HR", StringComparison.OrdinalIgnoreCase))
            {
                return OperateResult.CreateSuccessResult(isRead ? "RH" : "WH", (int)ushort.Parse(address[2..]));
            }
            if (address.StartsWith("AR", StringComparison.OrdinalIgnoreCase))
            {
                return OperateResult.CreateSuccessResult(isRead ? "RJ" : "WJ", (int)ushort.Parse(address[2..]));
            }
            if (address.StartsWith("CIO", StringComparison.OrdinalIgnoreCase))
            {
                return OperateResult.CreateSuccessResult(isRead ? "RR" : "WR", (int)ushort.Parse(address[3..]));
            }
            if (address.StartsWith("TIM", StringComparison.OrdinalIgnoreCase))
            {
                return OperateResult.CreateSuccessResult(isRead ? "RC" : "WC", (int)ushort.Parse(address[3..]));
            }
            if (address.StartsWith("CNT", StringComparison.OrdinalIgnoreCase))
            {
                return OperateResult.CreateSuccessResult(isRead ? "RC" : "WC", ushort.Parse(address[3..]) + 2048);
            }
            if (address.StartsWith("EM", StringComparison.OrdinalIgnoreCase))
            {
                return GetEMAddress(address, 2, isRead);
            }
            return address[0] switch
            {
                'D' or 'd' => OperateResult.CreateSuccessResult(isRead ? "RD" : "WD", (int)ushort.Parse(address[1..])),
                'C' or 'c' => OperateResult.CreateSuccessResult(isRead ? "RR" : "WR", (int)ushort.Parse(address[1..])),
                'H' or 'h' => OperateResult.CreateSuccessResult(isRead ? "RH" : "WH", (int)ushort.Parse(address[1..])),
                'A' or 'a' => OperateResult.CreateSuccessResult(isRead ? "RJ" : "WJ", (int)ushort.Parse(address[1..])),
                'E' or 'e' => GetEMAddress(address, 1, isRead),
                _ => throw new CommunicationException(StringResources.Language.NotSupportedDataType),
            };
        }
        catch (Exception ex)
        {
            return new OperateResult<string, int>(ex.Message);
        }
    }

    /// <summary>
    /// 根据读取的地址，长度，是否位读取创建Fins协议的核心报文。
    /// </summary>
    /// <param name="address">地址，具体格式请参照示例说明</param>
    /// <param name="length">读取的数据长度</param>
    /// <returns>带有成功标识的Fins核心报文</returns>
    private static OperateResult<List<byte[]>> BuildReadCommand(string address, ushort length)
    {
        var operateResult = AnalysisAddress(address, isRead: true);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<List<byte[]>>(operateResult);
        }

        var array = CollectionUtils.SplitIntegerToArray(length, 30);
        var list = new List<byte[]>();
        for (var i = 0; i < array.Length; i++)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(operateResult.Content1);
            stringBuilder.Append(operateResult.Content2.ToString("D4"));
            stringBuilder.Append(array[i].ToString("D4"));
            list.Add(Encoding.ASCII.GetBytes(stringBuilder.ToString()));
            operateResult.Content2 += array[i];
        }
        return OperateResult.CreateSuccessResult(list);
    }

    /// <summary>
    /// 根据读取的地址，长度，是否位读取创建Fins协议的核心报文。
    /// </summary>
    /// <param name="address">地址，具体格式请参照示例说明</param>
    /// <param name="value">等待写入的数据</param>
    /// <returns>带有成功标识的Fins核心报文</returns>
    private static OperateResult<List<byte[]>> BuildWriteWordCommand(string address, byte[] value)
    {
        var operateResult = AnalysisAddress(address, false);
        if (!operateResult.IsSuccess)
        {
            return OperateResult.CreateFailedResult<List<byte[]>>(operateResult);
        }

        var list = CollectionUtils.SplitByLength(value, 60);
        var list2 = new List<byte[]>();
        for (var i = 0; i < list.Count; i++)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(operateResult.Content1);
            stringBuilder.Append(operateResult.Content2.ToString("D4"));
            if (list[i].Length != 0)
            {
                stringBuilder.Append(list[i].ToHexString());
            }
            list2.Add(Encoding.ASCII.GetBytes(stringBuilder.ToString()));
            operateResult.Content2 += list[i].Length / 2;
        }
        return OperateResult.CreateSuccessResult(list2);
    }

    /// <summary>
    /// 验证欧姆龙的Fins-TCP返回的数据是否正确的数据，如果正确的话，并返回所有的数据内容。
    /// </summary>
    /// <param name="response">来自欧姆龙返回的数据内容</param>
    /// <param name="isRead">是否读取</param>
    /// <returns>带有是否成功的结果对象</returns>
    private static OperateResult<byte[]> ResponseValidAnalysis(byte[] response, bool isRead)
    {
        if (response.Length >= 11)
        {
            try
            {
                var num = Convert.ToInt32(Encoding.ASCII.GetString(response, 5, 2), 16);
                byte[] array = [];
                if (response.Length > 11)
                {
                    array = Encoding.ASCII.GetString(response, 7, response.Length - 11).ToHexBytes();
                }
                if (num > 0)
                {
                    return new OperateResult<byte[]>
                    {
                        ErrorCode = num,
                        Message = GetErrorMessage(num),
                        Content = array
                    };
                }
                return OperateResult.CreateSuccessResult(array);
            }
            catch (Exception ex)
            {
                return new OperateResult<byte[]>("ResponseValidAnalysis failed: " + ex.Message + " Source: " + response.ToHexString(' '));
            }
        }
        return new OperateResult<byte[]>(StringResources.Language.OmronReceiveDataError);
    }

    /// <summary>
    /// 将普通的指令打包成完整的指令
    /// </summary>
    /// <param name="cmd">fins指令</param>
    /// <param name="unitNumber">站号信息</param>
    /// <returns>完整的质量</returns>
    private static byte[] PackCommand(byte[] cmd, byte unitNumber)
    {
        var array = new byte[7 + cmd.Length];
        array[0] = 64;
        array[1] = ByteExtensions.BuildAsciiBytesFrom(unitNumber)[0];
        array[2] = ByteExtensions.BuildAsciiBytesFrom(unitNumber)[1];
        array[^2] = 42;
        array[^1] = 13;
        cmd.CopyTo(array, 3);
        int num = array[0];
        for (var i = 1; i < array.Length - 4; i++)
        {
            num ^= array[i];
        }
        array[^4] = ByteExtensions.BuildAsciiBytesFrom((byte)num)[0];
        array[^3] = ByteExtensions.BuildAsciiBytesFrom((byte)num)[1];
        return array;
    }

    /// <summary>
    /// 获取model的字符串描述信息
    /// </summary>
    /// <param name="model">型号代码</param>
    /// <returns>是否解析成功</returns>
    private static OperateResult<string> GetModelText(string model)
    {
        return model switch
        {
            "30" => OperateResult.CreateSuccessResult("CS/CJ"),
            "01" => OperateResult.CreateSuccessResult("C250"),
            "02" => OperateResult.CreateSuccessResult("C500"),
            "03" => OperateResult.CreateSuccessResult("C120/C50"),
            "09" => OperateResult.CreateSuccessResult("C250F"),
            "0A" => OperateResult.CreateSuccessResult("C500F"),
            "0B" => OperateResult.CreateSuccessResult("C120F"),
            "0E" => OperateResult.CreateSuccessResult("C2000"),
            "10" => OperateResult.CreateSuccessResult("C1000H"),
            "11" => OperateResult.CreateSuccessResult("C2000H/CQM1/CPM1"),
            "12" => OperateResult.CreateSuccessResult("C20H/C28H/C40H, C200H, C200HS, C200HX/HG/HE (-ZE)"),
            "20" => OperateResult.CreateSuccessResult("CV500"),
            "21" => OperateResult.CreateSuccessResult("CV1000"),
            "22" => OperateResult.CreateSuccessResult("CV2000"),
            "40" => OperateResult.CreateSuccessResult("CVM1-CPU01-E"),
            "41" => OperateResult.CreateSuccessResult("CVM1-CPU11-E"),
            "42" => OperateResult.CreateSuccessResult("CVM1-CPU21-E"),
            _ => new OperateResult<string>("Unknown model, model code:" + model),
        };
    }

    /// <summary>
    /// 根据错误码的信息，返回错误的具体描述的文本。
    /// </summary>
    /// <param name="err">错误码</param>
    /// <returns>错误的描述文本</returns>
    private static string GetErrorMessage(int err)
    {
        return err switch
        {
            1 => "Not executable in RUN mode",
            2 => "Not executable in MONITOR mode",
            3 => "UM write-protected",
            4 => "Address over: The program address setting in an read or write command is above the highest program address.",
            11 => "Not executable in PROGRAM mode",
            19 => "The FCS is wrong.",
            20 => "The command format is wrong, or a command that cannot be divided has been divided, or the frame length is smaller than the minimum length for the applicable command.",
            21 => "1. The data is outside of the specified range or too long. 2.Hexadecimal data has not been specified.",
            22 => "Command not supported: The operand specified in an SV Read or SV Change command does not exist in the program.",
            24 => "Frame length error: The maximum frame length of 131 bytes was exceeded.",
            25 => "Not executable: The read SV exceeded 9,999, or an I/O memory batch read was executed when items to read were not registered for composite command, or access right was not obtained.",
            32 => "Could not create I/O table",
            33 => "Not executable due to CPU Unit CPU error( See note.)",
            35 => "User memory protected, The UM is read-protected or writeprotected.",
            163 => "Aborted due to FCS error in transmission data",
            164 => "Aborted due to format error in transmission data",
            165 => "Aborted due to entry number data error in transmission data",
            168 => "Aborted due to frame length error in transmission data",
            _ => StringResources.Language.UnknownError,
        };
    }
}
