using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Enchantments
{
    public interface IEnchantmentComponent : IComponent
    {
        string? Description { get; }

        bool CanMergeWith(IEnchantmentComponent other)
        {
            return this.GetType() == other.GetType();
        }
    }
}
