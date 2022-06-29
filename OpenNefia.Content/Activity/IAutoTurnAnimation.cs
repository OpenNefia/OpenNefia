using OpenNefia.Content.Audio;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Content.Prototypes;

namespace OpenNefia.Content.Activity
{
    [ImplicitDataDefinitionForInheritors]
    public interface IAutoTurnAnim
    {
        SoundSpecifier Sound { get; }
    }

    public class MiningAutoTurnAnim : IAutoTurnAnim
    {
        public SoundSpecifier Sound { get; } = new SoundPathSpecifier(Protos.Sound.Dig1);
    }

    public class FishingAutoTurnAnim : IAutoTurnAnim
    {
        public SoundSpecifier Sound { get; } = new SoundPathSpecifier(Protos.Sound.Water);
    }

    public class HarvestingAutoTurnAnim : IAutoTurnAnim
    {
        public SoundSpecifier Sound { get; } = new SoundPathSpecifier(Protos.Sound.Bush1);
    }

    public class SearchingAutoTurnAnim : IAutoTurnAnim
    {
        public SoundSpecifier Sound { get; } = new SoundPathSpecifier(Protos.Sound.Dig2);
    }
}
