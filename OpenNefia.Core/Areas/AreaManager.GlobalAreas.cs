using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Areas
{
    public sealed partial class AreaManager
    {
        public bool GlobalAreaExists(GlobalAreaId globalId)
        {
            return TryGetGlobalArea(globalId, out _);
        }

        public bool TryGetGlobalArea(GlobalAreaId globalId, [NotNullWhen(true)] out IArea? area)
        {
            area = _areas.FirstOrDefault(area => area.Value.GlobalId == globalId).Value;
            return area != null;
        }

        public IArea GetGlobalArea(GlobalAreaId globalId)
        {
            if (!TryGetGlobalArea(globalId, out var area))
                throw new ArgumentException($"Area with global ID {globalId} was not found.", nameof(globalId));

            return area;
        }
    }
}
