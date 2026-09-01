using System;
using System.Collections.Generic;

namespace com.github.lhervier.ksp.shared
{
    /// <summary>
    /// The log levels as a settings UI shows them: the raw option list a combo carries, and the localized
    /// label of one option.
    ///
    /// Both live here rather than in each mod because the labels themselves are shared: their keys are in
    /// KSP-Shared's own localization file, so a mod offering a log level setting never spells out a key
    /// name nor re-derives the list from the enum.
    /// </summary>
    public static class LogLevels
    {
        // One shared localization key per enum name: logLevel_None ... logLevel_Trace.
        private const string KeyPrefix = "logLevel_";

        private static readonly List<string> _names = BuildNames();

        /// <summary>The level names, in the order of the enum: the raw values a combo is filled with.</summary>
        public static IReadOnlyList<string> Names => _names;

        /// <summary>
        /// The localized label of a level, given its raw name. Falls back on the raw name when the level
        /// has no translation, so a new enum value shows up rather than leaving an empty line.
        /// </summary>
        public static string LabelFor(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }
            string localized = ModLocalization.GetString(KeyPrefix + name);
            return string.IsNullOrEmpty(localized) ? name : localized;
        }

        private static List<string> BuildNames()
        {
            var names = new List<string>();
            foreach (LogLevel level in (LogLevel[]) Enum.GetValues(typeof(LogLevel)))
            {
                names.Add(level.ToString());
            }
            return names;
        }
    }
}
