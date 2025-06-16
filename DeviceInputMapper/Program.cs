using System.Reactive.Linq;
using EventQueue;
using SharpDX.DirectInput;
using SharpDX.XInput;

namespace DeviceInputMapper;

class Program
{
    private static readonly List<(Task task, CancellationTokenSource cts)> _allHandlersTasks = [];
    private static readonly bool _enableGlobalLogging = false;

    [STAThread]
    static async Task Main(string[] args)
    {
        Thread.CurrentThread.Name = "Main";

        var directInput = new DirectInput();
        var configFilePath = args[0];

        var deviceController = new DeviceController(directInput, configFilePath);
        deviceController.PrintAllDevicesInfo();

        EventBus.Queue.Subscribe(async void (ev) =>
        {
            if (ev.Message != null)
            {
                if (ev.Message.Equals(Event.StartMessage))
                {
                    Task.Run(() => LoadAndRun(directInput, deviceController));
                }

                if (ev.Message.Equals(Event.ReloadMessage))
                {
                    foreach (var (task, cts) in _allHandlersTasks)
                    {
                        cts.Cancel();
                    }

                    _allHandlersTasks.Clear();
                    Task.Run(() => LoadAndRun(directInput, deviceController));
                }

                if (ev.Message.Equals(Event.ExitMessage))
                {
                    foreach (var (task, cts) in _allHandlersTasks)
                    {
                        cts.Cancel();
                    }

                    _allHandlersTasks.Clear();

                    EventBus.Complete();
                }
            }
        });

        EventBus.Emit(Event.Start);

        await EventBus.Queue;
    }

    private static async Task LoadAndRun(DirectInput directInput, DeviceController deviceController)
    {
        Thread.Sleep(500);
        Console.WriteLine("Starting...");
        var rawConfig = deviceController.LoadConfig();
        var config = rawConfig.ParseConfig();

        foreach (var (id, deviceConfig) in config.Devices)
        {
            Handler handler = null;

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
                            handler = new JoystickHandler(id, deviceConfig, joystick);
                        }

                        if (deviceConfig.InputDeviceType == InputDeviceType.Keyboard)
                        {
                            var keyboard = new SharpDX.DirectInput.Keyboard(directInput);
                            handler = new KeyboardHandler(id, deviceConfig, keyboard);
                        }

                        if (deviceConfig.InputDeviceType == InputDeviceType.Mouse)
                        {
                            var mouse = new SharpDX.DirectInput.Mouse(directInput);
                            handler = new MouseHandler(id, deviceConfig, mouse);
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
                    handler = new ControllerHandler(id, deviceConfig, controller);
                }
            }
            catch (Exception e)
            {
                // ignored
            }

            if (handler != null)
            {
                handler.EnableLogging = _enableGlobalLogging;
                var prepared = handler.Prepare();
                prepared.task.Start();
                _allHandlersTasks.Add(prepared);
            }
        }

        await Task.WhenAll(_allHandlersTasks.Select(t => t.task));

        Console.WriteLine("Finished");
    }
}