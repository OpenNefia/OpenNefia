using OpenNefia.Content.Logic;
using OpenNefia.Content.UI;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Levels
{
    public interface ILevelSystem : IEntitySystem
    {
        int GetLevel(EntityUid entity, LevelComponent? levelComp = null);
    }

    public sealed class LevelSystem : EntitySystem, ILevelSystem
    {
        public int GetLevel(EntityUid entity, LevelComponent? levelComp = null)
        {
            if (!Resolve(entity, ref levelComp))
                return 1;

            return levelComp.Level;
        }
    }
}
