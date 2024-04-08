using System;
using System.Collections.Generic;
using HarmonyLib;
using MyceliumNetworking;
using UnityEngine;
using UnityEngine.Networking;
using WebSocketSharp;
using static FunnyProjector.Plugin;

namespace FunnyProjector.Patches;

[HarmonyPatch(typeof(ProjectorMachine), "Start")]
public class LogImages {
    static void Postfix(ProjectorMachine __instance) {
        Urls!.OnResultUrls += (urls, keepVanilla) => ApplyUrls(__instance, urls, keepVanilla);

        if (MyceliumNetwork.IsHost) {
            ApplyUrls(__instance, Urls.Urls, Config!.KeepVanilla);
        }

        if (MyceliumNetwork.IsHost && SurfaceNetworkHandler.HasStarted) {
            Urls!.SendResultUrls();
        }
    }

    static Coroutine ApplyUrls(ProjectorMachine proj, IEnumerable<string> urls, bool keepVanilla)
    => proj.StartCoroutine(LoadTextures(urls, loadedTextures => {
        var textures = keepVanilla ? proj.textures : proj.textures.SubArray(0, 1);
        proj.textures = [.. textures, .. loadedTextures];
    }));

    static IEnumerator<AsyncOperation> LoadTextures(IEnumerable<string> urls, Action<List<Texture2D>> handler) {
        var textures = new List<Texture2D>();

        foreach (var url in urls) {
            var www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();

            try {
                textures.Add(DownloadHandlerTexture.GetContent(www));
            } catch(Exception ex) {
                textures.Add(FallbackTexture!);
                Debug.LogError(ex);
            }
        }

        handler(textures);
    }
}