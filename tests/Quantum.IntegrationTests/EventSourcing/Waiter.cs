using System;
using System.Threading.Tasks;

namespace Quantum.IntegrationTests.EventSourcing
{
    public class Waiter
    {
        public static Task Wait(Func<bool> func, TimeSpan time)
        {
            var maxMilisecountToWait = time.TotalMilliseconds;
            var now = DateTime.Now;
            while (!func.Invoke() && now.AddMilliseconds(maxMilisecountToWait) >= DateTime.Now)
            {
            }

            return Task.CompletedTask;
        }
    }
}