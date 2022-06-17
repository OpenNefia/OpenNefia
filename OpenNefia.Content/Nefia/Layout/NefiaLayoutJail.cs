using OpenNefia.Content.Levels;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Directions;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Nefia.Layout;

namespace OpenNefia.Content.Nefia
{
    /// <summary>
    /// Same as <see cref="NefiaLayoutResident"/> but wider.
    /// </summary>
    public class NefiaLayoutJail : IVanillaNefiaLayout
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly INefiaLayoutCommon _nefiaLayout = default!;
        [Dependency] private readonly IRandom _rand = default!;

        private NefiaLayoutResident _resident = new();

        public IMap? Generate(IArea area, MapId mapId, int generationAttempt, int floorNumber, Blackboard<NefiaGenParams> data)
        {
            data.Get<BaseNefiaGenParams>().MapSize = (48 + _rand.Next(20), 22);

            EntitySystem.InjectDependencies(_resident);
            return _resident.Generate(area, mapId, generationAttempt, floorNumber, data);
        }
    }
}