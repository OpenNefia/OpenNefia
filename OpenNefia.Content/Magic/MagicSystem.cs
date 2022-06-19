using OpenNefia.Content.Effects;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Magic
{
    public interface IMagicSystem
    {
        EffectResult Cast(PrototypeId<MagicPrototype> magicId, int power, EntityUid target, EntityUid? source = null, EntityUid? item = null, CurseState curseState = CurseState.Normal);
    }

    public sealed class MagicSystem : IMagicSystem
    {
        public EffectResult Cast(PrototypeId<MagicPrototype> magicId, int power, EntityUid target, EntityUid? source = null, EntityUid? item = null, CurseState curseState = CurseState.Normal)
        {
            // TODO
            return EffectResult.Succeeded;
        }
    }
}
