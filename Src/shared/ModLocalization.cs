using System;
using KSP.Localization;

namespace com.github.lhervier.ksp.shared
{
    /// <summary>
    /// Localization for the Steam Input mod (KSP Localizer).
    /// </summary>
    public static class ModLocalization
    {
        private static readonly ModLogger LOGGER = new ModLogger("ModLocalization");

        public static string GetString(string key, params object[] args)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }
            string tag = $"#LOC_{Constants.ModName}_{key}";
            try
            {
                string formatted = Localizer.Format(tag, args);
                // Localizer.Format returns the tag unchanged when it is not defined.
                // Treat that as "missing" so callers can fall back (e.g. EInput.GetLabel).
                if (formatted == tag)
                {
                    return string.Empty;
                }
                return formatted;
            }
            catch (Exception e)
            {
                LOGGER.LogWarning($"Error formatting localization string '{key}': {e.Message}");
                return $"[{key}]";
            }
        }
    }
}
