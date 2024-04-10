using System;
using System.Collections.Generic;
using System.Linq;
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
    public IEnumerable<string> Urls => _suggestedUrls.Count switch {
        0 => [],
        1 => _suggestedUrls.Values.First(),
        _ => _suggestedUrls.Values.Aggregate((a, b) => a.Concat(b)),
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

            var allowed = _config.AllowedFrom.Value switch {
                Group.Friends => SteamFriends.GetFriendRelationship(steamID) == EFriendRelationship.k_EFriendRelationshipFriend || steamID == CurrentUser,
                Group.Host => steamID == CurrentUser,
                Group.Everyone => true,
                _ => throw new NotImplementedException()
            };

            if (!allowed) return;

            _suggestedUrls.Add(steamID, urls);
            SendResultUrls();
        };
        rpc.OnResultUrls += (urls, keepVanilla) => OnResultUrls?.Invoke(urls, keepVanilla);

        /* Reset OnResultUrls each time scene changes to avoid
        setting textures for old greenscreens */
        SceneManager.activeSceneChanged += (_, _) => OnResultUrls = null;
    }

    public void SendResultUrls() => _rpc.SendResultUrls(Urls, _config.KeepVanilla.Value);
}