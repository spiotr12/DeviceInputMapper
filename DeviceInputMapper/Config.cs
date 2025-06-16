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
    [JsonProperty("devices", NullValueHandling = NullValueHandling.Ignore)]
    public IDictionary<string, DeviceConfig> Devices;
}

public class DeviceConfig
{
    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string? Description;

    [JsonProperty("modes", NullValueHandling = NullValueHandling.Ignore)]
    public IDictionary<string, IDictionary<string, IEnumerable<InputConfig>>>? Modes;

    [JsonProperty("inputDeviceType", NullValueHandling = NullValueHandling.Ignore)]
    public string? InputDeviceType;


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

public class InputConfig
{
    [JsonProperty("condition", NullValueHandling = NullValueHandling.Ignore)]
    public string? Condition;

    [JsonProperty("action", NullValueHandling = NullValueHandling.Ignore)]
    public string Action;
}