using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Core.Prototypes.EntityPrototype>;

namespace OpenNefia.Content.GameObjects
{
    [PrototypeOfEntries]
    public class EntityPrototypeOf
    {
        public static EntityPrototypeId FeatDoor = new(nameof(FeatDoor));
    }
}
