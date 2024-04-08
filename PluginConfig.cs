using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Configuration;
using static FunnyProjector.MyPluginInfo;

namespace FunnyProjector;

public class PluginConfig(ConfigFile config, string configDir) {
    private readonly ConfigEntry<bool> _keepVanilla = config.Bind("Textures", "Keep vanilla", true);
    private readonly ConfigEntry<bool> _acceptFromAll = config.Bind("Textures", "Accept from all", true);

    public bool KeepVanilla => _keepVanilla.Value;
    public bool AcceptFromAll => _acceptFromAll.Value;

    public IEnumerable<string> Urls {
        get {
            var path = Path.Join(configDir, $"{PLUGIN_NAME}.Backgrounds.txt");

            if (File.Exists(path)) {
                return File.ReadAllLines(path)
                           .AsEnumerable()
                           .Select(url => url.Trim())
                           .Where(url => !string.IsNullOrWhiteSpace(url))
                           .ToArray();
            }
            else {
                File.Create(path).Close();
                return [];
            }
        }
    }
}