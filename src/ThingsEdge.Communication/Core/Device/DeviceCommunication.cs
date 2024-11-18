using ThingsEdge.Communication.Common;
using ThingsEdge.Communication.Core.Net;
using ThingsEdge.Communication.Reflection;

namespace ThingsEdge.Communication.Core.Device;

/// <summary>
/// 设备通信的基类信息。
/// </summary>
public abstract class DeviceCommunication : BinaryCommunication, IReadWriteDevice, IReadWriteNet, IDisposable
{
    private bool _disposedValue;

    /// <summary>
    /// 一个字单位的数据表示的地址长度，西门子为2，三菱，欧姆龙，modbusTcp就为1，AB PLC无效。
    /// </summary>
    /// <remarks>
    /// 对设备来说，一个地址的数据对应的字节数，或是1个字节或是2个字节，4个字节，通常是这四个选择，当设置为0时，则表示4字节的地址长度信息。
    /// </remarks>
    protected ushort WordLength { get; set; } = 1;

    /// <summary>
    /// 当前的数据变换机制，当你需要从字节数据转换类型数据的时候需要。
    /// </summary>
    /// <remarks>
    /// 提供了三种数据变换机制，分别是 <see cref="RegularByteTransform" />, <see cref="ReverseBytesTransform" />,
    /// <see cref="ReverseWordTransform" />，各自的<see cref="DataFormat" />属性也可以自定调整，基本满足所有的情况使用。
    /// </remarks>
    public IByteTransform ByteTransform { get; set; } = new RegularByteTransform();

    /// <summary>
    /// 一个字单位的数据表示的地址长度，西门子为2，三菱，欧姆龙，modbusTcp就为1，AB PLC无效。
    /// </summary>
    /// <remarks>
    /// 对设备来说，一个地址的数据对应的字节数，或是1个字节或是2个字节，通常是这两个选择。
    /// 当前也可以重写来根据不同的地址动态控制不同的地址长度，比如有的地址是一个地址一个字节的，有的地址是一个地址两个字节的。
    /// </remarks>
    /// <param name="address">读取的设备的地址信息</param>
    /// <param name="length">读取的数据长度信息</param>
    /// <param name="dataTypeLength">数据类型的字节长度信息，比如short, 就是2，int,float就是4</param>
    protected virtual ushort GetWordLength(string address, int length, int dataTypeLength)
    {
        if (WordLength == 0)
        {
            var num = length * dataTypeLength * 2 / 4;
            return (ushort)(num == 0 ? 1 : (ushort)num);
        }
        return (ushort)(WordLength * length * dataTypeLength);
    }

    #region read

    /// <inheritdoc />
    public abstract Task<OperateResult<byte[]>> ReadAsync(string address, ushort length);

    /// <inheritdoc />
    public virtual async Task<OperateResult<bool>> ReadBoolAsync(string address)
    {
        return ByteTransformHelper.GetResultFromArray(await ReadBoolAsync(address, 1).ConfigureAwait(false));
    }

    /// <inheritdoc />
    public abstract Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length);

    /// <inheritdoc />
    public async Task<OperateResult<short>> ReadInt16Async(string address)
    {
        return ByteTransformHelper.GetResultFromArray(await ReadInt16Async(address, 1).ConfigureAwait(false));
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult<short[]>> ReadInt16Async(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, GetWordLength(address, length, 1)).ConfigureAwait(false), (m) => ByteTransform.TransInt16(m, 0, length));
    }

    /// <inheritdoc />
    public async Task<OperateResult<ushort>> ReadUInt16Async(string address)
    {
        return ByteTransformHelper.GetResultFromArray(await ReadUInt16Async(address, 1).ConfigureAwait(false));
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult<ushort[]>> ReadUInt16Async(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, GetWordLength(address, length, 1)).ConfigureAwait(false), (m) => ByteTransform.TransUInt16(m, 0, length));
    }

    /// <inheritdoc />
    public async Task<OperateResult<int>> ReadInt32Async(string address)
    {
        return ByteTransformHelper.GetResultFromArray(await ReadInt32Async(address, 1).ConfigureAwait(false));
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult<int[]>> ReadInt32Async(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, GetWordLength(address, length, 2)).ConfigureAwait(false), (m) => ByteTransform.TransInt32(m, 0, length));
    }

    /// <inheritdoc />
    public async Task<OperateResult<uint>> ReadUInt32Async(string address)
    {
        return ByteTransformHelper.GetResultFromArray(await ReadUInt32Async(address, 1).ConfigureAwait(false));
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult<uint[]>> ReadUInt32Async(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, GetWordLength(address, length, 2)).ConfigureAwait(false), (m) => ByteTransform.TransUInt32(m, 0, length));
    }

    /// <inheritdoc />
    public async Task<OperateResult<long>> ReadInt64Async(string address)
    {
        return ByteTransformHelper.GetResultFromArray(await ReadInt64Async(address, 1).ConfigureAwait(false));
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult<long[]>> ReadInt64Async(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, GetWordLength(address, length, 4)).ConfigureAwait(false), (m) => ByteTransform.TransInt64(m, 0, length));
    }

    /// <inheritdoc />
    public async Task<OperateResult<ulong>> ReadUInt64Async(string address)
    {
        return ByteTransformHelper.GetResultFromArray(await ReadUInt64Async(address, 1).ConfigureAwait(false));
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult<ulong[]>> ReadUInt64Async(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, GetWordLength(address, length, 4)).ConfigureAwait(false), (m) => ByteTransform.TransUInt64(m, 0, length));
    }

    /// <inheritdoc />
    public async Task<OperateResult<float>> ReadFloatAsync(string address)
    {
        return ByteTransformHelper.GetResultFromArray(await ReadFloatAsync(address, 1).ConfigureAwait(false));
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult<float[]>> ReadFloatAsync(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, GetWordLength(address, length, 2)).ConfigureAwait(false), (m) => ByteTransform.TransSingle(m, 0, length));
    }
    
    /// <inheritdoc />
    public async Task<OperateResult<double>> ReadDoubleAsync(string address)
    {
        return ByteTransformHelper.GetResultFromArray(await ReadDoubleAsync(address, 1).ConfigureAwait(false));
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult<double[]>> ReadDoubleAsync(string address, ushort length)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, GetWordLength(address, length, 4)).ConfigureAwait(false), (m) => ByteTransform.TransDouble(m, 0, length));
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult<string>> ReadStringAsync(string address, ushort length)
    {
        return await ReadStringAsync(address, length, Encoding.ASCII).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult<string>> ReadStringAsync(string address, ushort length, Encoding encoding)
    {
        return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length).ConfigureAwait(false), (m) => ByteTransform.TransString(m, 0, m.Length, encoding));
    }
  
    /// <inheritdoc />
    public async Task<OperateResult<T>> ReadCustomerAsync<T>(string address) where T : IDataTransfer, new()
    {
        return await ReadWriteNetHelper.ReadCustomerAsync<T>(this, address).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult<T>> ReadStructAsync<T>(string address, ushort length) where T : class, new()
    {
        return await ReadWriteNetHelper.ReadStructAsync<T>(this, address, length, ByteTransform).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<OperateResult<T>> ReadCustomerAsync<T>(string address, T obj) where T : IDataTransfer, new()
    {
        return await ReadWriteNetHelper.ReadCustomerAsync(this, address, obj).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult<T>> ReadAsync<T>() where T : class, new()
    {
        return await CommReflectionHelper.ReadAsync<T>(this).ConfigureAwait(false);
    }

    #endregion

    #region write

    /// <inheritdoc />
    public abstract Task<OperateResult> WriteAsync(string address, byte[] values);

    /// <inheritdoc />
    public virtual async Task<OperateResult> WriteAsync(string address, bool value)
    {
        return await WriteAsync(address, [value]).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public abstract Task<OperateResult> WriteAsync(string address, bool[] values);

    /// <inheritdoc />
    public virtual async Task<OperateResult> WriteAsync(string address, short value)
    {
        return await WriteAsync(address, [value]).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult> WriteAsync(string address, short[] values)
    {
        return await WriteAsync(address, ByteTransform.TransByte(values)).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult> WriteAsync(string address, ushort value)
    {
        return await WriteAsync(address, [value]).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult> WriteAsync(string address, ushort[] values)
    {
        return await WriteAsync(address, ByteTransform.TransByte(values)).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult> WriteAsync(string address, int value)
    {
        return await WriteAsync(address, [value]).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult> WriteAsync(string address, int[] values)
    {
        return await WriteAsync(address, ByteTransform.TransByte(values)).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult> WriteAsync(string address, uint value)
    {
        return await WriteAsync(address, [value]).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult> WriteAsync(string address, uint[] values)
    {
        return await WriteAsync(address, ByteTransform.TransByte(values)).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult> WriteAsync(string address, float value)
    {
        return await WriteAsync(address, [value]).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult> WriteAsync(string address, float[] values)
    {
        return await WriteAsync(address, ByteTransform.TransByte(values)).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult> WriteAsync(string address, long value)
    {
        return await WriteAsync(address, [value]).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult> WriteAsync(string address, long[] values)
    {
        return await WriteAsync(address, ByteTransform.TransByte(values)).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult> WriteAsync(string address, ulong value)
    {
        return await WriteAsync(address, [value]).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult> WriteAsync(string address, ulong[] values)
    {
        return await WriteAsync(address, ByteTransform.TransByte(values)).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult> WriteAsync(string address, double value)
    {
        return await WriteAsync(address, [value]).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult> WriteAsync(string address, double[] values)
    {
        return await WriteAsync(address, ByteTransform.TransByte(values)).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult> WriteAsync(string address, string value)
    {
        return await WriteAsync(address, value, Encoding.ASCII).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult> WriteAsync(string address, string value, Encoding encoding)
    {
        var temp = ByteTransform.TransByte(value, encoding);
        if (WordLength == 1)
        {
            temp = SoftBasic.ArrayExpandToLengthEven(temp);
        }
        return await WriteAsync(address, temp).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult> WriteAsync(string address, string value, int length)
    {
        return await WriteAsync(address, value, length, Encoding.ASCII).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult> WriteAsync(string address, string value, int length, Encoding encoding)
    {
        var temp = ByteTransform.TransByte(value, encoding);
        if (WordLength == 1)
        {
            temp = SoftBasic.ArrayExpandToLengthEven(temp);
        }
        temp = SoftBasic.ArrayExpandToLength(temp, length);
        return await WriteAsync(address, temp).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<OperateResult> WriteCustomerAsync<T>(string address, T data) where T : IDataTransfer, new()
    {
        return await ReadWriteNetHelper.WriteCustomerAsync(this, address, data).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<OperateResult> WriteAsync<T>(T data) where T : class, new()
    {
        return await CommReflectionHelper.WriteAsync(data, this).ConfigureAwait(false);
    }

    #endregion

    /// <inheritdoc />
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            CommunicationPipe?.CloseCommunication();
            CommunicationPipe?.Dispose();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposedValue)
        {
            Dispose(disposing: true);
            _disposedValue = true;
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"DeviceCommunication<{ByteTransform}>{{{CommunicationPipe}}}";
    }
}
