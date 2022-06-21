using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;

namespace OpenNefia.Content.TitleScreen
{
    public enum TitleScreenAction
    {
        ReturnToTitle,
        RestoreSave,
        Generate,
        Options,
        Quit,
        QuickStart,
    }

    public record TitleScreenResult(TitleScreenAction Action);

    public interface ITitleScreenLayer : IUiLayerWithResult<UINone, TitleScreenResult>
    {
    }
}