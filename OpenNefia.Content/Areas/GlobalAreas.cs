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

        public static readonly GlobalAreaId Vernis = new($"Elona.Area{nameof(Vernis)}");
        public static readonly GlobalAreaId PortKapul = new($"Elona.Area{nameof(PortKapul)}");
        public static readonly GlobalAreaId Yowyn = new($"Elona.Area{nameof(Yowyn)}");
        public static readonly GlobalAreaId Derphy = new($"Elona.Area{nameof(Derphy)}");
        public static readonly GlobalAreaId Palmia = new($"Elona.Area{nameof(Palmia)}");
        public static readonly GlobalAreaId Noyel = new($"Elona.Area{nameof(Noyel)}");
        public static readonly GlobalAreaId Lumiest = new($"Elona.Area{nameof(Lumiest)}");
    }
}
