using OpenNefia.Content.Feats;
using OpenNefia.Core.Prototypes;
using static OpenNefia.Content.UI.Element.FeatWindow;

namespace OpenNefia.Content.UI.Element
{
    public interface IFeatWindowBehavior
    {
        IReadOnlyDictionary<PrototypeId<FeatPrototype>, FeatLevel> GetGainedFeats();
        void OnFeatSelected(FeatNameAndDesc.Feat feat);
        int GetNumberOfFeatsAcquirable();
    }
}