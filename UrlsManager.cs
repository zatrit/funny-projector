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
    public IEnumerable<string> Urls => _suggestedUrls.Values.Aggregate((a, b) => a.Concat(b));

    static CSteamID CurrentUser => SteamUser.GetSteamID();

    public UrlsManager(UrlsRPC rpc, PluginConfig config) {
        (_rpc, _config) = (rpc, config);

        PlayerLeft += steamID => _suggestedUrls.Remove(steamID);
        LobbyLeft += _suggestedUrls.Clear;
        LobbyEntered += () => {
            _suggestedUrls.Clear();
            _rpc.SuggestUrls(_config.Urls, CurrentUser);
        };

        rpc.OnSuggestUrls += (steamID, urls) => {
            if (!IsHost) {
                throw new Exception("Not a host");
            }
            if (!_config.AcceptFromAll && steamID != CurrentUser) {
                return;
            }

            _suggestedUrls.Add(steamID, urls);
            SendResultUrls();
        };
        rpc.OnResultUrls += (urls, keepVanilla) => OnResultUrls?.Invoke(urls, keepVanilla);

        /* Reset OnResultUrls each time scene changes to avoid
        setting textures for old greenscreens */
        SceneManager.activeSceneChanged += (_, _) => OnResultUrls = null;
    }

    public void SendResultUrls() => _rpc.SendResultUrls(Urls, _config.KeepVanilla);
}