﻿using ThingsEdge.Providers.Ops.Configuration;
using ThingsEdge.Providers.Ops.Events;
using ThingsEdge.Router.Events;

namespace ThingsEdge.Providers.Ops.Exchange.Monitors;

/// <summary>
/// 触发监控
/// </summary>
internal sealed class TriggerMonitor : AbstractMonitor, ITransientDependency
{
    private readonly IProducer _producer;
    private readonly OpsConfig _opsConfig;
    private readonly ILogger _logger;

    public TriggerMonitor(IProducer producer,
        IOptionsMonitor<OpsConfig> opsConfig,
        ILogger<TriggerMonitor> logger)
    {
        _producer = producer;
        _opsConfig = opsConfig.CurrentValue;
        _logger = logger;
    }

    public override void Monitor(IDriverConnector connector, string channelName, Device device, CancellationToken cancellationToken)
    {
        var tags = device.GetAllTags(TagFlag.Trigger);
        foreach (var tag in tags)
        {
            _ = Task.Run(async () =>
            {
                int pollingInterval = tag.ScanRate > 0 ? tag.ScanRate : _opsConfig.DefaultScanRate;
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(pollingInterval, cancellationToken).ConfigureAwait(false);

                        // 第一次检测
                        if (cancellationToken .IsCancellationRequested)
                        {
                            break;
                        }

                        if (!connector.CanConnect)
                        {
                            continue;
                        }

                        // 若读取失败，该信号点不会复位，下次会继续读取执行。
                        var (ok, data, err) = await connector.ReadAsync(tag).ConfigureAwait(false); // short 类型
                        if (!ok)
                        {
                            string msg = $"[TriggerMonitor] Trigger 数据读取异常，设备：{device.Name}，标记：{tag.Name}, 地址：{tag.Address}，错误：{err}";
                            _logger.LogError(msg);
                            await _producer.ProduceAsync(LoggingMessageEvent.Error(msg)).ConfigureAwait(false);

                            continue;
                        }

                        // 校验触发标记
                        var state = data!.GetInt(); // 触发标记还可能包含状态码信息。

                        // 必须先检测并更新标记状态值，若值有变动且触发标记值为 1 则推送数据。
                        if (!TagValueSet.CompareAndSwap(tag.TagId, state) && state == 1)
                        {
                            // 发布触发事件
                            await _producer.ProduceAsync(new TriggerEvent
                            {
                                Connector = connector,
                                ChannelName = channelName,
                                Device = device,
                                Tag = tag,
                                Self = data,
                            }).ConfigureAwait(false);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        string msg = $"[TriggerMonitor] Trigger 数据处理异常，设备：{device.Name}，标记：{tag.Name}, 地址：{tag.Address}";
                        _logger.LogError(ex, msg);
                        await _producer.ProduceAsync(LoggingMessageEvent.Error(msg)).ConfigureAwait(false);
                    }
                }
            });
        }
    }
}
