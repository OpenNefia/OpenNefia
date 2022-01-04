using OpenNefia.Content.GameObjects;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Locale.Funcs
{
    [RegisterLocaleFunctions("en_US")]
    public static class EnglishBuiltins
    {
        /// <summary>
        /// Function: s(entity, needE)
        /// </summary>
        /// <hsp>#defcfunc _s int tg, int op</hsp>
        [LocaleFunction("s")]
        public static string BuiltIn_s(object? obj, bool needE = false)
        {
            switch (obj)
            {
                case int objInt:
                    if (objInt > 1)
                        return "";
                    else
                        return "s";

                case EntityUid objEntity:
                    var entMan = IoCManager.Resolve<IEntityManager>();

                    if (entMan.TryGetComponent(objEntity, out StackComponent stack))
                    {
                        if (stack.Count > 1)
                            return "";
                        else
                            return "s";
                    }

                    var gameSession = IoCManager.Resolve<IGameSessionManager>();

                    if (gameSession.IsPlayer(objEntity))
                        return "";
                    else if (needE)
                        return "es";
                    else
                        return "s";

                default:
                    return "s";
            }
        }

        /// <summary>
        /// Function: have(entity, needE)
        /// </summary>
        /// <hsp>#defcfunc have int have_charid</hsp>
        [LocaleFunction("have")]
        public static string BuiltIn_have(object? obj, bool needE = false)
        {
            switch (obj)
            {
                case EntityUid objEntity:
                    var gameSession = IoCManager.Resolve<IGameSessionManager>();

                    if (gameSession.IsPlayer(objEntity))
                        return "have";
                    else if (needE)
                        return "has";
                    else
                        return "has";

                default:
                    return "has";
            }
        }
    }
}
