using OpenNefia.Content.Areas;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Areas
{
    internal interface IGlobalAreaSystem : IEntitySystem
    {
        void InitializeGlobalAreas();
    }

    internal sealed class GlobalAreaSystem : EntitySystem, IGlobalAreaSystem
    {
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        /// <summary>
        /// Does one-time setup of global areas. This is for setting up areas like towns to
        /// be able to generate escort/other quests between them when a new save is being 
        /// initialized.
        /// </summary>
        public void InitializeGlobalAreas()
        {
            foreach (var (areaEntityProto, globalAreaId) in EnumerateGlobalAreas())
            {
                var areaId = areaEntityProto.GetStrongID();
                _areaManager.CreateArea(areaId, globalAreaId);
            }
        }

        private IEnumerable<(EntityPrototype, GlobalAreaId)> EnumerateGlobalAreas()
        {
            foreach (var proto in _protos.EnumeratePrototypes<EntityPrototype>())
            {
                if (proto.Components.TryGetComponent<AreaEntranceComponent>(out var areaEntrance))
                {
                    if (areaEntrance.GlobalId != null)
                    {
                        yield return (proto, areaEntrance.GlobalId.Value);
                    }
                }
            }
        }
    }
}