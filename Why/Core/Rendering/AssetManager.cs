using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.Rendering
{
    public class AssetManager : IAssetManager
    {
        private readonly Dictionary<PrototypeId<AssetPrototype>, AssetDrawable> _assets = new();

        public AssetDrawable GetAsset(AssetPrototype proto)
        {
            return _assets[proto.GetStrongID()];
        }
    }
}
