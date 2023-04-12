﻿using ThingsEdge.Common.Storage;

namespace ThingsEdge.Router.Management;

internal sealed class DefaultDeviceManager : IDeviceManager
{
    public const string CacheName = "ThingsEdge.Device.Cache";

    private readonly IMemoryCache _cache;

    public DefaultDeviceManager(IMemoryCache cache)
    {
        _cache = cache;
    }

    public List<Channel> GetChannels()
    {
        return _cache.GetOrCreate(CacheName, entry =>
        {
            using var resp = DbManager.Create();
            return resp.Query<Channel>()
                    .Include(s => s.Devices)
                    .ToList();
        }) ?? new(0);
    }

    public List<Device> GetDevices()
    {
        var channels = GetChannels();
        return channels.SelectMany(s => s.Devices).ToList();
    }

    public Device? GetDevice(string deviceId)
    {
        var devices = GetDevices();
        return devices.FirstOrDefault(s => s.DeviceId == deviceId);
    }

    public void AddChannel(Channel channel)
    {
        throw new NotImplementedException();
    }

    public void AddDevice(string channelId, Device device)
    {
        throw new NotImplementedException();
    }

    public void AddDeviceTag(string tagGroupId, Tag tag)
    {
        throw new NotImplementedException();
    }

    public void AddTagGroup(string deviceId, TagGroup tagGroup)
    {
        throw new NotImplementedException();
    }

    public void AddTag(string tagGroupId, Tag tag)
    {
        throw new NotImplementedException();
    }

    public void RemoveChannel(string channelId)
    {
        throw new NotImplementedException();
    }

    public void RemoveDevice(string deviceId)
    {
        throw new NotImplementedException();
    }

    public void RemoveTagGroup(string tagGroupId)
    {
        throw new NotImplementedException();
    }

    public void RemoveTag(string tagId)
    {
        throw new NotImplementedException();
    }
}
