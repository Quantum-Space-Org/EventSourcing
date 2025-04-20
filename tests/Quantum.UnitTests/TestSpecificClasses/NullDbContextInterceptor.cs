using Quantum.DataBase;
using Quantum.DataBase.EntityFramework.Interceptor;

namespace Quantum.UnitTests.TestSpecificClasses
{
    internal class NullDbContextInterceptor : IDbContextInterceptor
    {
        public Task Start() => Task.CompletedTask;

        public Task Commit() => Task.CompletedTask;

        public Task RoleBack() => Task.CompletedTask;
        

        public static IDbContextInterceptor New()
        {
            return new NullDbContextInterceptor();
        }
    }
}