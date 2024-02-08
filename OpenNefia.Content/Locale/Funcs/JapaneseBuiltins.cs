using OpenNefia.Content.Charas;
using OpenNefia.Content.Dialog;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Visibility;
using OpenNefia.Content.World;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Locale.Funcs
{
    [RegisterLocaleFunctions(language: "ja_JP")]
    public static partial class JapaneseBuiltins
    {
        [LocaleFunction("sore_wa")]
        public static string BuiltIn_sore_wa(object? obj, bool ignoreSight = false)
        {
            if (obj is string s)
                return s;

            if (obj is not EntityUid entity)
                return "それは";

            var gameSession = IoCManager.Resolve<IGameSessionManager>();

            if (gameSession.IsPlayer(entity))
                return "";

            var visibilitySys = EntitySystem.Get<VisibilitySystem>();

            if (!visibilitySys.CanSeeEntity(GameSession.Player, entity) && !ignoreSight)
            {
                return "それは";
            }

            return EntitySystem.Get<IDisplayNameSystem>().GetDisplayName(entity) + "は";
        }

        [LocaleFunction("kare_wa")]
        public static string BuiltIn_he(object? obj)
        {
            switch (obj)
            {
                case int objInt:
                    if (objInt == 1)
                        return "それは";
                    else
                        return "それらは";

                case long objLong:
                    if (objLong == 1L)
                        return "それは";
                    else
                        return "それらは";

                case EntityUid objEntity:
                    var gameSession = IoCManager.Resolve<IGameSessionManager>();
                    if (gameSession.IsPlayer(objEntity))
                    {
                        return "あなたは";
                    }

                    var entMan = IoCManager.Resolve<IEntityManager>();

                    if (entMan.TryGetComponent(objEntity, out CharaComponent chara))
                    {
                        switch (chara.Gender)
                        {
                            case Gender.Female:
                                return "彼女は";
                            case Gender.Male:
                                return "彼は";
                            default:
                                return "彼らは";
                        }
                    }

                    if (entMan.TryGetComponent(objEntity, out StackComponent stack))
                    {
                        if (stack.Count != 1)
                            return "それらは";
                    }

                    return "それは";

                default:
                    return "それは";
            }
        }

        /// <hsp>#defcfunc cnvDate int d,int mode</hsp>
        [LocaleFunction("format_date")]
        public static string BuiltIn_format_date(object? obj, string? mode = null)
        {
            if (obj is not GameDateTime dateTime)
                return "";

            var s = $"﻿{dateTime.Year}﻿年{dateTime.Month}﻿月{dateTime.Day}日";

            if (mode == "hour")
                s += $"﻿{dateTime.Hour}時";

            return s;
        }

        [LocaleFunction("theTarget")]
        public static string BuiltIn_theTarget(object? source, object? target)
        {
            if (target is not EntityUid targetEnt)
            {
                if (source == target)
                    return "自分";
                else
                    return "何か";
            }

            if (source is not EntityUid sourceEnt || sourceEnt != targetEnt)
                return SharedBuiltins.BuiltIn_name(target, true);

            return "自分";
        }
    }
}
