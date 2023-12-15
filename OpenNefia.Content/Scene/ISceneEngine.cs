using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Content.Scene
{
    public interface ISceneEngine
    {
        void SetActors(IDictionary<string, ActorSpec> actors);
        void SetBackground(PrototypeId<AssetPrototype> assetID);

        void ShowText(IList<string> text);
        void ShowDialog(string actor, IList<string> text);

        void Wait();
        void FadeOut();
        void FadeIn();
    }
}
