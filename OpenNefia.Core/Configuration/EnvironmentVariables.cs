using System;
using System.Collections.Generic;

namespace OpenNefia.Core.Configuration
{
    public static class EnvironmentVariables
    {
        /// <summary>
        /// The environment variable for configuring CVar overrides. The value
        /// of the variable should be passed as `key=value` separated by
        /// semicolons.
        /// </summary>
        public const string ConfigVarEnvironmentVariable = "OPENNEFIA_CVARS";

        /// <summary>
        /// Get the CVar overrides defined in the relevant environment variable.
        /// </summary>
        public static IEnumerable<(string, string)> GetEnvironmentCVars()
        {
            var eVarString = Environment.GetEnvironmentVariable(ConfigVarEnvironmentVariable);

            if (eVarString == null)
            {
                yield break;
            }

            foreach (var cVarPair in eVarString.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                var pairParts = cVarPair.Split('=', 2);
                yield return (pairParts[0], pairParts[1]);
            }
        }
    }
}
