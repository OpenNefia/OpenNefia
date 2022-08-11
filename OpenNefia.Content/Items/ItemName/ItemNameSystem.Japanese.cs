using OpenNefia.Content.Items;
using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.DisplayName
{
    public sealed partial class ItemNameSystem
    {
        public string ItemNameJP(EntityUid uid,
            ItemComponent? item = null,
            MetaDataComponent? meta = null,
            StackComponent? stack = null)
        {
            if (!Resolve(uid, ref item, ref meta, ref stack))
                return $"<item {uid}>";

            var basename = meta.DisplayName;

            if (stack.Count == 1)
                return $"{basename}";

            return $"{stack.Count}個の{basename}";
        }
    }
}
