using BepInEx;
using static FunnyProjector.MyPluginInfo;
using HarmonyLib;
using FunnyProjector.RPCs;
using UnityEngine;
using System.IO;

#pragma warning disable IDE0051 // Remove unused private members

namespace FunnyProjector;

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin {
    private static readonly UrlsRPC _urlsRPC = new(PLUGIN_ID);

    public static readonly uint PLUGIN_ID = GetStableHashCode(PLUGIN_NAME);

    public static Texture2D? FallbackTexture;
    public static new PluginConfig? Config;
    public static UrlsManager? Urls;

    public static Texture2D[] Textures = [];

    void Awake() {
        Config = new(base.Config, Paths.ConfigPath);
        Urls = new(_urlsRPC, Config);

        var assembly = GetType().Assembly;
        using var fallbackStream = assembly.GetManifestResourceStream("fallback.png");
        using var memoryStream = new MemoryStream();
        fallbackStream.CopyTo(memoryStream);

        FallbackTexture = new Texture2D(800, 600);
        FallbackTexture.LoadImage(memoryStream.ToArray());

        new Harmony(PLUGIN_NAME).PatchAll();
    }

    void Start() => _urlsRPC.Register();

    // https://stackoverflow.com/a/36845864/12245612
    private static uint GetStableHashCode(string str) {
        unchecked {
            uint hash1 = 5381;
            uint hash2 = hash1;

            for (int i = 0; i < str.Length && str[i] != '\0'; i += 2) {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1 || str[i + 1] == '\0')
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }

            return hash1 + (hash2 * 1566083941);
        }
    }
}