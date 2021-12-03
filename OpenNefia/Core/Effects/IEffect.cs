using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.Effects
{
    [ImplicitDataDefinitionForInheritors]
    public interface IEffect
    {
        public void Apply(EntityUid target, MapCoordinates coords, EntityUid source);
    }
}
