using Newtonsoft.Json;

namespace Extensions;

public static class ThreadExtension
{
    public static void WaitAll(this IEnumerable<Thread> threads)
    {
        if (threads != null)
        {
            foreach (Thread thread in threads)
            {
                thread.Join();
            }
        }
    }
}

public static class CopyExtension
{
    public static T Copy<T>(this T original)
    {
        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(original));
    }
}