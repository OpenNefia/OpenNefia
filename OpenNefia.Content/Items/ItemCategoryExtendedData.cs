using OpenNefia.Content.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Items
{
    public enum ItemCategoryType
    {
        Major,
        Minor
    }

    public sealed class ExtTagItemCategory : IPrototypeExtendedData<TagPrototype>
    {
        [DataField(required: true)]
        public ItemCategoryType CategoryType { get; set; }
    }
}