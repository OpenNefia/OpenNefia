using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Content.Scene
{
    public interface ISceneEngine
    {
        void SetActors(IDictionary<string, SceneActorSpec> actors);
        void SetBackground(PrototypeId<AssetPrototype> assetID);

        void ShowText(IList<string> text);
        void ShowDialog(IList<SceneDialogText> texts);

        void Wait();
        void FadeOut();
        void FadeIn();
    }
}
