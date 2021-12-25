using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.Effects
{
    [ImplicitDataDefinitionForInheritors]
    public interface IEffect
    {
        public EffectResult Apply(EntityUid source, MapCoordinates coords, EntityUid target, EffectArgs args);
    }

    [DataDefinition]
    public class EffectArgs
    {
        public int Power = 1;
        public CurseState CurseState;

        public override bool Equals(object? obj)
        {
            if (obj is not EffectArgs objArgs)
                return false;

            return Power == objArgs.Power && CurseState == objArgs.CurseState;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Power, CurseState);
        }
    }

    public enum CurseState
    {
        Blessed = 1,
        Normal = 0,
        Cursed = -1,
        Doomed = -2
    }

    public enum EffectResult
    {
        Succeeded = 0,
        Aborted = 1,
        Failed = 2
    }

    public static class EffectResultExt
    {
        public static TurnResult ToTurnResult(this EffectResult result)
        {
            switch (result)
            {
                case EffectResult.Succeeded:
                    return TurnResult.Succeeded;
                case EffectResult.Aborted:
                    return TurnResult.Aborted;
                case EffectResult.Failed:
                default:
                    return TurnResult.Failed;
            }
        }
    }
}
