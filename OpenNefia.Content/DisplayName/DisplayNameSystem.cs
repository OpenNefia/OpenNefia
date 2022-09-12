using OpenNefia.Content.CustomName;
using OpenNefia.Core.GameObjects;
using System.ComponentModel;

namespace OpenNefia.Content.DisplayName
{
    public interface IDisplayNameSystem : IEntitySystem
    {
        string GetBaseName(EntityUid uid);
        string GetDisplayName(EntityUid uid, bool noArticle = false, int? amount = null);
    }

    public class DisplayNameSystem : EntitySystem, IDisplayNameSystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeComponent<MetaDataComponent, GetBaseNameEventArgs>(GetDefaultBaseName, priority: EventPriorities.Highest);
            SubscribeComponent<MetaDataComponent, GetDisplayNameEventArgs>(GetDefaultDisplayName, priority: EventPriorities.Highest);
        }

        private string GetFallbackName(EntityUid uid)
        {
            return $"<entity {uid}>";
        }

        private void GetDefaultBaseName(EntityUid uid, MetaDataComponent metaData, ref GetBaseNameEventArgs args)
        {
            var baseName = metaData.DisplayName ?? GetFallbackName(uid);

            args.OutBaseName = baseName;
        }

        private void GetDefaultDisplayName(EntityUid uid, MetaDataComponent component, ref GetDisplayNameEventArgs args)
        {
            if (component.NameIsProperNoun)
                args.OutNoArticle = true;
        }

        public string GetBaseName(EntityUid uid)
        {
            var ev = new GetBaseNameEventArgs();
            RaiseEvent(uid, ref ev);
            return ev.OutBaseName;
        }

        public string GetDisplayName(EntityUid uid, bool noArticle = false, int? amount = null)
        {
            var baseName = GetBaseName(uid);
            var ev = new GetDisplayNameEventArgs(baseName, amount, noArticle);
            RaiseEvent(uid, ref ev);
            return ev.OutName;
        }
    }

    [ByRefEvent]
    public struct GetBaseNameEventArgs
    {
        public string OutBaseName { get; set; } = string.Empty;

        public GetBaseNameEventArgs(string baseName)
        {
            OutBaseName = baseName;
        }
    }

    [ByRefEvent]
    public struct GetDisplayNameEventArgs
    {
        public string BaseName { get; }
        public int? AmountOverride { get; }

        public string OutName { get; set; } = string.Empty;
        public bool OutNoArticle { get; set; } = true;

        public GetDisplayNameEventArgs(string baseName, int? amount, bool noArticle)
        {
            BaseName = baseName;
            AmountOverride = amount;
            OutName = baseName;
            OutNoArticle = noArticle;
        }
    }
}
