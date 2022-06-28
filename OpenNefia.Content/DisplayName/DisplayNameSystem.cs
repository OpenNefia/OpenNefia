using OpenNefia.Content.CustomName;
using OpenNefia.Core.GameObjects;
using System.ComponentModel;

namespace OpenNefia.Content.DisplayName
{
    public interface IDisplayNameSystem : IEntitySystem
    {
        string GetBaseName(EntityUid uid);
        string GetDisplayName(EntityUid uid);
    }

    public class DisplayNameSystem : EntitySystem, IDisplayNameSystem
    {
        public static readonly SubId HandlerGetDefaultBaseName = new(typeof(DisplayNameSystem), nameof(GetDefaultBaseName));

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<MetaDataComponent, GetBaseNameEventArgs>(GetDefaultBaseName, nameof(GetDefaultBaseName));
        }

        private string GetFallbackName(EntityUid uid)
        {
            return $"<entity {uid}>";
        }

        private void GetDefaultBaseName(EntityUid uid, MetaDataComponent metaData, ref GetBaseNameEventArgs args)
        {
            var baseName = metaData.DisplayName ?? GetFallbackName(uid); 

            if (EntityManager.TryGetComponent(uid, out CustomNameComponent customName))
            {
                baseName = customName.CustomName;
            }

            args.BaseName = baseName;
        }

        public string GetBaseName(EntityUid uid)
        {
            var ev = new GetBaseNameEventArgs();
            RaiseLocalEvent(uid, ref ev);
            return ev.BaseName;
        }

        public string GetDisplayName(EntityUid uid)
        {
            var baseName = GetBaseName(uid);
            var ev = new GetDisplayNameEventArgs() { Name = baseName };
            RaiseLocalEvent(uid, ref ev);
            return ev.Name;
        }
    }

    [ByRefEvent]
    public struct GetBaseNameEventArgs
    {
        public string BaseName = string.Empty;

        public GetBaseNameEventArgs()
        {
        }
    }

    [ByRefEvent]
    public struct GetDisplayNameEventArgs
    {
        public string Name = string.Empty;

        public GetDisplayNameEventArgs()
        {
        }
    }
}
