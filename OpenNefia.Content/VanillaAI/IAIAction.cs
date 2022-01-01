using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.VanillaAI
{
    public interface IAIAction
    {
        public TurnResult Act(IEntityManager entMan, EntityUid actor);
    }
}
