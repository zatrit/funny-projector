using System;
using System.Collections.Generic;
using System.Reflection.Emit;
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
        Debug.LogError("Creating projector machine");

        Urls!.OnResultUrls += (urls, keepVanilla) => ApplyUrls(__instance, urls, keepVanilla);

        if (MyceliumNetwork.IsHost) {
            Urls!.SendResultUrls();
        }
    }

    static Coroutine ApplyUrls(ProjectorMachine proj, IEnumerable<string> urls, bool keepVanilla)
        => proj.StartCoroutine(LoadTextures(urls, loadedTextures => {
            var textures = keepVanilla ? proj.textures : proj.textures.SubArray(0, 1);
            Textures = [.. textures, .. loadedTextures];
        }));

    static IEnumerator<AsyncOperation> LoadTextures(IEnumerable<string> urls, Action<List<Texture2D>> handler) {
        List<Texture2D> textures = [];

        foreach (var url in urls) {
            if (!url.StartsWith("https://")) {
                textures.Add(FallbackTexture!);
                Debug.LogError("Only https:// is supported");
                continue;
            }

            var www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();

            try {
                textures.Add(DownloadHandlerTexture.GetContent(www));
            } catch (Exception ex) {
                textures.Add(FallbackTexture!);
                Debug.LogError(ex);
            }
        }

        handler(textures);
    }
}

[HarmonyPatch(typeof(ProjectorMachine), "RPCA_Press")]
public class ReplaceTextures {
    static readonly CodeMatch[] origTextureLoad = [
        new(OpCodes.Ldfld, typeof(ProjectorMachine).GetField("textures"))
    ];

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
        => new CodeMatcher(codes)
            .Start()
            .MatchForward(true, origTextureLoad)
            .Repeat(matcher => matcher
                .RemoveInstruction()
                .InsertAndAdvance([
                    new(OpCodes.Ldfld, typeof(Plugin).GetField(nameof(Textures)))
                ]))
            .InstructionEnumeration();
}