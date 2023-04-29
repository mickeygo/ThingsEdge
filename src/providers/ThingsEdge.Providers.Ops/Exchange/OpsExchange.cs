﻿using OneOf.Types;
using ThingsEdge.Common.EventBus;
using ThingsEdge.Contracts.Devices;
using ThingsEdge.Providers.Ops.Configuration;
using ThingsEdge.Providers.Ops.Handlers;
using ThingsEdge.Router;
using ThingsEdge.Router.Events;
using ThingsEdge.Router.Model;

namespace ThingsEdge.Providers.Ops.Exchange;

/// <summary>
/// 信息采集引擎。
/// </summary>
public sealed class OpsExchange : IExchange
{
    private CancellationTokenSource? _cts = new();

    private readonly IEventPublisher _publisher;
    private readonly IDeviceManager _deviceManager;
    private readonly DriverConnectorManager _driverConnectorManager;
    private readonly OpsConfig _opsConfig;
    private readonly ILogger _logger;

    public OpsExchange(IEventPublisher publisher,
        IDeviceManager deviceManager,
        DriverConnectorManager driverConnectorManager,
        IOptionsMonitor<OpsConfig> opsConfig,
        ILogger<OpsExchange> logger)
    {
        _publisher = publisher;
        _deviceManager = deviceManager;
        _driverConnectorManager = driverConnectorManager;
        _opsConfig = opsConfig.CurrentValue;
        _logger = logger;
    }

    public bool IsRunning { get; private set; }

    public async Task StartAsync()
    {
        if (IsRunning)
        {
            return;
        }
        IsRunning = true;

        _logger.LogInformation("[Engine] 引擎启动");

        _cts ??= new();

        var devices = _deviceManager.GetDevices();
        _driverConnectorManager.Load(devices);
        await _driverConnectorManager.ConnectAsync().ConfigureAwait(false);

        foreach (var connector in _driverConnectorManager.GetAllDriver())
        {
            // 心跳数据监控器
            _ = HeartbeatMonitorAsync(connector);

            // 触发数据监控器
            _ = TriggerMonitorAsync(connector);

            // 通知数据监控器
            _ = NoticeMonitorAsync(connector);

            // 开关数据监控器
            _ = SwitchMonitorAsync(connector);
        }
    }

    private Task HeartbeatMonitorAsync(DriverConnector connector)
    {
        var (channelName, device) = _deviceManager.GetDevice2(connector.Id);
        var tags = device!.GetAllTags(TagFlag.Heartbeat); // 所有标记为心跳的都进行监控。
        foreach (var tag in tags)
        {
            _ = Task.Run(async () =>
            {
                int pollingInterval = tag.ScanRate > 0 ? tag.ScanRate : _opsConfig.DefaultScanRate;
                while (_cts != null && !_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(pollingInterval, _cts.Token).ConfigureAwait(false);

                        if (_cts == null)
                        {
                            // 发布设备心跳断开事件。
                            await _publisher.Publish(new DeviceHeartbeatEvent
                            {
                                ChannelName = channelName,
                                Device = device,
                                Tag = tag,
                                ConnectState = DeviceConnectState.Offline,
                            }, PublishStrategy.AsyncContinueOnException).ConfigureAwait(false);

                            break;
                        }

                        if (!connector.CanConnect)
                        {
                            // 发布设备心跳断开事件。
                            await _publisher.Publish(new DeviceHeartbeatEvent
                            {
                                ChannelName = channelName,
                                Device = device,
                                Tag = tag,
                                ConnectState = DeviceConnectState.Offline,
                            }, PublishStrategy.AsyncContinueOnException).ConfigureAwait(false);

                            continue;
                        }

                        var (ok, data, err) = await connector.ReadAsync(tag).ConfigureAwait(false);
                        if (!ok)
                        {
                            _logger.LogError("[Engine] Heartbeat 数据读取异常，设备：{DeviceName}，标记：{TagName}, 地址：{TagAddress}，错误：{Err}",
                                  device.Name, tag.Name, tag.Address, err);

                            continue;
                        }

                        // 心跳标记数据类型必须为 bool 或 int16
                        bool on = tag.DataType switch
                        {
                            DataType.Bit => data.GetBit(),
                            DataType.Int => data.GetInt() == 1,
                            _ => throw new NotSupportedException(),
                        };

                        if (on)
                        {
                            if (tag.DataType == DataType.Bit)
                            {
                                await connector.WriteAsync(tag, false).ConfigureAwait(false);
                            }
                            else if (tag.DataType == DataType.Int)
                            {
                                await connector.WriteAsync(tag, (short)0).ConfigureAwait(false);
                            }

                            // 发布心跳正常事件。
                            await _publisher.Publish(new DeviceHeartbeatEvent
                            {
                                ChannelName = channelName,
                                Device = device,
                                Tag = tag,
                                ConnectState = DeviceConnectState.Online,
                            }, PublishStrategy.AsyncContinueOnException).ConfigureAwait(false);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[Engine] Heartbeat 数据处理异常，设备：{DeviceName}，标记：{TagName}, 地址：{TagAddress}",
                            device.Name, tag.Name, tag.Address);
                    }
                }
            });
        }

        return Task.CompletedTask;
    }

    private Task TriggerMonitorAsync(DriverConnector connector)
    {
        var (channelName, device) = _deviceManager.GetDevice2(connector.Id);
        var tags = device!.GetAllTags(TagFlag.Trigger);
        foreach (var tag in tags)
        {
            _ = Task.Run(async () =>
            {
                int pollingInterval = tag.ScanRate > 0 ? tag.ScanRate : _opsConfig.DefaultScanRate;
                while (_cts != null && !_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(pollingInterval, _cts.Token).ConfigureAwait(false);

                        if (_cts == null)
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
                            _logger.LogError("[Engine] Trigger 数据读取异常，设备：{DeviceName}，标记：{TagName}, 地址：{TagAddress}，错误：{Err}",
                                device.Name, tag.Name, tag.Address, err);

                            continue;
                        }

                        // 校验触发标记
                        var state = data.GetInt();

                        // 检测标记状态是否有变动
                        if (!TagValueSet.CompareAndSwap(tag.TagId, state))
                        {
                            // 推送数据
                            if (state == 1)
                            {
                                // 发布触发事件
                                await _publisher.Publish(new TriggerEvent
                                {
                                    Connector = connector,
                                    ChannelName = channelName,
                                    Device = device,
                                    Tag = tag,
                                    Self = data,
                                }).ConfigureAwait(false);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[Engine] Trigger 数据处理异常，设备：{DeviceName}，变量：{TagName}, 地址：{TagAddress}", 
                            device.Name, tag.Name, tag.Address);
                    }
                }
            });
        }

        return Task.CompletedTask;
    }

    private Task NoticeMonitorAsync(DriverConnector connector)
    {
        var (channelName, device) = _deviceManager.GetDevice2(connector.Id);
        var tags = device!.GetAllTags(TagFlag.Notice);
        foreach (var tag in tags)
        {
            _ = Task.Run(async () =>
            {
                int pollingInterval = tag.ScanRate > 0 ? tag.ScanRate : _opsConfig.DefaultScanRate;
                while (_cts != null && !_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(pollingInterval, _cts.Token).ConfigureAwait(false);

                        if (_cts == null)
                        {
                            break;
                        }

                        if (!connector.CanConnect)
                        {
                            continue;
                        }

                        // 若读取失败，该信号点不会复位，下次会继续读取执行。
                        var (ok, data, err) = await connector.ReadAsync(tag).ConfigureAwait(false);
                        if (!ok)
                        {
                            _logger.LogError("[Engine] Notice 数据读取异常，设备：{DeviceName}，标记：{TagName}, 地址：{TagAddress}，错误：{Err}", 
                                device.Name, tag.Name, tag.Address, err);

                            continue;
                        }

                        // 在仅数据变更才发送模式下，会校验数据是否有跳变。
                        if (tag.PublishMode == PublishMode.OnlyDataChanged && TagValueSet.CompareAndSwap(tag.TagId, data.Value))
                        {
                            continue;
                        }

                        // 发布通知事件
                        await _publisher.Publish(new NoticeEvent
                        {
                            Connector = connector,
                            ChannelName = channelName,
                            Device = device,
                            Tag = tag,
                            Self = data,
                        }).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[Engine] Notice 数据处理异常，设备：{DeviceName}，标记：{TagName}, 地址：{TagAddress}", 
                            device.Name, tag.Name, tag.Address);
                    }
                }
            });
        }

        return Task.CompletedTask;
    }

    private Task SwitchMonitorAsync(DriverConnector connector)
    {
        var (channelName, device) = _deviceManager.GetDevice2(connector.Id);
        var tags = device!.GetAllTags(TagFlag.Switch);
        foreach (var tag in tags)
        {
            AsyncManualResetEvent mre = new(false); // 手动事件
           
            // 开关绑定的数据
            _ = Task.Run(async () =>
            {
                int pollingInterval = _opsConfig.SwitchScanRate > 0 ? _opsConfig.SwitchScanRate : 30;
                while (_cts != null && !_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await mre.WaitAsync().ConfigureAwait(false);

                        // 第一次检测
                        if (_cts == null)
                        {
                            break;
                        }

                        await Task.Delay(pollingInterval, _cts.Token).ConfigureAwait(false);

                        // 第二次检测
                        if (_cts == null)
                        {
                            break;
                        }

                        if (!connector.CanConnect)
                        {
                            continue;
                        }

                        // 记录数据
                        await _publisher.Publish(new SwitchEvent { Connector = connector, ChannelName = channelName, Device = device, Tag = tag }).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[Engine] Switch 数据读取异常，设备：{DeviceName}，标记：{TagName}, 地址：{TagAddress}",
                            device.Name, tag.Name, tag.Address);
                    }
                }
            });

            // 开关状态监控
            _ = Task.Run(async () =>
            {
                int pollingInterval = tag.ScanRate > 0 ? tag.ScanRate : _opsConfig.DefaultScanRate;
                bool isOn = false; // 开关开启状态
                while (_cts != null && !_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(pollingInterval, _cts.Token).ConfigureAwait(false);

                        if (_cts == null)
                        {
                            break;
                        }

                        if (!connector.CanConnect)
                        {
                            continue;
                        }

                        var (ok, data, _) = await connector.ReadAsync(tag).ConfigureAwait(false);

                        // 读取成功且开关处于 on 状态，发送开启动作信号。
                        if (ok)
                        {
                            // 开关标记数据类型必须为 bool 或 int16
                            bool on = tag.DataType switch
                            {
                                DataType.Bit => data.GetBit(),
                                DataType.Int => data.GetInt() == 1,
                                _ => throw new NotSupportedException(),
                            };

                            if (on) // Open 标记
                            {
                                // Open 信号，在本身处于关闭状态，才执行开启动作。
                                if (!isOn)
                                {
                                    // 发送 On 信号结束标识
                                    await _publisher.Publish(new SwitchEvent
                                    {
                                        Connector = connector,
                                        ChannelName = channelName,
                                        Device = device,
                                        Tag = tag,
                                        Self = data,
                                        State = SwitchState.On,
                                        IsSwitchSignal = true,
                                    }).ConfigureAwait(false);

                                    // 开关开启时，发送信号，让子任务执行。
                                    isOn = true;
                                    mre.Set();
                                }
                            }
                            else
                            {
                                // Close 标记，在本身处于开启状态，才执行关闭动作。
                                if (isOn)
                                {
                                    // 发送 Off 信号结束标识事件
                                    await _publisher.Publish(new SwitchEvent
                                    {
                                        Connector = connector,
                                        ChannelName = channelName,
                                        Device = device,
                                        Tag = tag,
                                        Self = data,
                                        State = SwitchState.Off,
                                        IsSwitchSignal = true,
                                    }).ConfigureAwait(false);

                                    // 读取失败或开关关闭时，重置信号，让子任务阻塞。
                                    isOn = false;
                                    mre.Reset();
                                }
                            }

                            // 跳转
                            continue;
                        }

                        // 若读取失败，且开关处于 on 状态，则发送关闭动作信号（防止因设备未掉线，而读取失败导致一直发送数据）。
                        if (isOn)
                        {
                            // 发送 Off 信号结束标识事件
                            await _publisher.Publish(new SwitchEvent 
                            {
                                Connector = connector,
                                ChannelName = channelName,
                                Device = device,
                                Tag = tag,
                                State = SwitchState.Off,
                                IsSwitchSignal = true,
                            }).ConfigureAwait(false);

                            // 读取失败或开关关闭时，重置信号，让子任务阻塞。
                            isOn = false;
                            mre.Reset();
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[Engine] Switch 开关数据处理异常，设备：{DeviceName}，标记：{TagName}, 地址：{TagAddress}",
                            device.Name, tag.Name, tag.Address);
                    }
                }

                // 任务取消后，无论什么情况都发送信号，确保让子任务也能退出
                mre.Set();
            });
        }

        return Task.CompletedTask;
    }

    public Task ShutdownAsync()
    {
        if (!IsRunning)
        {
            return Task.CompletedTask;
        }
        IsRunning = false;

        CancellationTokenSource? cts = _cts;
        if (_cts != null)
        {
            _cts = null;
            cts!.Cancel();
        }

        // 需延迟 Dispose
        _ = Task.Run(async() =>
        {
            await Task.Delay(1000).ConfigureAwait(false);
            
            cts?.Dispose();
            _driverConnectorManager.Close();
        }).ConfigureAwait(false);

        _logger.LogInformation("[Engine] 引擎停止");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        ShutdownAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    }
}
