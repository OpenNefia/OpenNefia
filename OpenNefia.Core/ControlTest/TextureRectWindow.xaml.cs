using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.UserInterface.XAML;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.ControlTest
{
    public partial class TextureRectWindow : DefaultWindow
    {
        private static PrototypeId<AssetPrototype> AssetId = new("Elona.G1");

        public TextureRectWindow()
        {
            OpenNefiaXamlLoader.Load(this);

            foreach (var stretchMode in EnumHelpers.EnumerateValues<TextureRect.StretchMode>())
            {
                var textureRect = new TextureRect()
                {
                    Texture = Assets.Get(new("Elona.G2")),
                    Stretch = stretchMode,
                    ExactSize = (300, 300),
                };

                var stretchModeName = Enum.GetName(typeof(TextureRect.StretchMode), stretchMode); ;

                var label = new Label()
                {
                    Text = $"{nameof(TextureRect.StretchMode)}={stretchModeName}",
                    VerticalAlignment = VAlignment.Bottom,
                    Margin = new Thickness(5, 0, 0, 0)
                };

                var container = new BoxContainer()
                {
                    Orientation = BoxContainer.LayoutOrientation.Horizontal,
                    VerticalExpand = true,
                    HorizontalExpand = true,
                    Margin = new Thickness(0, 10)
                };

                container.AddChild(textureRect);
                container.AddChild(label);

                TextureContainer.AddChild(container);
            }
        }
    }
}
