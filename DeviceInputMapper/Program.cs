using SharpDX.DirectInput;

namespace DeviceInputMapper;

class Program
{
    [STAThread]
    static async Task Main(string[] args)
    {
        var directInput = new DirectInput();
        var configFilePath = "C:\\Projects\\DeviceInputMapper\\config.json";

        var deviceController = new DeviceController(directInput, configFilePath);
        // deviceController.PrintAllDevicesInfo();

        var config = deviceController.LoadConfig();

        var allTasks = new List<Task>();
        var handlers = new List<object>();
        var globalState = new Dictionary<string, IDictionary<string, object>>();

        foreach (var (id, deviceConfig) in config.Devices)
        {
            var instanceGuid = Guid.Parse(id);
            var device = deviceController.FindByInstanceGuid(instanceGuid);
            if (device != null)
            {
                if (deviceConfig.InputDeviceType == "joystick")
                {
                    var joystick = new Joystick(directInput, instanceGuid);
                    var handler = new JoystickHandler(id, deviceConfig, joystick);
                    handlers.Add(handler);
                    handler.EnableLogging = true;
                    allTasks.Add(handler.Run());
                }

                if (deviceConfig.InputDeviceType == "keyboard")
                {
                    var keyboard = new SharpDX.DirectInput.Keyboard(directInput);
                    var handler = new KeyboardHandler(id, deviceConfig, keyboard);
                    handlers.Add(handler);
                    allTasks.Add(handler.Run());
                }

                if (deviceConfig.InputDeviceType == "mouse")
                {
                    var mouse = new SharpDX.DirectInput.Mouse(directInput);
                    var handler = new MouseHandler(id, deviceConfig, mouse);
                    handlers.Add(handler);
                    allTasks.Add(handler.Run());
                }
            }
        }

        await Task.WhenAll(allTasks);
    }

    private static IEnumerable<bool> Infinite()
    {
        while (true)
        {
            yield return true;
        }
    }
}