namespace DeviceInputMapper;

public interface Handler
{
    public abstract (Task task, CancellationTokenSource cts) Prepare();
    public bool EnableLogging { get; set; }
}