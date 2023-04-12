﻿using ThingsEdge.Providers.Ops.Exchange;
using ThingsEdge.Router.Forwarder;

namespace ThingsEdge.Providers.Ops;

/// <summary>
/// 消息推送器。
/// </summary>
public sealed class MessagePusher : IMessagePusher
{
    private readonly IHttpForwarder _httpForwarder;
    private readonly ILogger _logger;

    public MessagePusher(IHttpForwarder httpForwarder, ILogger<MessagePusher> logger)
    {
        _httpForwarder = httpForwarder;
        _logger = logger;
    }

    public async Task PushAsync(DriverConnector connector, Device device, Tag tag, PayloadData? self = null, CancellationToken cancellationToken = default)
    {
        var tagGroup = device.TagGroups.FirstOrDefault(s => s.Tags.Any(x => x.TagId == tag.TagId));
        var message = new RequestMessage
        {
            Schema = new()
            {
                ChannelName = "", // 可不用设置
                DeviceName = device.Name,
                TagGroupName = tagGroup?.Name,
            },
            Flag = tag.Flag,
        };
        if (self != null)
        {
            message.Values.Add(self);
        }

        foreach (var normalTag in tag.NormalTags)
        {
            var (ok, data, err) = await connector.ReadAsync(normalTag);
            if (!ok)
            {
                _logger.LogError($"读取子标记值失败，设备：{message.Schema.DeviceName}，标记：{tag.Name}，地址：{tag.Address}, 错误：{err}");
                continue;
            }

            message.Values.Add(data!);
        }

        // 推送消息。
        var result = await _httpForwarder.SendAsync(message, cancellationToken);
        if (!result.IsSuccess())
        {
            // TODO: 推送失败日志
            _logger.LogError($"推送消息失败，设备：{message.Schema.DeviceName}，标记：{tag.Name}，地址：{tag.Address}, 错误：{result.ErrorMessage}");
            return;
        }

        // 非触发数据。
        if (tag.Flag != TagFlag.Trigger)
        {
            return;
        }

        // 连接器已断开。
        if (connector.CanConnect)
        {
            return;
        }

        // 若存在子数据，则先回写。
        bool hasError = false;
        foreach (var (tagName, tagValue) in result.Data.CallbackItems)
        {
            // 通过 tagName 找到对应的 Tag 标记。
            // 注意：回写标记与触发标记处于同一级别，位于设备下或是分组中。
            Tag? tag2 = (tagGroup?.Tags ?? device.Tags).FirstOrDefault(s => s.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase));
            if (tag2 == null)
            {
                _logger.LogError($"地址表中没有找到要回写的标记，设备：{message.Schema.DeviceName}，标记：{tag.Name}，地址：{tag.Address}, 回写标记：{tagName}");

                hasError = true;
                break;
            }

            var (ok2, err2) = await connector.WriteAsync(tag2!, tagValue);
            if (!ok2)
            {
                _logger.LogError($"回写标记数据失败，设备：{message.Schema.DeviceName}，标记：{tag2!.Name}，地址：{tag2.Address}, 错误：{err2}");

                hasError = true;
                break;
            }
        }

        // 回写标记状态。
        int tagCode = hasError ? (int)ErrorCode.CallbackItemError : result.Data.TagCode;
        var (ok3, err3) = await connector.WriteAsync(tag, tagCode);
        if (!ok3)
        {
            _logger.LogError($"回写触发标记状态失败，设备：{message.Schema.DeviceName}，标记：{tag.Name}，地址：{tag.Address}, 错误：{err3}");
            return;
        }
    }
}
