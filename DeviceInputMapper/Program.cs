using SharpDX.DirectInput;
using SharpDX.RawInput;
using DeviceType = SharpDX.DirectInput.DeviceType;

namespace DeviceInputMapper;

class Program
{
    static async Task Main(string[] args)
    {
        var directInput = new DirectInput();
        var configFilePath = "C:\\Projects\\DeviceInputMapper\\config.json";

        var deviceController = new DeviceController(directInput, configFilePath);
        deviceController.PrintAllDevicesInfo();

        var config = deviceController.LoadConfig();

        var allTasks = new List<Task>();

        foreach (var (id, deviceConfig) in config.Devices)
        {
            var instanceGuid = Guid.Parse(id);
            var device = deviceController.FindByInstanceGuid(instanceGuid);
            if (device != null)
            {
                if (deviceConfig.InputDeviceType == "joystick")
                {
                    var joystick = new Joystick(directInput, instanceGuid);
                    var handler = new JoystickHandler(deviceConfig, joystick);
                    allTasks.Add(handler.Run());
                }

                if (deviceConfig.InputDeviceType == "keyboard")
                {
                    var keyboard = new Keyboard(directInput);
                    var handler = new KeyboardHandler(deviceConfig, keyboard);
                    allTasks.Add(handler.Run());
                }

                if (deviceConfig.InputDeviceType == "mouse")
                {
                    var mouse = new Mouse(directInput);
                    var handler = new MouseHandler(deviceConfig, mouse);
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

class Handler<T, TRaw, TUpdate>
    where T : class, IDeviceState<TRaw, TUpdate>, new()
    where TRaw : struct
    where TUpdate : struct, IStateUpdate
{
    protected readonly DeviceConfig _config;
    protected readonly CustomDevice<T, TRaw, TUpdate> _device;

    public Handler(DeviceConfig config, CustomDevice<T, TRaw, TUpdate> device)
    {
        _config = config;
        _device = device;

        _device.Properties.BufferSize = 128;
        _device.Acquire();
    }

    public Task Run()
    {
        return Task.Run(() =>
        {
            Console.WriteLine("{0} [{1}]\n> Task started\n", _config.InstanceName, _config.InstanceGuid);

            // Poll events from joystick
            while (true)
            {
                _device.Poll();
                var datas = _device.GetBufferedData();
                foreach (var state in datas)
                {
                    Console.WriteLine("{0} [{1}] ", _config.InstanceName, _config.InstanceGuid);
                    Console.WriteLine("> {0}", state);
                    Console.WriteLine();
                }
            }
        }, new CancellationToken());
    }
}

class JoystickHandler : Handler<JoystickState, RawJoystickState, JoystickUpdate>
{
    private readonly Joystick _joystick;

    public JoystickHandler(DeviceConfig config, Joystick joystick)
        : base(config, joystick)
    {
        _joystick = joystick;
    }
}

class KeyboardHandler : Handler<KeyboardState, RawKeyboardState, KeyboardUpdate>
{
    private readonly Keyboard _keyboard;

    public KeyboardHandler(DeviceConfig config, Keyboard keyboard)
        : base(config, keyboard)
    {
        _keyboard = keyboard;
    }
}

class MouseHandler : Handler<MouseState, RawMouseState, MouseUpdate>
{
    private readonly Mouse _mouse;

    public MouseHandler(DeviceConfig config, Mouse mouse)
        : base(config, mouse)
    {
        _mouse = mouse;
    }
}