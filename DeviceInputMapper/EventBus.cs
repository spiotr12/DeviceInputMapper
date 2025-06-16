using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace EventQueue;

public struct Event
{
    public static readonly string StartMessage = "Start";
    public static readonly string ReloadMessage = "Reload";
    public static readonly string ExitMessage = "Exit";

    public static Event Start => new() { Message = StartMessage };
    public static Event Reload => new() { Message = ReloadMessage };
    public static Event Exit => new() { Message = ExitMessage };

    public string? Message;
    public object? Payload;

    public override string ToString()
    {
        return $"Event message = \"{Message}\" payload = \"{Payload}\"";
    }
}

public static class EventBus
{
    public static IObservable<Event> Queue => _queue.AsObservable();

    private static readonly ISubject<Event> _queue = new Subject<Event>();

    public static void Emit(Event e)
    {
        _queue.OnNext(e);
    }

    public static void Complete()
    {
        _queue.OnCompleted();
    }
}