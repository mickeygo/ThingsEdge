﻿using ThingsEdge.Providers.Ops.Events;
using ThingsEdge.Providers.Ops.Exchange.Monitors;
using ThingsEdge.Router;
using ThingsEdge.Router.Devices;
using ThingsEdge.Router.Events;

namespace ThingsEdge.Providers.Ops.Exchange;

/// <summary>
/// 数据交换引擎。
/// </summary>
internal sealed class OpsExchange : IExchange, ISingletonDependency
{
    private CancellationTokenSource? _cts;

    private readonly IProducer _producer;
    private readonly IDeviceManager _deviceManager;
    private readonly DriverConnectorManager _driverConnectorManager;
    private readonly MonitorLoop _monitorLoop;
    private readonly ILogger _logger;

    public OpsExchange(IProducer producer,
        IDeviceManager deviceManager,
        DriverConnectorManager driverConnectorManager,
        MonitorLoop monitorLoop,
        ILogger<OpsExchange> logger)
    {
        _producer = producer;
        _deviceManager = deviceManager;
        _driverConnectorManager = driverConnectorManager;
        _monitorLoop = monitorLoop;
        _logger = logger;

        // 注册监控器
        MonitorLoop.Register<HeartbeatMonitor>();
        MonitorLoop.Register<NoticeMonitor>();
        MonitorLoop.Register<TriggerMonitor>();
        MonitorLoop.Register<SwitchMonitor>();
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
        await _producer.ProduceAsync(new ExchangeChangedEvent { State = RunningState.Startup }).ConfigureAwait(false);
        await _producer.ProduceAsync(LoggingMessageEvent.Info("[Engine] 引擎启动")).ConfigureAwait(false);

        _cts = new();

        var devices = _deviceManager.ReloadDevices();
        _driverConnectorManager.Load(devices);
        await _driverConnectorManager.ConnectAsync().ConfigureAwait(false);

        // 获取所有驱动，并监控设备数据
        foreach (var connector in _driverConnectorManager.GetAllDriver())
        {
            var (channelName, device) = _deviceManager.GetDevice2(connector.Id);
            _monitorLoop.Monitor(connector, channelName!, device!, _cts.Token);
        }
    }

    public async Task ShutdownAsync()
    {
        string msg = "[Engine] 引擎已停止";
        if (!IsRunning)
        {
            await _producer.ProduceAsync(LoggingMessageEvent.Info(msg)).ConfigureAwait(false);
            return;
        }
        IsRunning = false;

        CancellationTokenSource? cts = _cts;
        if (_cts != null)
        {
            _cts = null;
            cts!.Cancel();
        }

        // 需延迟 Dispose
        await Task.Delay(1000).ConfigureAwait(false);

        //cts?.Dispose(); // Dispose 会导致部分问题
        _driverConnectorManager.Close();

        _logger.LogInformation(msg);
        await _producer.ProduceAsync(new ExchangeChangedEvent { State = RunningState.Stop }).ConfigureAwait(false);
        await _producer.ProduceAsync(LoggingMessageEvent.Info(msg)).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        await ShutdownAsync().ConfigureAwait(false);
    }
}
