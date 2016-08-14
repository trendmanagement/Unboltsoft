using System.Collections.Generic;
using Newtonsoft.Json;

namespace ICE_Import
{
    /// <summary>
    /// The class that provides a schema for JSON configuration file deserializer.
    /// Also, it contains some functionality for data validation.
    /// </summary>
    public class JsonConfig
    {
        [JsonProperty(Required = Required.Always)]
        public string ICE_ProductName;

        [JsonProperty(Required = Required.Always)]
        public string TMLDB_Description;

        [JsonProperty(Required = Required.DisallowNull)]
        public string[] Regular_Options;

        private static HashSet<string> ValidMonths = new HashSet<string>() { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        /// <summary>
        /// Validate JSON file contents.
        /// </summary>
        /// <param name="futuresOnly">Whether "Futures Only" checkbox is checked.</param>
        /// <returns>An error message or null if no errors.</returns>
        public string Validate(bool futuresOnly)
        {
            if (futuresOnly)
            {
                return null;
            }
            else if (Regular_Options == null)
            {
                return "Required property 'ICE_Configuration.Regular_Options' not found in JSON.";
            }

            foreach (string month in Regular_Options)
            {
                if (!ValidMonths.Contains(month))
                {
                    return string.Format(
                        "Unexpected string encountered while parsing month: '{0}'. Path 'ICE_Configuration.Regular_Options'.",
                        month);
                }
            }
            return null;
        }
    }
}
