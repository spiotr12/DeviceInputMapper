using AutoMapper;
using Newtonsoft.Json;
using SharpDX.DirectInput;

namespace DeviceInputMapper;

public class DeviceController
{
    private readonly DirectInput _directInput;
    private readonly string _configFilePath;

    public DeviceController(DirectInput directInput, string configFilePath)
    {
        _directInput = directInput;
        _configFilePath = configFilePath;
    }

    public Config LoadConfig()
    {
        return LoadConfig(_configFilePath);
    }

    private Config LoadConfig(string filePath)
    {
        var rawJson = File.ReadAllText(filePath);
        var deserializeConfig = JsonConvert.DeserializeObject<Config>(rawJson);

        var mapperConfiguration = new MapperConfiguration(cfg => cfg.CreateMap<DeviceInstance, DeviceConfig>());
        var mapper = mapperConfiguration.CreateMapper();

        foreach (var (instanceGuid, deviceConfig) in deserializeConfig.Devices)
        {
            var device = FindByInstanceGuid(Guid.Parse(instanceGuid));
            if (device != null)
            {
                mapper.Map(device, deviceConfig);
            }

            if (deviceConfig.Modes == null || deviceConfig.Modes.Count == 0)
            {
                deviceConfig.Modes = new Dictionary<string, object>();
                deviceConfig.Modes.Add("Default", new Object());
            }
        }

        SaveConfig(deserializeConfig, filePath);

        return deserializeConfig;
    }

    public void SaveConfig(Config config)
    {
        SaveConfig(config, _configFilePath);
    }

    private void SaveConfig(Config config, string filePath)
    {
        var updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented, new JsonSerializerSettings
            {
                // ContractResolver = new CamelCasePropertyNamesContractResolver(),
            }
        );

        File.WriteAllText(filePath, updatedJson);
    }

    public DeviceInstance? FindByInstanceGuid(Guid guid)
    {
        foreach (var device in _directInput.GetDevices())
        {
            if (device.InstanceGuid == guid)
            {
                return device;
            }
        }

        return null;
    }

    public void PrintAllDevicesInfo()
    {
        foreach (var device in _directInput.GetDevices())
        {
            PrintDeriveInfo(device);
            Console.WriteLine();
        }
    }

    public void PrintDeriveInfo(DeviceInstance device)
    {
        Console.WriteLine("InstanceName:\t\t {0}", device.InstanceName);
        Console.WriteLine("InstanceGuid:\t\t {0}", device.InstanceGuid);
        Console.WriteLine("ProductName:\t\t {0}", device.ProductName);
        Console.WriteLine("ProductGuid:\t\t {0}", device.ProductGuid);
        Console.WriteLine("ForceFeedbackDriverGuid: {0}", device.ForceFeedbackDriverGuid);
        Console.WriteLine("IsHumanInterfaceDevice:\t {0}", device.IsHumanInterfaceDevice);
        Console.WriteLine("Type:\t\t\t {0}", device.Type);
        Console.WriteLine("Subtype:\t\t {0}", device.Subtype);
        Console.WriteLine("Usage:\t\t\t {0}", device.Usage);
        Console.WriteLine("UsagePage:\t\t\t {0}", device.UsagePage);
    }
}