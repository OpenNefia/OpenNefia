using Love;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace OpenNefia.Core.Rendering
{
    [DataDefinition]
    public class ImageFilter
    {
        [DataField]
        public Love.FilterMode Min = Love.FilterMode.Linear;

        [DataField]
        public Love.FilterMode Mag = Love.FilterMode.Linear;

        [DataField]
        public int Anisotropy = 1;

        public ImageFilter()
        {
        }

        public ImageFilter(FilterMode min, FilterMode mag, int anisotropy)
        {
            Min = min;
            Mag = mag;
            Anisotropy = anisotropy;
        }
    }
}
