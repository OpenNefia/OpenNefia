using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.UserInterface
{
    public static class LogicalExtensions
    {
        public static IEnumerable<UiElement> GetSelfAndLogicalAncestors(this UiElement control)
        {
            UiElement? c = control;
            while (c != null)
            {
                yield return c;
                c = c.Parent;
            }
        }
    }
}
