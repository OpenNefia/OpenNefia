using OpenNefia.Content.Actions;
using OpenNefia.Content.Charas;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.Spells;
using OpenNefia.Content.Visibility;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.VisualAI.Locale
{
    [RegisterLocaleFunctions("VisualAI")]
    public static class VisualAISharedBuiltins
    {
        [LocaleFunction("formatSpell")]
        public static string BuiltIn_FormatSpell(object? obj)
        {
            var protos = IoCManager.Resolve<IPrototypeManager>();
            if (!protos.TryIndex(typeof(SpellPrototype), $"{obj}", out var proto))
                return $"<unknown prototype {obj}>";

            return Loc.GetPrototypeString(((SpellPrototype)proto).SkillID, "Name");
        }

        [LocaleFunction("formatAction")]
        public static string BuiltIn_FormatAction(object? obj)
        {
            var protos = IoCManager.Resolve<IPrototypeManager>();
            if (!protos.TryIndex(typeof(ActionPrototype), $"{obj}", out var proto))
                return $"<unknown prototype {obj}>";

            return Loc.GetPrototypeString(((ActionPrototype)proto).SkillID, "Name");
        }

        [LocaleFunction("formatEnum")]
        public static string BuiltIn_FormatEnum(object? obj)
        {
            if (obj is not Enum val)
                return "<???>";

            return Loc.GetString($"VisualAI.Types.{val.GetType().Name}.{val}");
        }
    }
}