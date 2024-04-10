using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using FunnyProjector.RPCs;
using Steamworks;
using UnityEngine.SceneManagement;
using static MyceliumNetworking.MyceliumNetwork;

namespace FunnyProjector;

public class UrlsManager {
    private readonly Dictionary<CSteamID, IEnumerable<string>> _suggestedUrls = [];
    private readonly PluginConfig _config;
    private readonly UrlsRPC _rpc;

    public Action<IEnumerable<string>, bool>? OnResultUrls;

    private IEnumerable<IEnumerable<string>> _allowedUrls
        => _suggestedUrls.Where(pair => IsAllowed(pair.Key))
                         .Select(pair => pair.Value);

    public IEnumerable<string> Urls => _suggestedUrls.Count switch {
        0 => [],
        1 => _allowedUrls.First(),
        _ => _allowedUrls.Aggregate((a, b) => a.Concat(b)),
    };

    static CSteamID CurrentUser => SteamUser.GetSteamID();

    public UrlsManager(UrlsRPC rpc, PluginConfig config) {
        (_rpc, _config) = (rpc, config);

        PlayerLeft += steamID => _suggestedUrls.Remove(steamID);
        LobbyEntered += () => {
            _suggestedUrls.Clear();
            _rpc.SuggestUrls(_config.Urls);
        };

        rpc.OnSuggestUrls += (steamID, urls) => {
            if (!IsHost) {
                throw new Exception("Not a host");
            }

            _suggestedUrls.Add(steamID, urls);
            SendResultUrls();
        };
        rpc.OnResultUrls += (urls, keepVanilla) => OnResultUrls?.Invoke(urls, keepVanilla);

        ResendOnConfigChange(_config.KeepVanilla);
        ResendOnConfigChange(_config.AllowedFrom);

        /* Reset OnResultUrls each time scene changes to avoid
        setting textures for old greenscreens */
        SceneManager.activeSceneChanged += (_, _) => OnResultUrls = null;
    }

    public void SendResultUrls() => _rpc.SendResultUrls(Urls, _config.KeepVanilla.Value);

    private bool IsAllowed(CSteamID steamID)
        => _config.AllowedFrom.Value switch {
            Group.Friends => SteamFriends.GetFriendRelationship(steamID) == EFriendRelationship.k_EFriendRelationshipFriend || steamID == CurrentUser,
            Group.Host => steamID == CurrentUser,
            Group.Everyone => true,
            _ => throw new NotImplementedException()
        };

    private void ResendOnConfigChange<T>(ConfigEntry<T> entry)
        => entry.SettingChanged += (_, _) => SendResultUrls();
}