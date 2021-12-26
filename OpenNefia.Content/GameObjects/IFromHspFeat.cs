using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects
{
    /// <summary>
    /// Indicates this component can be converted from a feat
    /// object entry in a 1.22 .obj file.
    /// </summary>
    public interface IFromHspFeat
    {
        void FromHspFeat(int cellObjId, int param1, int param2);
    }
}
