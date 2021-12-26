using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects
{
    /// <summary>
    /// Indicates this component can be converted from an item
    /// object entry in a 1.22 .obj file.
    /// </summary>
    public interface IFromHspItem
    {
        void FromHspItem(OwnState ownState);
    }
}
