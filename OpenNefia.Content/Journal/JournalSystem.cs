using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using System.Text;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Markup;

namespace OpenNefia.Content.Journal
{
    public interface IJournalSystem : IEntitySystem
    {
        ElonaMarkup GetJournalMarkup();
    }

    public sealed class JournalSystem : EntitySystem, IJournalSystem
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;

        public ElonaMarkup GetJournalMarkup()
        {
            var pages = _protos.EnumeratePrototypes<JournalPagePrototype>().ToList();
            var raw = new StringBuilder();
            foreach (var page in pages)
            {
                var ev = new P_JournalPageRenderEvent();
                _protos.EventBus.RaiseEvent(page, ev);
                var rawMarkup = ev.OutElonaMarkup;
                var lineCount = rawMarkup.Count(c => c.Equals('\n')) + 1;

                raw.AppendLine(rawMarkup);
                for (int i = lineCount % JournalLayer.MaxPageLines; i < JournalLayer.MaxPageLines; i++)
                    raw.AppendLine();
            }

            return new ElonaMarkupParser().Parse(raw.ToString());
        }
    }
}