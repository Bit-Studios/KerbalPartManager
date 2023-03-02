
using SpaceWarp.API.Configuration;
using Newtonsoft.Json;
namespace KerbalPartManager;

// Define our config class with the [ModConfig] attribute
[ModConfig]
[JsonObject(MemberSerialization.OptOut)]
public class KerbalPartManagerConfig {
    [ConfigField("funny number")]
    [ConfigDefaultValue(69)]
    public int funny_number;
}
