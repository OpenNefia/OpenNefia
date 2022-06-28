using OpenNefia.Content.RandomGen;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Maps
{
    public interface IMapCharaGenSystem : IEntitySystem
    {
    }

    public sealed class MapCharaGenSystem : EntitySystem, IMapCharaGenSystem
    {
        public override void Initialize()
        {
            SubscribeLocalEvent<MapCharaGenComponent, GetCharaFilterEvent>(SetDefaultFilter);
        }

        private void SetDefaultFilter(EntityUid uid, MapCharaGenComponent component, ref GetCharaFilterEvent args)
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
