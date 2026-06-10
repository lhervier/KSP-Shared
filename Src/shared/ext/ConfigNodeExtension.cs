using System;

namespace com.github.lhervier.ksp.shared.ext {
    public static class ConfigNodeExtension {
        
        private static readonly ModLogger LOGGER = new ModLogger("ConfigNodeExtension");

        private const string NEWLINE_PLACEHOLDER = "%NEWLINE%";

        /// <summary>
        /// Get an integer value from a config node
        /// </summary>
        /// <param name="node">Config node to get the value from</param>
        /// <param name="valueName">Name of the value to get</param>
        /// <param name="raiseException">True if an exception should be thrown if the value is not found or not a valid integer, false otherwise</param>
        /// <param name="defaultValue">Default value to return if the value is not found or not a valid integer</param>
        /// <returns>The value of the integer</returns>
        private static int GetIntValue(ConfigNode node, string valueName, bool raiseException, int defaultValue) {
            if( !node.HasValue(valueName) ) {
                LOGGER.LogWarning($"{valueName} not found in the bookmark node");
                if( raiseException ) {
                    throw new Exception($"{valueName} not found in the bookmark node");
                }
                return defaultValue;
            }

            if( !int.TryParse(node.GetValue(valueName), out int value) ) {
                LOGGER.LogWarning($"{valueName} is not a valid integer");
                if( raiseException ) {
                    throw new Exception($"{valueName} is not a valid integer");  
                }
                return defaultValue;
            }
            return value;
        }
        public static int GetIntValue(this ConfigNode node, string valueName, int defaultValue = 0) {
            return GetIntValue(node, valueName, false, defaultValue);
        }
        public static int GetMandatoryIntValue(this ConfigNode node, string valueName) {
            return GetIntValue(node, valueName, true, 0);
        }

        /// <summary>
        /// Get an integer value from a config node
        /// </summary>
        /// <param name="node">Config node to get the value from</param>
        /// <param name="valueName">Name of the value to get</param>
        /// <param name="raiseException">True if an exception should be thrown if the value is not found or not a valid integer, false otherwise</param>
        /// <param name="defaultValue">Default value to return if the value is not found or not a valid integer</param>
        /// <returns>The value of the integer</returns>
        private static uint GetUintValue(ConfigNode node, string valueName, bool raiseException, uint defaultValue) {
            if( !node.HasValue(valueName) ) {
                LOGGER.LogWarning($"{valueName} not found in the bookmark node");
                if( raiseException ) {
                    throw new Exception($"{valueName} not found in the bookmark node");
                }
                return defaultValue;
            }

            if( !uint.TryParse(node.GetValue(valueName), out uint value) ) {
                LOGGER.LogWarning($"{valueName} is not a valid unsigned integer");
                if( raiseException ) {
                    throw new Exception($"{valueName} is not a valid unsigned integer");
                }
                return defaultValue;
            }
            return value;
        }
        public static uint GetUintValue(this ConfigNode node, string valueName, uint defaultValue = 0) {
            return GetUintValue(node, valueName, false, defaultValue);
        }
        public static uint GetMandatoryUintValue(this ConfigNode node, string valueName) {
            return GetUintValue(node, valueName, true, 0);
        }

        /// <summary>
        /// Get an integer value from a config node
        /// </summary>
        /// <param name="node">Config node to get the value from</param>
        /// <param name="valueName">Name of the value to get</param>
        /// <param name="raiseException">True if an exception should be thrown if the value is not found or not a valid integer, false otherwise</param>
        /// <param name="defaultValue">Default value to return if the value is not found or not a valid integer</param>
        /// <returns>The value of the integer</returns>
        private static double GetDoubleValue(ConfigNode node, string valueName, bool raiseException, double defaultValue) {
            if( !node.HasValue(valueName) ) {
                LOGGER.LogWarning($"{valueName} not found in the bookmark node");
                if( raiseException ) {
                    throw new Exception($"{valueName} not found in the bookmark node");
                }
                return defaultValue;
            }

            if( !double.TryParse(node.GetValue(valueName), out double value) ) {
                LOGGER.LogWarning($"{valueName} is not a valid double");
                if( raiseException ) {
                    throw new Exception($"{valueName} is not a valid double");
                }
                return defaultValue;
            }
            return value;
        }
        public static double GetDoubleValue(this ConfigNode node, string valueName, double defaultValue = 0) {
            return GetDoubleValue(node, valueName, false, defaultValue);
        }
        public static double GetMandatoryDoubleValue(this ConfigNode node, string valueName) {
            return GetDoubleValue(node, valueName, true, 0);
        }

        /// <summary>
        /// Get a boolean value from a config node
        /// </summary>
        /// <param name="node">Config node to get the value from</param>
        /// <param name="valueName">Name of the value to get</param>
        /// <param name="raiseException">True if an exception should be thrown if the value is not found or not a valid boolean, false otherwise</param>
        /// <param name="defaultValue">Default value to return if the value is not found or not a valid boolean</param>
        /// <returns>The value of the boolean</returns>
        private static bool GetBoolValue(ConfigNode node, string valueName, bool raiseException, bool defaultValue) {
            string value = node.GetValue(valueName);
            if( string.IsNullOrEmpty(value) ) {
                LOGGER.LogWarning($"{valueName} not found in the bookmark node");
                if( raiseException ) {
                    throw new Exception($"{valueName} not found in the bookmark node");
                }
                return defaultValue;
            }

            if( !bool.TryParse(value, out bool valueBool) ) {
                LOGGER.LogWarning($"{valueName} is not a valid boolean");
                if( raiseException ) {
                    throw new Exception($"{valueName} is not a valid boolean");
                }
                return defaultValue;
            }
            return valueBool;
        }
        public static bool GetBoolValue(this ConfigNode node, string valueName, bool defaultValue = false) {
            return GetBoolValue(node, valueName, false, defaultValue);
        }
        public static bool GetMandatoryBoolValue(this ConfigNode node, string valueName) {
            return GetBoolValue(node, valueName, true, false);
        }

        /// <summary>
        /// Get an enum value from a config node
        /// </summary>
        /// <param name="node">Config node to get the value from</param>
        /// <param name="valueName">Name of the value to get</param>
        /// <param name="raiseException">True if an exception should be thrown if the value is not found or not a valid enum, false otherwise</param>
        /// <param name="defaultValue">Default value to return if the value is not found or not a valid enum</param>
        /// <returns>The value of the enum</returns>
        private static T GetEnumValue<T>(ConfigNode node, string valueName, bool raiseException, T defaultValue) where T : Enum {
            string value = node.GetValue(valueName);
            if( string.IsNullOrEmpty(value) ) {
                LOGGER.LogWarning($"{valueName} not found in the bookmark node");
                if( raiseException ) {
                    throw new Exception($"{valueName} not found in the bookmark node");
                }
                return defaultValue;
            }
            return (T) Enum.Parse(typeof(T), value);
        }
        public static T GetEnumValue<T>(this ConfigNode node, string valueName, T defaultValue = default) where T : Enum {
            return GetEnumValue<T>(node, valueName, false, defaultValue);
        }
        public static T GetMandatoryEnumValue<T>(this ConfigNode node, string valueName) where T : Enum {
            return GetEnumValue<T>(node, valueName, true, default(T));
        }

        /// <summary>
        /// Get a string value from a config node
        /// </summary>
        /// <param name="node">Config node to get the value from</param>
        /// <param name="valueName">Name of the value to get</param>
        /// <param name="raiseException">True if an exception should be thrown if the value is not found or not a valid string, false otherwise</param>
        /// <param name="defaultValue">Default value to return if the value is not found or not a valid string</param>
        /// <returns>The value of the string</returns>
        private static string GetStringValue(ConfigNode node, string valueName, bool raiseException, string defaultValue) {
            string value = node.GetValue(valueName);
            if( string.IsNullOrEmpty(value) ) {
                LOGGER.LogWarning($"{valueName} is empty in the bookmark node");
                if( raiseException ) {
                    throw new Exception($"{valueName} is empty in the bookmark node");
                }
                return defaultValue;
            }
            
            // Process multi line
            value = value.Replace(NEWLINE_PLACEHOLDER, "\n");
            return value;
        }
        public static string GetStringValue(this ConfigNode node, string valueName, string defaultValue = "") {
            return GetStringValue(node, valueName, false, defaultValue);
        }
        public static string GetMandatoryStringValue(this ConfigNode node, string valueName) {
            return GetStringValue(node, valueName, true, "");
        }

        // ===============================================================================================

        /// <summary>
        /// Add an unsigned integer value to a config node
        /// </summary>
        /// <param name="node">Config node to add the value to</param>
        /// <param name="valueName">Name of the value to add</param>
        /// <param name="value">Value to add</param>
        /// <param name="defaultValue">Default value to add if the value is 0</param>
        public static void AddUintValue(this ConfigNode node, string valueName, uint value, uint defaultValue = 0) {
            if( value == 0 ) {
                value = defaultValue;
            }
            node.AddValue(valueName, value);
        }

        /// <summary>
        /// Add an integer value to a config node
        /// </summary>
        /// <param name="node">Config node to add the value to</param>
        /// <param name="valueName">Name of the value to add</param>
        /// <param name="value">Value to add</param>
        /// <param name="defaultValue">Default value to add if the value is 0</param>
        public static void AddIntValue(this ConfigNode node, string valueName, int value, int defaultValue = 0) {
            if( value == 0 ) {
                value = defaultValue;
            }
            node.AddValue(valueName, value);
        }

        /// <summary>
        /// Add a double value to a config node
        /// </summary>
        /// <param name="node">Config node to add the value to</param>
        /// <param name="valueName">Name of the value to add</param>
        /// <param name="value">Value to add</param>
        /// <param name="defaultValue">Default value to add if the value is 0</param>
        public static void AddDoubleValue(this ConfigNode node, string valueName, double value, double defaultValue = 0) {
            if( value == 0 ) {
                value = defaultValue;
            }
            node.AddValue(valueName, value);
        }

        /// <summary>
        /// Add a boolean value to a config node
        /// </summary>
        /// <param name="node">Config node to add the value to</param>
        /// <param name="valueName">Name of the value to add</param>
        /// <param name="value">Value to add</param>
        /// <param name="defaultValue">Default value to add if the value is false</param>
        public static void AddBoolValue(this ConfigNode node, string valueName, bool value, bool defaultValue = false) {
            if( value == false ) {
                value = defaultValue;
            }
            node.AddValue(valueName, value.ToString());
        }

        /// <summary>
        /// Set a string value to a config node
        /// </summary>
        /// <param name="node">Config node to set the value to</param>
        /// <param name="valueName">Name of the value to set</param>
        /// <param name="value">Value to set</param>
        /// <param name="defaultValue">Default value to set if the value is empty</param>
        public static void AddStringValue(this ConfigNode node, string valueName, string value, string defaultValue = "") {
            if( string.IsNullOrEmpty(value) ) {
                value = defaultValue;
            }
            
            // Process multi line
            value = value.Replace("\n", NEWLINE_PLACEHOLDER);
            node.AddValue(valueName, value);
        }

        /// <summary>
        /// Add an enum value to a config node
        /// </summary>
        /// <param name="node">Config node to add the value to</param>
        /// <param name="valueName">Name of the value to add</param>
        /// <param name="value">Value to add</param>
        /// <param name="defaultValue">Default value to add if the value is null</param>
        public static void AddEnumValue<T>(this ConfigNode node, string valueName, T value, T defaultValue = default) where T : Enum {
            if( value == null ) {
                value = defaultValue;
            }
            node.AddValue(valueName, value.ToString());
        }



    }
}