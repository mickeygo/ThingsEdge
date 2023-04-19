﻿namespace ThingsEdge.Providers.Ops.Exchange;

public static class DriverConnectorExtensions
{
    /// <summary>
    /// 驱动连接器读取数据。
    /// </summary>
    /// <param name="connector">设备连接器。</param>
    /// <param name="tag">要读取的数据标记。</param>
    /// <returns></returns>
    public static async Task<(bool ok, PayloadData data, string err)> ReadAsync(this DriverConnector connector, Tag tag)
    {
        return await DriverReadWriteUtil.ReadAsync(connector.Driver, tag).ConfigureAwait(false);
    }

    /// <summary>
    /// 驱动连接器写入数据。
    /// 注：写入数据前可进行数据根式化。
    /// </summary>
    /// <param name="connector">设备连接器。</param>
    /// <param name="tag">要写入的标记。</param>
    /// <param name="data">要写入的数据。</param>
    /// <param name="format">是否写入前格式化数据。</param>
    /// <returns></returns>
    /// <remarks>要写入的数据必须与标记的数据类型匹配，或是可转换为标记设定的类型。</remarks>
    public static async Task<(bool ok, string? err)> WriteAsync(this DriverConnector connector, Tag tag, object data, bool format = true)
    {
        try
        {
            object? data2 = data;
            if (format)
            {
                (var ok1, data2, string? err1) = TagFormater.Format(tag, data);
                if (!ok1)
                {
                    return (false, err1);
                }
            }

            return await DriverReadWriteUtil.WriteAsync(connector.Driver, tag, data2!).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}