using System.Collections.Generic;
using Fixie.Execution;
using System;
using System.Linq;
using System.Reflection;

namespace Fixie.Listeners
{
    public class ListenerFactory
    {
        public Listener CreateListener(ILookup<string, string> options)
        {
            return GetCustomListenerOrNull(options)
                   ?? GetTeamCityListenerOrNull(options)
                   ?? new ConsoleListener();
        }

        static Listener GetCustomListenerOrNull(ILookup<string, string> options)
        {
            var customListenerSpecified = options.Contains(CommandLineOption.CustomListener);

            if (!customListenerSpecified) return null;

            var listeners = new List<Listener>();
            foreach (var option in options[CommandLineOption.CustomListener])
            {
                var parts = option.Split(new[] { ';' }, StringSplitOptions.None);
                if (parts.Length != 2)
                {
                    var message = string.Format("Valid {0} format is 'assembly-path;type'.", CommandLineOption.CustomListener);
                    throw new FormatException(message);
                }

                var path = parts[0];
                var typeName = parts[1];

                var assembly = Assembly.LoadFrom(path);
                var type = assembly.GetType(typeName, true);
                var listener = (Listener)Activator.CreateInstance(type);
                listeners.Add(listener);
            }

            return listeners.Count == 1
                       ? listeners.Single()
                       : new CompoundListener(listeners);
        }

        static Listener GetTeamCityListenerOrNull(ILookup<string, string> options)
        {
            var teamCityExplicitlySpecified = options.Contains(CommandLineOption.TeamCity);

            var runningUnderTeamCity = Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null;

            var useTeamCityListener =
                (teamCityExplicitlySpecified && options[CommandLineOption.TeamCity].First() == "on") ||
                (!teamCityExplicitlySpecified && runningUnderTeamCity);

            return useTeamCityListener ? new TeamCityListener() : null;
        }
    }
}