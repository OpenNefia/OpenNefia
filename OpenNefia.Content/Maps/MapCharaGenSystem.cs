using OpenNefia.Content.RandomGen;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Maps
{
    public sealed class MapCharaGenSystem : EntitySystem
    {
        public override void Initialize()
        {
            SubscribeLocalEvent<MapCharaGenComponent, GetCharaFilterEvent>(SetDefaultFilter, nameof(SetDefaultFilter));
        }

        private void SetDefaultFilter(EntityUid uid, MapCharaGenComponent component, GetCharaFilterEvent args)
        {
            if (component.CharaFilterGen != null)
            {
                Logger.DebugS("map.charagen", $"Setting chara filter: {component.CharaFilterGen}");
                EntitySystem.InjectDependencies(component.CharaFilterGen);
                args.CharaFilter = component.CharaFilterGen.GenerateFilter(args.Map);
            }
        }
    }
}
