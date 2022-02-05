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
    /// <summary>
    /// Classes which allow for game data to be inserted into dialog messages and choices
    /// </summary>
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
            return nameSys?.GetBaseName(GameSession.Player) ?? string.Empty;
        }
    }

    /// <summary>
    /// Takes a string value directly from the dialog context to insert into text
    /// an example would be prices in choices (like investing in a shop)
    /// </summary>
    public sealed class ContextFormat : DialogFormatData
    {
        [DataField(required: true)]
        public string Key { get; } = "";

        public override string GetFormatText(DialogContextData context)
        {
            if (context.TryGet<string>(Key, out var str))
                return str;
            return $"<Key {Key} not present in DialogContextData.>";
        }
    }
}
