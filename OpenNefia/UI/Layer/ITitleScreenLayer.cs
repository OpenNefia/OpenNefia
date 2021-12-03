namespace OpenNefia.Core.UI.Layer
{
    public enum TitleScreenAction
    {
        ReturnToTitle,
        StartGame,
        Quit
    }

    public record TitleScreenResult(TitleScreenAction Action);

    public interface ITitleScreenLayer : IUiLayerWithResult<TitleScreenResult>
    {
    }
}