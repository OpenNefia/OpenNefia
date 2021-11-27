using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    public interface IAssetManager
    {
        public void LoadAsset(PrototypeId<AssetPrototype> id);
        public AssetDrawable LoadSizedAsset(PrototypeId<AssetPrototype> id, Vector2i size);
        public AssetDrawable GetAsset(PrototypeId<AssetPrototype> id);
    }
}
