﻿namespace ThingsEdge.Contracts;

/// <summary>
/// 响应消息。
/// </summary>
public sealed class ResponseMessage
{
    /// <summary>
    /// 加载的数据头。
    /// </summary>
    [NotNull]
    public Schema? Schema { get; set; }

    /// <summary>
    /// 数据回写集合。
    /// Key 为标记名称，Value 为值。
    /// </summary>
    [NotNull]
    public Dictionary<string, object>? CallbackItems { get; set; } = new();
}
