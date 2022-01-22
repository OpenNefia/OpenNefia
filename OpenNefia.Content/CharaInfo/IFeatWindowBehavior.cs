using OpenNefia.Content.Feats;
using OpenNefia.Core.Prototypes;
using static OpenNefia.Content.CharaInfo.FeatWindow;

namespace OpenNefia.Content.CharaInfo
{
    public interface IFeatWindowBehavior
    {
        IReadOnlyDictionary<PrototypeId<FeatPrototype>, FeatLevel> GetGainedFeats();
        void OnFeatSelected(FeatNameAndDesc.Feat feat);
        int GetNumberOfFeatsAcquirable();
    }
}