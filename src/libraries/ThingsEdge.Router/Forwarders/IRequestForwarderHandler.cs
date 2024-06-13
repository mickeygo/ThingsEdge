﻿namespace ThingsEdge.Router.Forwarders;

/// <summary>
/// 表示为发送请求服务处理接口，其中 <see cref="TagFlag.Trigger"/> 会发布此事件。
/// </summary>
public interface IRequestForwarderHandler
{
    /// <summary>
    /// 处理请求数据。
    /// </summary>
    /// <param name="message">请求的数据</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ResponseMessage> HandleAsync(RequestMessage message, CancellationToken cancellationToken = default);
}
