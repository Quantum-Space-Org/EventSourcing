namespace Quantum.UnitTests.EventSourcing;

public class Waiter
{
    public static Task Wait(Func<bool> func, TimeSpan time)
    {
        var maxMilliSecondToWait = time.TotalMilliseconds;
        var now = DateTime.Now;
        while (!func.Invoke() && now.AddMilliseconds(maxMilliSecondToWait) >= DateTime.Now)
        {
        }

        return Task.CompletedTask;
    }
}