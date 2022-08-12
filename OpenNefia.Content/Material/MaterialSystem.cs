using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Items;
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

namespace OpenNefia.Content.Material
{
    public interface IMaterialSystem : IEntitySystem
    {
    }

    public sealed class MaterialSystem : EntitySystem, IMaterialSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public override void Initialize()
        {
            SubscribeComponent<MaterialComponent, EntityBeingGeneratedEvent>(PickRandomMaterial, priority: EventPriorities.High);
        }

        private void PickRandomMaterial(EntityUid uid, MaterialComponent material, ref EntityBeingGeneratedEvent args)
        {
            // TODO replace this later!
            if (material.MaterialID == Protos.Material.Metal)
            {
                material.MaterialID = Protos.Material.Mica;
            }
            else if (material.MaterialID == Protos.Material.Soft)
            {
                material.MaterialID = Protos.Material.Paper;
            }

            // TODO value.Value = RecalcValue(uid);
        }
    }
}