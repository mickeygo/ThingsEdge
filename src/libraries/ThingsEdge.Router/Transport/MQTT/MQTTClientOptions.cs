﻿namespace ThingsEdge.Router.Transport.MQTT;

public sealed class MQTTClientOptions
{
    /// <summary>
    /// MQTT服务器连接地址，如：mqtt://user:password@127.0.0.1，其中scheme可为tcp、mqtt、mqtts、ws和wss。
    /// </summary>
    [NotNull]
    public string? ConnectionUri { get; set; }

    /// <summary>
    /// 客户端唯一标识。
    /// </summary>
    [NotNull]
    public string? ClientId { get; set; }

    /// <summary>
    /// 订阅的Topic集合。
    /// </summary>
    [NotNull]
    public string[]? Topics { get; set; }
}