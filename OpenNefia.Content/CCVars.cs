using OpenNefia.Core.Configuration;
using OpenNefia.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content
{   
    /// <summary>
    /// Contains content <see cref="CVar"/>s.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    [CVarDefs]
    public sealed class CCVars : CVars
    {
        /*
         * Anime
         */

        public static readonly CVarDef<float> AnimeAlertWait =
            CVarDef.Create("anime.alertWait", 0.5f);

        /*
         * Debug
         */

        public static readonly CVarDef<bool> DebugShowDetailedSkillPower =
            CVarDef.Create("debug.showDetailedSkillPower", false, CVar.Archive);

        public static readonly CVarDef<bool> DebugShowDetailedResistPower =
            CVarDef.Create("debug.showDetailedResistPower", false, CVar.Archive);
    }
}
