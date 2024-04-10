﻿using BepInEx;
using HarmonyLib;
using FunnyProjector.RPCs;
using UnityEngine;
using System.IO;

using static FunnyProjector.MyPluginInfo;

#pragma warning disable IDE0051 // Remove unused private members

namespace FunnyProjector;

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin {
    private static readonly UrlsRPC _urlsRPC = new(PLUGIN_ID);

    public const uint PLUGIN_ID = 971466466;

    public static Texture2D? FallbackTexture;
    public static new PluginConfig? Config;
    public static UrlsManager? Urls;

    public static Texture[] Textures = [];

    void Awake() {
        Config = new(base.Config, Paths.ConfigPath);
        Urls = new(_urlsRPC, Config);

        var assembly = GetType().Assembly;
        using var fallbackStream = assembly.GetManifestResourceStream("fallback.png");
        using var memoryStream = new MemoryStream(2048);
        fallbackStream.CopyTo(memoryStream);

        FallbackTexture = new Texture2D(800, 600);
        FallbackTexture.LoadImage(memoryStream.ToArray());

        new Harmony(PLUGIN_NAME).PatchAll();
    }

    void Start() => _urlsRPC.Register();
}
