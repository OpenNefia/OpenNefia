using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;

namespace OpenNefia.Content.TitleScreen
{
    public enum TitleScreenAction
    {
        ReturnToTitle,
        StartGame,
        Quit
    }

    public record TitleScreenResult(TitleScreenAction Action);

    public interface ITitleScreenLayer : IUiLayerWithResult<UINone, TitleScreenResult>
    {
    }
}