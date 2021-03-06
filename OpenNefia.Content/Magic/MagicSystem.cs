using OpenNefia.Content.Effects;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Logic;
using OpenNefia.Content.UI;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Magic
{
    public interface IMagicSystem : IEntitySystem
    {
        EffectResult Cast(PrototypeId<MagicPrototype> magicId, int power, EntityUid target, EntityUid? source = null, EntityUid? item = null, CurseState curseState = CurseState.Normal);
    }

    public sealed class MagicSystem : EntitySystem, IMagicSystem
    {
        public EffectResult Cast(PrototypeId<MagicPrototype> magicId, int power, EntityUid target, EntityUid? source = null, EntityUid? item = null, CurseState curseState = CurseState.Normal)
        {
            IoCManager.Resolve<IMessagesManager>().Display($"TODO: Cast magic {magicId}", UiColors.MesYellow);
            return EffectResult.Succeeded;
        }
    }
}
