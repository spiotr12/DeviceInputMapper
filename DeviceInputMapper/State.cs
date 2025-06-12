namespace DeviceInputMapper;

public static class State
{
    public static readonly IDictionary<string, IDictionary<string, (double value, double rawValue)>> Devices = new Dictionary<string, IDictionary<string, (double value, double rawValue)>>();
    public static string Mode { get; set; } = "Default";

    public static string ToString()
    {

        var str = String.Format("Mode: {0}\n", Mode);
        foreach (var (id, buttons) in Devices)
        {
            str += String.Format("id: {0}\n", id);

            foreach (var (key, value) in buttons)
            {
                str += String.Format("  key: {0}, value: {1}\n", key, value);
            }
        }

        return str;
    }
}