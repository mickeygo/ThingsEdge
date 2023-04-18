﻿using ThingsEdge.Contracts.Devices;
using ThingsEdge.Providers.Ops.Exchange;

namespace ThingsEdge.Providers.Ops.Handlers;

/// <summary>
/// 标记 <see cref="Contracts.Devices.TagFlag.Switch"/> 事件处理器。
/// </summary>
internal sealed class SwitchHandler : INotificationHandler<SwitchEvent>
{
    private readonly SwitchContainer _container;
    private readonly CurveStorage _curveStorage;
    private readonly ILogger _logger;

    public SwitchHandler(SwitchContainer container, CurveStorage curveStorage, ILogger<SwitchHandler> logger)
    {
        _container = container;
        _curveStorage = curveStorage;
        _logger = logger;
    }

    public async Task Handle(SwitchEvent notification, CancellationToken cancellationToken)
    {
        // 开关信号
        if (notification.IsSwitchSignal)
        {
            if (notification.State == SwitchState.On)
            {
                string? sn = null, no = null;
                // 码和序号
                var snTag = notification.Tag.NormalTags.FirstOrDefault(s => s.Usage == TagUsage.SwitchSN);
                if (snTag != null)
                {
                    var (ok1, data1, err1) = await notification.Connector.ReadAsync(snTag);
                    if (ok1)
                    {
                        sn = data1.GetString();
                    }
                    else
                    {
                        _logger.LogError("读取SwitchSN标记值失败, 设备: {DeviceName}, 标记: {TagName}，地址: {TagAddress}, 错误: {Err}",
                            notification.Device.Name, notification.Tag.Name, notification.Tag.Address, err1);
                    }
                }

                var noTag = notification.Tag.NormalTags.FirstOrDefault(s => s.Usage == TagUsage.SwitchNo);
                if (noTag != null)
                {
                    var (ok2, data2, err2) = await notification.Connector.ReadAsync(noTag);
                    if (ok2)
                    {
                        no = data2.GetString();
                    }
                    else
                    {
                        _logger.LogError("读取SwitchIndex标记值失败, 设备: {DeviceName}, 标记: {TagName}，地址: {TagAddress}, 错误: {Err}",
                            notification.Device.Name, notification.Tag.Name, notification.Tag.Address, err2);
                    }
                }

                var tagGroup = notification.Device.GetTagGroup(notification.Tag.TagId);
                var writer = _container.GetOrCreate(notification.Tag.TagId, _curveStorage.BuildCurveFilePath(tagGroup?.Name, sn, no)); 

                // 添加头信息
                var headers = string.Join(",", notification.Tag.NormalTags.Select(s => s.Keynote));
                await writer.WriteLineAsync(headers);
            }
            else if (notification.State == SwitchState.Off)
            {
                // 可根据返回的文件路径做其他处理。
                if (_container.TryRemove(notification.Tag.TagId, out var filepath))
                {
                    var (ok, err) = await _curveStorage.TryCopyAsync(filepath, cancellationToken);
                    if (!ok)
                    {
                        _logger.LogError("拷贝曲线失败，错误：{Err}", err);
                    }
                }
            }

            return;
        }

        // 开关数据
        if(!_container.TryGet(notification.Tag.TagId, out var writer2))
        {
            return;
        }

        // 读取触发标记下的子数据。
        List<string> items = new();
        foreach (var normalTag in notification.Tag.NormalTags.Where(s => s.Usage == Contracts.Devices.TagUsage.SwitchCurve))
        {
            // TODO: 思考如何将子数据地址合并，减少多次读取产生的性能开销。

            var (ok, data, err) = await notification.Connector.ReadAsync(normalTag);
            if (!ok)
            {
                _logger.LogError("读取子标记值失败, 设备: {DeviceName}, 标记: {TagName}，地址: {TagAddress}, 错误: {Err}",
                    notification.Device.Name, notification.Tag.Name, notification.Tag.Address, err);
            }

            items.Add(data?.GetString() ?? "0");
        }

        await writer2.WriteLineAsync(string.Join(",", items));
    }
}
