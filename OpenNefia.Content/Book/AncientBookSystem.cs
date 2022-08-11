using OpenNefia.Content.Activity;
using OpenNefia.Content.Damage;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Identify;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Spells;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.Visibility;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Logic;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.SaveGames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Book
{
    public interface IAncientBookSystem : IEntitySystem
    {
        int GetDecodeDifficulty(EntityUid uid, AncientBookComponent? ancientBook = null);
        int GetDecodeDifficulty(int baseDifficulty);
    }

    public sealed class AncientBookSystem : EntitySystem, IAncientBookSystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IStatusEffectSystem _effects = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly IActivitySystem _activities = default!;

        public override void Initialize()
        {
            SubscribeComponent<AncientBookComponent, LocalizeItemNameExtraEvent>(LocalizeExtra_AncientBook);
            SubscribeComponent<AncientBookComponent, EntityBeingGeneratedEvent>(BeingGenerated_AncientBook);
            SubscribeComponent<AncientBookComponent, GetVerbsEventArgs>(GetVerbs_AncientBook);
        }

        public const int MaxAncientBookLevel = 14;

        private void BeingGenerated_AncientBook(EntityUid uid, AncientBookComponent component, ref EntityBeingGeneratedEvent args)
        {
            // >>>>>>>> elona122/shade2/item.hsp:671 		iParam1(ci)=rnd(rnd(limit(objLv/2,1,maxMageBook) ...
            component.DecodeDifficulty = _rand.Next(_rand.Next(Math.Clamp(args.CommonArgs.MinLevel / 2, 1, MaxAncientBookLevel)) + 1);
            // <<<<<<<< elona122/shade2/item.hsp:671 		iParam1(ci)=rnd(rnd(limit(objLv/2,1,maxMageBook) ...
        }

        private void LocalizeExtra_AncientBook(EntityUid uid, AncientBookComponent component, ref LocalizeItemNameExtraEvent args)
        {
            var identify = CompOrNull<IdentifyComponent>(uid)?.IdentifyState ?? IdentifyState.None;
            if (identify >= IdentifyState.Full)
            {
                var title = Loc.GetString($"Elona.Read.AncientBook.ItemName.Titles.{component.DecodeDifficulty}");
                var s = Loc.GetString($"Elona.Read.AncientBook.ItemName.Title",
                    ("name", args.OutFullName.ToString()),
                    ("title", title));
                args.OutFullName = new StringBuilder(s);
            }
            if (component.IsDecoded && Loc.TryGetString("Elona.Read.AncientBook.ItemName.Decoded", out var s, ("name", args.OutFullName.ToString())))
            {
                args.OutFullName = new StringBuilder(s);
            }
            else if (!component.IsDecoded && Loc.TryGetString("Elona.Read.AncientBook.ItemName.Undecoded", out s, ("name", args.OutFullName.ToString())))
            {
                args.OutFullName = new StringBuilder(s);
            }
        }

        private void GetVerbs_AncientBook(EntityUid uid, AncientBookComponent component, GetVerbsEventArgs args)
        {
            args.OutVerbs.Add(new Verb(ReadInventoryBehavior.VerbTypeRead, "Read Ancient Book", () => ReadAncientBook(args.Source, args.Target)));
        }

        public int GetDecodeDifficulty(EntityUid uid, AncientBookComponent? ancientBook = null)
        {
            if (!Resolve(uid, ref ancientBook))
                return 0;

            return GetDecodeDifficulty(ancientBook.DecodeDifficulty);
        }

        public int GetDecodeDifficulty(int baseDifficulty)
        {
            return 50 + baseDifficulty * 50 + baseDifficulty * baseDifficulty * 20;
        }

        private TurnResult ReadAncientBook(EntityUid reader, EntityUid ancientBook)
        {
            if (!TryComp<AncientBookComponent>(ancientBook, out var ancientBookComp))
            {
                return TurnResult.Aborted;
            }

            if (ancientBookComp.IsDecoded)
            {
                _mes.Display(Loc.GetString("Elona.Read.AncientBook.AlreadyDecoded", ("reader", reader), ("book", ancientBook)));
                return TurnResult.Aborted;
            }

            if (_effects.HasEffect(reader, Protos.StatusEffect.Blindness))
            {
                _mes.Display(Loc.GetString("Elona.Read.CannotSee", ("reader", reader)));
                return TurnResult.Aborted;
            }

            if (!_stacks.TrySplit(ancientBook, 1, out var split))
                return TurnResult.Aborted;

            var difficulty = GetDecodeDifficulty(ancientBook, ancientBookComp);
            var turns = difficulty / (2 + _skills.Level(reader, Protos.Skill.Literacy)) + 1;
            var activity = EntityManager.SpawnEntity(Protos.Activity.ReadingAncientBook, MapCoordinates.Global);
            Comp<ActivityReadingAncientBookComponent>(activity).AncientBook = split;
            _activities.StartActivity(reader, activity, turns);

            return TurnResult.Succeeded;
        }
    }
}