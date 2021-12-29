using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    /// <summary>
    /// A UI element wrapper around an <see cref="IAssetInstance"/> with its own
    /// position, color, and other properties.
    /// </summary>
    public interface IAssetDrawable : IDrawable, IUiDefaultSizeable
    {
        IAssetInstance Instance { get; set; }
        Color Color { get; set; }
        bool Centered { get; set; }
        float Rotation { get; set; }
        string? RegionId { get; set; }
    }

    public class AssetDrawable : UiElement, IAssetDrawable
    {
        public IAssetInstance Instance { get; set; }
        public Color Color { get; set; } = Color.White;
        public bool Centered { get; set; }
        public float Rotation { get; set; }
        public string? RegionId { get; set; }


        public AssetDrawable(PrototypeId<AssetPrototype> proto, Color? color = null, bool centered = false, float rotation = 0f, string? regionId = null)
        {
            Instance = Assets.Get(proto);
            if (color != null)
                Color = color.Value;
            Centered = centered;
            Rotation = rotation;
            RegionId = regionId;
        }

        public override void GetPreferredSize(out Vector2i size)
        {
            size = Instance.Size;
        }

        public override void Update(float dt)
        {
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(Color);

            if (RegionId != null)
            {
                Instance.DrawRegion(RegionId, X, Y, Width, Height, Centered, Rotation);
            }
            else
            {
                Instance.Draw(X, Y, Width, Height, Centered, Rotation);
            }
        }
    }
}
