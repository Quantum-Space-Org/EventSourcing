using Quantum.Resolver;

namespace Quantum.UnitTests.EventSourcing
{
    public class StubedResolver : IResolver
    {
        public T Resolve<T>()
        {
            throw new NotImplementedException();
        }

        public object Resolve(Type type)
        {
            return Obj;
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            throw new NotImplementedException(" ResolveAll<T>()");
        }

        public static IResolver WhichResolve(object subscriver)
        {
            return new StubedResolver
            {
                Obj = subscriver
            };
        }

        public object Obj { get; set; }
    }
}