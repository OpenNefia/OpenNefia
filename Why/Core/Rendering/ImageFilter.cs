using Love;
using OpenNefia.Core.Data.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace OpenNefia.Core.Rendering
{
    public class ImageFilter : IDefDeserializable
    {
        public Love.FilterMode Min = Love.FilterMode.Linear;
        public Love.FilterMode Mag = Love.FilterMode.Linear;
        public int Anisotropy = 1;

        public ImageFilter(FilterMode min, FilterMode mag, int anisotropy)
        {
            Min = min;
            Mag = mag;
            Anisotropy = anisotropy;
        }

        public void DeserializeDefField(IDefDeserializer deserializer, XElement node, Type containingModType)
        {
            deserializer.PopulateAllFields(node, this, containingModType);
        }
    }
}
