using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Configuration;

using static FunnyProjector.MyPluginInfo;

namespace FunnyProjector;

public enum Group {
    Host,
    Friends,
    Everyone,
}

public class PluginConfig(ConfigFile config, string configDir) {
    private const string SECTION = "Textures";

    public readonly ConfigEntry<bool> KeepVanilla = config.Bind(SECTION, "Keep vanilla", true);
    public readonly ConfigEntry<Group> AllowedFrom = config.Bind(SECTION, "Allowed from", Group.Friends);

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