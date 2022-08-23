using OpenNefia.Content.Damage;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.UI;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Combat
{
    public interface IExplosiveSystem : IEntitySystem
    {
    }

    public sealed class ExplosiveSystem : EntitySystem, IExplosiveSystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;

        public override void Initialize()
        {
            SubscribeComponent<ExplosiveComponent, EntityWoundedEvent>(ProcExplode, priority: EventPriorities.VeryHigh + 30000);
        }

        private void ProcExplode(EntityUid uid, ExplosiveComponent component, ref EntityWoundedEvent args)
        {
            if (!component.IsExplosive.Buffed || !component.ExplodesRandomlyWhenAttacked)
                return;

            if (_rand.Prob(component.ExplodeChance.Buffed))
            {
                component.IsAboutToExplode = true;
                _mes.Display(Loc.GetString("Elona.Explosive.Click"), UiColors.MesLightBlue);
            }
        }
    }
}