using OpenNefia.Content.DisplayName;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Audio;
using OpenNefia.Content.Items;

namespace OpenNefia.Content.Book
{
    public interface IBookOfRachelSystem : IEntitySystem
    {
        TurnResult ReadBookOfRachel(EntityUid source, EntityUid target, BookOfRachelComponent component);
    }

    public sealed class BookOfRachelSystem : EntitySystem, IBookOfRachelSystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;

        public override void Initialize()
        {
            SubscribeComponent<BookOfRachelComponent, LocalizeItemNameExtraEvent>(LocalizeExtra_BookOfRachel);
            SubscribeComponent<BookOfRachelComponent, EntityBeingGeneratedEvent>(HandleEntityBeingGenerated);
            SubscribeComponent<BookOfRachelComponent, GetVerbsEventArgs>(GetVerbs_BookOfRachel);
        }

        private void LocalizeExtra_BookOfRachel(EntityUid uid, BookOfRachelComponent component, ref LocalizeItemNameExtraEvent args)
        {
            var s = Loc.GetString("Elona.Read.BookOfRachel.ItemName.Title",
                ("name", args.OutFullName.ToString()),
                ("no", component.BookOfRachelNumber));
            args.OutFullName.Clear().Append(s);
        }

        private void HandleEntityBeingGenerated(EntityUid uid, BookOfRachelComponent component, ref EntityBeingGeneratedEvent args)
        {
            if (component.BookOfRachelNumber <= 0)
            {
                component.BookOfRachelNumber = _rand.Next(4) + 1;
            }
        }

        private void GetVerbs_BookOfRachel(EntityUid uid, BookOfRachelComponent component, GetVerbsEventArgs args)
        {
            args.OutVerbs.Add(new Verb(ReadInventoryBehavior.VerbTypeRead, "Read Book of Rachel", () => ReadBookOfRachel(args.Source, args.Target, component)));
        }

        public TurnResult ReadBookOfRachel(EntityUid source, EntityUid target, BookOfRachelComponent component)
        {
            _audio.Play(Protos.Sound.Book1);
            _mes.Display(Loc.GetString("Elona.Read.BookOfRachel.Text"));
            return TurnResult.Succeeded;
        }
    }
}