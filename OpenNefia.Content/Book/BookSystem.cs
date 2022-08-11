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
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.Identify;
using OpenNefia.Content.Inventory;

namespace OpenNefia.Content.Book
{
    public interface IBookSystem : IEntitySystem
    {
        TurnResult ReadBook(EntityUid source, EntityUid target, BookComponent? book = null);
    }

    public sealed class BookSystem : EntitySystem, IBookSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IIdentifySystem _identify = default!;

        public override void Initialize()
        {
            SubscribeComponent<BookComponent, EntityBeingGeneratedEvent>(EntityBeingGenerated_Book);
            SubscribeComponent<BookComponent, LocalizeItemNameExtraEvent>(LocalizeExtra_Book);
            SubscribeComponent<BookComponent, GetVerbsEventArgs>(GetVerbs_Book);
        }

        private void EntityBeingGenerated_Book(EntityUid uid, BookComponent component, ref EntityBeingGeneratedEvent args)
        {
            if (!component.BookID.IsValid())
            {
                component.BookID = PickRandomBookID();
            }
        }

        private PrototypeId<BookPrototype> PickRandomBookID()
        {
            var candidates = _protos.EnumeratePrototypes<BookPrototype>()
                .Where(b => b.GenerateRandomly)
                .ToList();
            return _rand.Pick(candidates).GetStrongID();
        }

        private void LocalizeExtra_Book(EntityUid uid, BookComponent component, ref LocalizeItemNameExtraEvent args)
        {
            var identify = CompOrNull<IdentifyComponent>(uid)?.IdentifyState ?? IdentifyState.None;
            if (identify >= IdentifyState.Name)
            {
                var title = Loc.GetPrototypeString(component.BookID, "Title");
                var s = Loc.GetString("Elona.Read.Book.ItemName.Title", ("name", args.OutFullName.ToString()), ("title", title));
                args.OutFullName.Clear().Append(s);
            }
        }

        private void GetVerbs_Book(EntityUid uid, BookComponent component, GetVerbsEventArgs args)
        {
            args.OutVerbs.Add(new Verb(ReadInventoryBehavior.VerbTypeRead, "Read Book", () => ReadBook(args.Source, args.Target)));
        }

        public TurnResult ReadBook(EntityUid source, EntityUid target, BookComponent? book = null)
        {
            if (!Resolve(target, ref book))
                return TurnResult.Aborted;

            _identify.Identify(target, IdentifyState.Name);
            var text = Loc.GetPrototypeString(book.BookID, "Text");
            // TODO book menu
            return TurnResult.Aborted;
        }
    }
}