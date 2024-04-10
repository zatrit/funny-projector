using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using MyceliumNetworking;

using static FunnyProjector.Plugin;

namespace FunnyProjector.Patches;

[HarmonyPatch(typeof(ProjectorMachine), "Start")]
public class LogImages {
    static void Postfix(ProjectorMachine __instance) {
        var proj = __instance;
        Urls!.OnResultUrls += (urls, keepVanilla) =>
            TextureLoader.LoadTexturesAsync(urls).ContinueWith(loaded => {
                var textures = keepVanilla ? proj.textures : proj.textures.Take(1);
                Textures = [.. textures, .. loaded.Result];
            });

        if (MyceliumNetwork.IsHost) {
            Urls!.SendResultUrls();
        }
    }
}

[HarmonyPatch(typeof(ProjectorMachine), "RPCA_Press")]
public class ReplaceTextures {
    static readonly CodeMatch[] origTextureLoad = [
        new(OpCodes.Ldfld, typeof(ProjectorMachine).GetField("textures"))
    ];

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
        => new CodeMatcher(codes)
            .MatchForward(true, origTextureLoad)
            .Repeat(match => match.Set(OpCodes.Ldfld, typeof(Plugin).GetField(nameof(Textures))))
            .InstructionEnumeration();
}