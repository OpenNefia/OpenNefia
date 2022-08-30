using OpenNefia.Core.Areas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Areas
{
    public static class GlobalAreas
    {
        public static readonly GlobalAreaId NorthTyris = new($"Elona.Area{nameof(NorthTyris)}");
        public static readonly AreaFloorId NorthTyris_FloorNorthTyris = new("Elona.FloorNorthTyris", 0);
    }
}
