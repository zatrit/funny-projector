using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace FunnyProjector;

public static class TextureLoader {
    private static readonly HttpClient _httpClient = new();

    public static Task<Texture2D[]> LoadTexturesAsync(IEnumerable<string> urls)
        => Task.WhenAll(urls.Select(async url => {
            try {
                if (!url.StartsWith("https://"))
                    throw new Exception("Only https:// is supported");

                var result = await _httpClient.GetAsync(url);
                var texture = new Texture2D(1, 1);
                texture.LoadImage(await result.Content.ReadAsByteArrayAsync());

                return texture;
            } catch (Exception ex) {
                Debug.LogError(ex);
                return Plugin.FallbackTexture!;
            }
        }));
}