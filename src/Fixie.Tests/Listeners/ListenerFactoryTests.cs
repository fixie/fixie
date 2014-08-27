using Fixie.Execution;
using Fixie.Listeners;
using Fixie.Results;
using Should;
using Should.Core.Assertions;
using System;
using System.Linq;
using System.Reflection;

namespace Fixie.Tests.Listeners
{
    public class ListenerFactoryTests
    {
        public void CanCreateConsoleListener()
        {
            var options = Enumerable.Empty<string>().ToLookup(x => x);

            var listener = new ListenerFactory().CreateListener(options);
            listener.ShouldBeType<ConsoleListener>();
        }

        public void CanCreateTeamCityListener()
        {
            var options = new[]
            {
                new
                {
                    key = CommandLineOption.TeamCity,
                    value = "on"
                }
            }.ToLookup(x => x.key, x => x.value);

            var listener = new ListenerFactory().CreateListener(options);
            listener.ShouldBeType<TeamCityListener>();
        }

        public void CanCreateCustomListener()
        {
            var path = GetType().Assembly.Location;
            var type = typeof(TestListener).FullName;

            var options = new[]
            {
                new
                {
                    key = CommandLineOption.CustomListener,
                    value = path + "|" + type
                }
            }.ToLookup(x => x.key, x => x.value);

            var listener = new ListenerFactory().CreateListener(options);
            listener.ShouldBeType<TestListener>();
        }

        public void CanCreateMultipleCustomListeners()
        {
            var path = GetType().Assembly.Location;
            var type1 = typeof(TestListener).FullName;
            var type2 = typeof(TestListener).FullName;

            var options = new[]
            {
                new
                {
                    key = CommandLineOption.CustomListener,
                    value = path + "|" + type1
                },
                new
                {
                    key = CommandLineOption.CustomListener,
                    value = path + "|" + type2
                }
            }.ToLookup(x => x.key, x => x.value);

            var listener = new ListenerFactory().CreateListener(options);
            listener.ShouldBeType<CompoundListener>();
        }

        public void ShouldThrowIfCustomListenerOptionHasInvalidFormat()
        {
            var options = new[]
            {
                new
                {
                    key = CommandLineOption.CustomListener,
                    value = "-"
                }
            }.ToLookup(x => x.key, x => x.value);

            Assert.Throws<FormatException>(() => new ListenerFactory().CreateListener(options))
                  .Message.ShouldEqual("Valid CustomListener format is 'assembly-path|type'.");
        }

        class TestListener : Listener
        {
            public void AssemblyStarted(Assembly assembly)
            {
                throw new System.NotImplementedException();
            }

            public void CaseSkipped(SkipResult result)
            {
                throw new System.NotImplementedException();
            }

            public void CasePassed(PassResult result)
            {
                throw new System.NotImplementedException();
            }

            public void CaseFailed(FailResult result)
            {
                throw new System.NotImplementedException();
            }

            public void AssemblyCompleted(Assembly assembly, AssemblyResult result)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}