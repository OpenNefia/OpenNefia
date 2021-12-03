namespace OpenNefia.Content.UI.Element.List
{
    public interface IUiListItem
    {
        UiListChoiceKey? GetChoiceKey(int index);
        string GetChoiceText(int index);
    }
}