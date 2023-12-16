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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Targetable
{
    public interface ITargetableSystem : IEntitySystem
    {
        /// <summary>
        /// Gets the primary character on this tile.
        /// 
        /// In Elona, traditionally only one character is allowed on each tile. However, extra features
        /// such as the Riding mechanic or the Tag Teams mechanic added in Elona+ allow multiple characters to
        /// occupy the same tile.
        /// 
        /// This function retrieves the "primary" character used for things like
        /// damage calculation, spell effects, and so on, which should exclude the riding mount, tag team
        /// partner, etc.
        /// 
        /// It's necessary to keep track of the non-primary characters on the same tile because they are 
        /// still affected by things like area of effect magic.
        /// </summary>
        bool TryGetBlockingEntity(MapCoordinates coords, [NotNullWhen(true)] out SpatialComponent? spatial);

        bool IsTargetable(EntityUid uid, TargetableComponent? targetable = null);
    }

    public sealed class TargetableSystem : EntitySystem, ITargetableSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        /// <inheritdoc />
        public bool TryGetBlockingEntity(MapCoordinates coords, [NotNullWhen(true)] out SpatialComponent? spatial)
        {
            foreach (var entSpatial in _lookup.GetLiveEntitiesAtCoords(coords))
            {
                if (entSpatial.IsSolid && IsTargetable(entSpatial.Owner))
                {
                    spatial = entSpatial;
                    return true;
                }
            }

            spatial = null;
            return false;
        }

        public bool IsTargetable(EntityUid uid, TargetableComponent? targetable = null)
        {
            // If the component is missing, then the entity can always be targeted
            if (!Resolve(uid, ref targetable, logMissing: false))
                return true;

            return targetable.IsTargetable.Buffed;
        }
    }
}