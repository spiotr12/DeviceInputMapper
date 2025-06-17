using Extensions;
using Newtonsoft.Json;

namespace DeviceInputMapper;

public static class InputDeviceType
{
    public static string Joystick = "joystick";
    public static string Gamepad = "gamepad";
    public static string Controller = "controller";
    public static string Mouse = "mouse";
    public static string Keyboard = "keyboard";
}

public class Config
{
    [JsonProperty("defaultMode")] public string DefaultMode = "Default";

    [JsonProperty("modes", NullValueHandling = NullValueHandling.Ignore)]
    public IDictionary<string, ModeConfig> Modes = new Dictionary<string, ModeConfig>();

    [JsonProperty("devices", NullValueHandling = NullValueHandling.Ignore)]
    public IDictionary<string, DeviceConfig> Devices = new Dictionary<string, DeviceConfig>();

    public Config ParseConfig()
    {
        var copy = this.Copy();

        // Nothing to merge
        if (Modes.Count <= 1)
        {
            return copy;
        }

        // At least one must not have parent
        var atLeastOneMasterMode = Modes.Select(m => m.Value).Any(c => c.Parent == null);
        if (!atLeastOneMasterMode)
        {
            throw new Exception("At least one mode must be a master (not have a parent)");
        }

        foreach (var (mode, modeConfig) in copy.Modes)
        {
            if (modeConfig.Parent != null)
            {
                if (mode.Equals(modeConfig.Parent))
                {
                    throw new Exception($"Mode \"{mode}\" cannot inherit from itself");
                }

                foreach (var (id, deviceConfig) in copy.Devices)
                {
                    if (deviceConfig.Configs == null)
                    {
                        deviceConfig.Configs = new Dictionary<string, IDictionary<string, InputMappingConfig>>();
                    }

                    if (!deviceConfig.Configs.ContainsKey(modeConfig.Parent))
                    {
                        deviceConfig.Configs[modeConfig.Parent] = new Dictionary<string, InputMappingConfig>();
                    }

                    if (!deviceConfig.Configs.ContainsKey(mode))
                    {
                        deviceConfig.Configs[mode] = new Dictionary<string, InputMappingConfig>();
                    }

                    var source = deviceConfig.Configs?[modeConfig.Parent];
                    var target = deviceConfig.Configs?[mode];

                    if (source == null)
                    {
                        Console.WriteLine($"No config found for source \"{id}\": \"{modeConfig.Parent}\"");
                    }

                    if (target == null)
                    {
                        Console.WriteLine($"No config found for target \"{id}\": \"{mode}\"");
                    }

                    if (source != null && target != null)
                    {
                        foreach (var (button, buttonConfig) in source)
                        {
                            if (!target.ContainsKey(button))
                            {
                                target.Add(button, buttonConfig.Copy());
                            }
                        }
                    }
                }
            }
        }

        return copy;
    }
}

public class DeviceConfig
{
    [JsonProperty("description")] public string? Description;

    [JsonProperty("configs", NullValueHandling = NullValueHandling.Ignore)]
    public IDictionary<string, IDictionary<string, InputMappingConfig>>? Configs;

    [JsonProperty("inputDeviceType")] public string? InputDeviceType;


    [JsonProperty("_instanceName", NullValueHandling = NullValueHandling.Ignore)]
    public string? InstanceName;

    [JsonProperty("_instanceGuid", NullValueHandling = NullValueHandling.Ignore)]
    public string? InstanceGuid;

    [JsonProperty("_productName", NullValueHandling = NullValueHandling.Ignore)]
    public string? ProductName;

    [JsonProperty("_productGuid", NullValueHandling = NullValueHandling.Ignore)]
    public string? ProductGuid;

    [JsonProperty("_forceFeedbackDriverGuid", NullValueHandling = NullValueHandling.Ignore)]
    public string? ForceFeedbackDriverGuid;

    [JsonProperty("_isHumanInterfaceDevice", NullValueHandling = NullValueHandling.Ignore)]
    public bool? IsHumanInterfaceDevice;

    [JsonProperty("_type", NullValueHandling = NullValueHandling.Ignore)]
    public string? Type;

    [JsonProperty("_subtype", NullValueHandling = NullValueHandling.Ignore)]
    public int? Subtype;

    [JsonProperty("_usage", NullValueHandling = NullValueHandling.Ignore)]
    public string? Usage;

    [JsonProperty("_usagePage", NullValueHandling = NullValueHandling.Ignore)]
    public string? UsagePage;
}

public class InputMappingConfig
{
    [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
    public string? label;

    [JsonProperty("min", NullValueHandling = NullValueHandling.Ignore)]
    public double? MinValue;

    [JsonProperty("max", NullValueHandling = NullValueHandling.Ignore)]
    public double? MaxValue;

    [JsonProperty("rawMin", NullValueHandling = NullValueHandling.Ignore)]
    public object? MinRawValue;

    [JsonProperty("rawMax", NullValueHandling = NullValueHandling.Ignore)]
    public object? MaxRawValue;

    [JsonProperty("actions", NullValueHandling = NullValueHandling.Ignore)]
    public IEnumerable<ActionConfig> Actions = new List<ActionConfig>();
}

public class ActionConfig
{
    [JsonProperty("condition", NullValueHandling = NullValueHandling.Ignore)]
    public string? Condition;

    [JsonProperty("action", NullValueHandling = NullValueHandling.Ignore)]
    public string Action = "";
}

public class ModeConfig
{
    [JsonProperty("parent", NullValueHandling = NullValueHandling.Ignore)]
    public string? Parent;
}