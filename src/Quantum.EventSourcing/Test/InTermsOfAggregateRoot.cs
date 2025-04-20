using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Quantum.Domain;
using Quantum.Domain.Messages.Event;

namespace Quantum.EventSourcing.Test
{
    public class InTermsOfAggregateRoot<T, TId>
        where T : IsAnAggregateRoot<TId> where TId : IsAnIdentity<TId>
    {
        private T _aggregate;


        public static InTermsOfAggregateRoot<T, TId> IfICreate(Func<T> func)
        {
            return new()
            {
                _aggregate = func.Invoke()
            };
        }

        public InTermsOfAggregateRoot<T, TId> ThenIWillExpectTheseEvents(params IsADomainEvent[] events)
        {
            var queuedEvents = _aggregate.DeQueueDomainEvents();
            queuedEvents.Count.Should().Be(events.Length);

            queuedEvents.SequenceEqual(events).Should().BeTrue();

            return this;
        }

        public void And(Func<T, bool> action)
            => action.Invoke(_aggregate).Should().BeTrue();
        
        public void ThenIWillExpect(Func<T, bool> action)
        {
            And(action);
        }

        public void And(params Action<T>[] actions)
        {
            var exceptions = new List<Exception>();
            foreach (var action in actions)
            {
                try
                {
                    action.Invoke(_aggregate);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }
            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }

        public static InTermsOfAggregateRoot<T, TId> IfIApplied(params IsADomainEvent[] events)
        {
            if (events == null || !events.Any())
                throw new Exception("Events can not be null or empty array!");

            object instance;
            try
            {
                instance = Activator.CreateInstance(typeof(T), new object[] { events });
            }
            catch (MissingMethodException e)
            {
                Console.WriteLine(e);
                throw new Exception($"the {typeof(T)} must have one public constructor with one parameters : {events.GetType()} ");
            }
            return new InTermsOfAggregateRoot<T, TId>
            {
                _aggregate = (T)instance
            };
        }

        public InTermsOfAggregateRoot<T, TId> WhenICall(Action<T> func)
        {
            func.Invoke(_aggregate);
            return this;
        }

        public void ThenIWillExpect(params Action<T>[] actions)
        {
            And(actions);
        }
    }
}