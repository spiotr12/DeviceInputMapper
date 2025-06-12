using SharpDX.DirectInput;
using SharpDX.XInput;

namespace DeviceInputMapper;

class Program
{
    [STAThread]
    static async Task Main(string[] args)
    {
        var directInput = new DirectInput();
        var configFilePath = "C:\\Projects\\DeviceInputMapper\\config.json";

        var deviceController = new DeviceController(directInput, configFilePath);
        deviceController.PrintAllDevicesInfo();

        var config = deviceController.LoadConfig();

        var allTasks = new List<Task>();

        // var c1 = new Controller(UserIndex.One);
        // var c2 = new Controller(UserIndex.Two);
        //
        // while (true)
        // {
        //     if (c1.IsConnected)
        //         Console.WriteLine("c1 {0}", c1.GetState().Gamepad.ToString());
        //     if (c2.IsConnected)
        //         Console.WriteLine("c2 {0}", c2.GetState().Gamepad.ToString());
        // }

        foreach (var (id, deviceConfig) in config.Devices)
        {
            // DirectInputDevices
            try
            {
                if (Guid.TryParse(id, out var instanceGuid))
                {
                    var device = deviceController.FindByInstanceGuid(instanceGuid);
                    if (device != null)
                    {
                        if (deviceConfig.InputDeviceType == InputDeviceType.Joystick)
                        {
                            var joystick = new Joystick(directInput, instanceGuid);
                            var handler = new JoystickHandler(id, deviceConfig, joystick);
                            handler.EnableLogging = true;
                            allTasks.Add(handler.Run());
                        }

                        if (deviceConfig.InputDeviceType == InputDeviceType.Keyboard)
                        {
                            var keyboard = new SharpDX.DirectInput.Keyboard(directInput);
                            var handler = new KeyboardHandler(id, deviceConfig, keyboard);
                            handler.EnableLogging = true;
                            allTasks.Add(handler.Run());
                        }

                        if (deviceConfig.InputDeviceType == InputDeviceType.Mouse)
                        {
                            var mouse = new SharpDX.DirectInput.Mouse(directInput);
                            var handler = new MouseHandler(id, deviceConfig, mouse);
                            allTasks.Add(handler.Run());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // ignored
            }

            // Controllers
            try
            {
                if (Enum.TryParse<UserIndex>(id, out UserIndex userIndex))
                {
                    var controller = new Controller(userIndex);
                    var handler = new ControllerHandler(id, deviceConfig, controller);
                    allTasks.Add(handler.Run());
                }
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        await Task.WhenAll(allTasks);
    }
}