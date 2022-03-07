using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.IoC;
using OpenNefia.Core.UI.Wisp;

namespace OpenNefia.Content.UI.Stylesheets
{
    public interface IStylesheetManager
    {
        Stylesheet SheetNano { get; }
        Stylesheet SheetSpace { get; }

        void Initialize();
    }

    public sealed class StylesheetManager : IStylesheetManager
    {
        [Dependency] private readonly IWispManager _wispManager = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;

        public Stylesheet SheetNano { get; private set; } = default!;
        public Stylesheet SheetSpace { get; private set; } = default!;

        public void Initialize()
        {
            SheetNano = new StyleNano(_resourceCache).Stylesheet;

            _wispManager.Stylesheet = SheetNano;
        }
    }
}
