namespace RoundReports
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Exiled.API.Features;
    using Exiled.API.Interfaces;
    using Exiled.Loader;

    public static class ScriptedEventsIntegration
    {
        /// <summary>
        /// Gets the plugin representing ScriptedEvents, if it's installed.
        /// </summary>
        public static IPlugin<IConfig> ScriptedPlugin => Loader.Plugins.FirstOrDefault(plugin => plugin.Assembly.GetName().Name == "ScriptedEvents");

        /// <summary>
        /// Gets the <see cref="Assembly"/> representing ScriptedEvents, if it's installed.
        /// </summary>
        public static Assembly ScriptedAssembly => ScriptedPlugin?.Assembly ?? null;

        /// <summary>
        /// Gets a value indicating whether or not this server has Scripted Events on it.
        /// </summary>
        public static bool Enabled => ScriptedAssembly is not null;

        /// <summary>
        /// Gets the <see cref="Type"/> representing the "ApiHelper" class of Scripted Events.
        /// </summary>
        public static Type ApiHelper => ScriptedAssembly?.GetType("ScriptedEvents.API.Helpers.ApiHelper");

        public static void AddAction(string name, Func<string[], Tuple<bool, string>> action)
        {
            if (ApiHelper is not null)
            {
                ApiHelper.GetMethod("RegisterCustomAction").Invoke(null, new object[] { name, action });
            }
        }

        public static void AddCustomActions()
        {
            if (!Enabled) return;

            Func<string[], Tuple<bool, string>> addRemarkAction = (string[] arguments) =>
            {
                if (arguments.Length < 1) return new(false, "Missing argument: Remark");

                string contents = string.Join(" ", arguments);
                MainPlugin.Reporter?.AddRemark(contents);
                return new(true, string.Empty);
            };

            Func<string[], Tuple<bool, string>> pauseReporterAction = (string[] arguments) =>
            {
                Log.Debug($"Kill request sent by SCRIPTED_EVENTS.");
                MainPlugin.Reporter?.Kill();
                return new(true, string.Empty);
            };

            AddAction("RR_ADDREMARK", addRemarkAction);
            AddAction("RR_PAUSEREPORTER", pauseReporterAction);
        }
    }
}
