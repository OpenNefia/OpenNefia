using OpenNefia.Content.DisplayName;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Audio;
using OpenNefia.Core;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Items.Impl
{
    public interface IMusicDiscSystem : IEntitySystem
    {
        TurnResult PlayMusicDisc(EntityUid source, EntityUid target, MusicDiscComponent? musicDisc = null);
    }

    public sealed class MusicDiscSystem : EntitySystem, IMusicDiscSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IMusicManager _music = default!;

        public override void Initialize()
        {
            SubscribeComponent<MusicDiscComponent, EntityBeingGeneratedEvent>(EntityBeingGenerated_MusicDisc);
            SubscribeComponent<MusicDiscComponent, LocalizeItemNameExtraEvent>(LocalizeExtra_MusicDisc);
            SubscribeComponent<MusicDiscComponent, GetVerbsEventArgs>(GetVerbs_MusicDisc);
        }

        private void EntityBeingGenerated_MusicDisc(EntityUid uid, MusicDiscComponent component, ref EntityBeingGeneratedEvent args)
        {
            if (!component.MusicID.IsValid())
            {
                component.MusicID = PickRandomMusicID();
            }
        }

        private PrototypeId<MusicPrototype> PickRandomMusicID()
        {
            var candidates = _protos.EnumeratePrototypes<MusicPrototype>()
                .Where(p =>
                {
                    if (_protos.TryGetExtendedData<MusicPrototype, ExtMusicDisc>(p, out var extMusicDisc))
                        return extMusicDisc.CanAppearInMusicDiscs;

                    return true;
                })
                .ToList();
            
            return _rand.Pick(candidates).GetStrongID();
        }

        private void LocalizeExtra_MusicDisc(EntityUid uid, MusicDiscComponent component, ref LocalizeItemNameExtraEvent args)
        {
            string info = "???";
            
            if (Loc.TryGetPrototypeString(component.MusicID, "Name", out var musicName))
            {
                info = musicName;
            }
            else
            {
                var proto = _protos.Index(component.MusicID);
                if (proto.HspIds != null && proto.HspIds.TryGetValue(ElonaVariants.Elona122, out var elonaID))
                {
                    info = $"BGM{(elonaID - 50 - 1)}";
                }
            }

            var s = $"{args.OutFullName} <{info}>";
            args.OutFullName.Clear().Append(s);
        }

        private void GetVerbs_MusicDisc(EntityUid uid, MusicDiscComponent component, GetVerbsEventArgs args)
        {
            args.OutVerbs.Add(new Verb(UseInventoryBehavior.VerbTypeUse, "Play Music Disc", () => PlayMusicDisc(args.Source, args.Target)));
        }

        public TurnResult PlayMusicDisc(EntityUid source, EntityUid target, MusicDiscComponent? musicDisc = null)
        {
            if (!Resolve(target, ref musicDisc))
                return TurnResult.Aborted;

            _mes.Display(Loc.GetString("Elona.MusicDisc.YouPlay", ("user", source), ("item", target)));
            if (TryMap(target, out var map) && TryComp<MapCommonComponent>(map.MapEntityUid, out var mapCommon))
            {
                mapCommon.Music = musicDisc.MusicID;
                if (_mapManager.ActiveMap?.Id == map.Id)
                    _music.Play(musicDisc.MusicID);
            }

            return TurnResult.Aborted;
        }
    }
    
    public sealed class ExtMusicDisc : IPrototypeExtendedData<MusicPrototype>
    {
        [DataField]
        public bool CanAppearInMusicDiscs { get; set; } = true;
    }
}