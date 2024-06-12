﻿namespace ThingsEdge.Router.Forwarder;

internal sealed class InternalForwarderFactory : IForwarderFactory, ISingletonDependency
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public InternalForwarderFactory(IServiceProvider serviceProvider, ILogger<InternalForwarderFactory> logger) 
        => (_serviceProvider, _logger) = (serviceProvider, logger);

    public async Task<ResponseResult> SendAsync(RequestMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            //var forwarders = InternalForwarderHub.Default.ResloveAll(_serviceProvider);

            var keys = ForwarderRegisterKeys.Default.Keys;
            var forwarders = keys.Select(key => _serviceProvider.GetKeyedService<IForwarder>(key))
                .Where(forwarder => forwarder != null)
                .ToArray();
            
            if (forwarders.Length > 0)
            {
                var tasks = forwarders.Select(s => s!.SendAsync(message, cancellationToken));
                var results = await Task.WhenAll(tasks).ConfigureAwait(false); // 等待所有任务结束

                // 只返回状态的第一个结果，按 Native > HTTP > MQTT 优先级选取
                var nativeResult = results.FirstOrDefault(s => s.Source == ForworderSource.Native);
                if (nativeResult is not null)
                {
                    return nativeResult;
                }

                var httpResult = results.FirstOrDefault(s => s.Source == ForworderSource.HTTP);
                if (httpResult is not null)
                {
                    return httpResult;
                }

                var mqttResult = results.FirstOrDefault(s => s.Source == ForworderSource.MQTT);
                if (mqttResult is not null)
                {
                    return mqttResult;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[InternalForwarderFactory] 数据发送异常。");
        }

        // 返回空的状态
        return ResponseResult.FromOk(new ResponseMessage
        {
            Request = message,
        });
    }
}
