using OpenNefia.Content.DisplayName;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Dialog
{
    [ImplicitDataDefinitionForInheritors]
    public abstract class DialogFormatData
    {
        public abstract string GetFormatText(DialogContextData context);
    }

    public sealed class PlayerNameFormat : DialogFormatData
    {
        public override string GetFormatText(DialogContextData context)
        {
            var nameSys = EntitySystem.Get<IDisplayNameSystem>();
            return nameSys?.GetDisplayName(GameSession.Player) ?? string.Empty;
        }
    }

    public sealed class ContextFormat : DialogFormatData
    {
        [DataField(required: true)]
        public string Key { get; } = "";

        public override string GetFormatText(DialogContextData context)
        {
            return $"{context.Ensure<object>(Key)}";
        }
    }
}
