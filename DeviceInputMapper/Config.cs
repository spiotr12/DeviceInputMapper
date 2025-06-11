using Newtonsoft.Json;
using SharpDX.DirectInput;

namespace DeviceInputMapper;

public class Config
{
    [JsonProperty("devices")] public IDictionary<string, DeviceConfig> Devices;
}

public class DeviceConfig
{
    [JsonProperty("modes")] public IDictionary<string, IDictionary<string, IEnumerable<InputConfig>>>? Modes;
    [JsonProperty("inputDeviceType")] public string? InputDeviceType;

    [JsonProperty("_instanceName")] public string? InstanceName;
    [JsonProperty("_instanceGuid")] public string? InstanceGuid;
    [JsonProperty("_productName")] public string? ProductName;
    [JsonProperty("_productGuid")] public string? ProductGuid;
    [JsonProperty("_forceFeedbackDriverGuid")] public string? ForceFeedbackDriverGuid;
    [JsonProperty("_isHumanInterfaceDevice")] public bool? IsHumanInterfaceDevice;
    [JsonProperty("_type")] public string? Type;
    [JsonProperty("_subtype")] public int? Subtype;
    [JsonProperty("_usage")] public string? Usage;
    [JsonProperty("_usagePage")] public string? UsagePage;
}

public class InputConfig
{

}