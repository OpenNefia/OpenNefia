using OpenNefia.Core.Log;
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
    public class AssetDrawable : UiElement
    {
        public IAssetInstance Instance { get; set; }
        public Color Color { get; set; } = Color.White;
        public bool Centered { get; set; }
        public float Rotation { get; set; }
        public string? RegionId { get; set; }
        public Vector2 OriginOffset { get; set; }


        public AssetDrawable(PrototypeId<AssetPrototype> proto, Color? color = null, bool centered = false, float rotation = 0f, string? regionId = null, Vector2 originOffset = default)
            : this(Assets.Get(proto), color, centered, rotation, regionId, originOffset)
        {
        }

        public AssetDrawable(IAssetInstance instance, Color? color = null, bool centered = false, float rotation = 0f, string? regionId = null, Vector2 originOffset = default)
        {
            Instance = instance;
            if (color != null)
                Color = color.Value;
            Centered = centered;
            Rotation = rotation;
            RegionId = regionId;
            OriginOffset = originOffset;
        }

        public override void GetPreferredSize(out Vector2 size)
        {
            if (RegionId != null)
            {
                if (Instance.Regions.TryGetValue(RegionId, out var region))
                {
                    size = region.Size;
                }
                else
                {
                    Logger.Warning("asset.drawable", $"No region with ID '{region}' found in asset instance!");
                    size = Vector2i.Zero;
                }
            }
            else
            {
                size = Instance.PixelSize;
            }
        }

        public override void Update(float dt)
        {
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(Color);

            if (RegionId != null)
            {
                Instance.DrawRegion(UIScale, RegionId, X, Y, Width, Height, Centered, Rotation, OriginOffset);
            }
            else
            {
                Instance.Draw(UIScale, X, Y, Width, Height, Centered, Rotation, OriginOffset);
            }
        }
    }
}
