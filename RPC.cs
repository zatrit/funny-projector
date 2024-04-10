using System.Collections.Generic;
using MyceliumNetworking;
using System;
using System.Linq;
using Steamworks;

using static MyceliumNetworking.MyceliumNetwork;

namespace FunnyProjector.RPCs;

public class UrlsRPC(uint modId) {
    public Action<CSteamID, IEnumerable<string>>? OnSuggestUrls;
    public Action<IEnumerable<string>, bool>? OnResultUrls;

    public void Register() => RegisterNetworkObject(this, modId);

    static IEnumerable<string> SplitUrls(string urls)
        => urls.Split("\n").Select(url => url.Trim()).ToArray();

    static string JoinUrls(IEnumerable<string> urls) => string.Join("\n", urls);

    [CustomRPC]
    void AcceptSuggestUrls(string data, RPCInfo info)
        => OnSuggestUrls?.Invoke(info.SenderSteamID, SplitUrls(data));

    [CustomRPC]
    void AcceptResultUrls(string data, bool keepVanilla, RPCInfo info) {
        if (info.SenderSteamID != LobbyHost) {
            throw new Exception("Only host can send ResultUrls requests");
        }

        OnResultUrls?.Invoke(SplitUrls(data), keepVanilla);
    }

    public void SuggestUrls(IEnumerable<string> urls)
        => RPCTarget(
            modId, nameof(AcceptSuggestUrls), LobbyHost, ReliableType.Reliable,
            JoinUrls(urls)
        );

    public void SendResultUrls(IEnumerable<string> urls, bool keepVanilla)
        => RPC(
            modId, nameof(AcceptResultUrls), ReliableType.Reliable,
            JoinUrls(urls), keepVanilla
        );
}