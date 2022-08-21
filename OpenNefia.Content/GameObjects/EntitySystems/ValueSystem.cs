using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
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

namespace OpenNefia.Content.GameObjects.EntitySystems
{
    public interface IValueSystem : IEntitySystem
    {
        int GetValue(EntityUid uid, ValueComponent? value = null);
    }

    public sealed class ValueSystem : EntitySystem, IValueSystem
    {
        public override void Initialize()
        {
            SubscribeComponent<ValueComponent, EntityRefreshEvent>(Value_Refreshed, priority: EventPriorities.Highest);
        }

        private void Value_Refreshed(EntityUid uid, ValueComponent component, ref EntityRefreshEvent args)
        {
            component.Value.Reset();
        }

        public int GetValue(EntityUid uid, ValueComponent? value = null)
        {
            if (!Resolve(uid, ref value))
                return 0;

            return value.Value.Buffed;
        }
    }
}