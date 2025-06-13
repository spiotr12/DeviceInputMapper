namespace DeviceInputMapper;

public struct StateValue
{
    public double value;
    public object rawValue;
}

public static class State
{
    public static readonly IDictionary<string, IDictionary<string, StateValue>> Devices =
        new Dictionary<string, IDictionary<string, StateValue>>();

    public static string Mode { get; set; } = "Default";

    public static string ToString()
    {
        var str = String.Format("Mode: {0}\n", Mode);
        foreach (var (id, buttons) in Devices)
        {
            str += String.Format("id: {0}\n", id);

            foreach (var (key, stateValue) in buttons)
            {
                str += String.Format("  key: \"{0}\", value: {1}, rawValue: {2}\n", key, stateValue.value, stateValue.rawValue);
            }
        }

        return str;
    }

    public static string DeviceToString(string id)
    {
        var str = "";
        Devices.TryGetValue(id, out var deviceState);
        foreach (var (key, value) in deviceState)
        {
            str += String.Format("  key: \"{0}\", value: {1}, rawValue: {2}\n", key, value.value, value.rawValue);
        }

        return str;
    }

    public static object? GetDevice(string id)
    {
        Devices.TryGetValue(id, out var deviceState);
        return deviceState;
    }
}