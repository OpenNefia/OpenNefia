using OpenNefia.Content.DeferredEvents;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.SaveLoad;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects
{
    public interface IMarriageSystem : IEntitySystem
    {
        void Marry(EntityUid source, EntityUid target, MarriageComponent? targetMarriage = null);
    }

    public sealed class MarriageSystem : EntitySystem, IMarriageSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IDeferredEventsSystem _deferredEvents = default!;
        [Dependency] private readonly IMusicManager _music = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly ISaveLoadSystem _saveLoad = default!;

        public override void Initialize()
        {
        }

        public void Marry(EntityUid source, EntityUid target, MarriageComponent? targetMarriage = null)
        {
            if (!Resolve(target, ref targetMarriage))
                return;

            targetMarriage.MarriagePartners.Add(target);
            _deferredEvents.Enqueue(() => Event_Marriage(source, target));
        }

        private TurnResult Event_Marriage(EntityUid source, EntityUid target)
        {
            _music.Play(Protos.Music.Wedding);

            var promptArgs = new RandomEventPrompt.Args()
            {
                Title = Loc.GetString("Elona.Marriage.Event.Title"),
                Text = Loc.GetString("Elona.Marriage.Event.Text",
                    ("source", source),
                    ("target", target)),
                Image = Protos.Asset.BgRe14,
                Choices = new List<RandomEventPrompt.Choice>()
                {
                    new RandomEventPrompt.Choice(0, Loc.GetString("Elona.Marriage.Event.Choices.0"))
                }
            };

            _uiManager.Query<RandomEventPrompt, RandomEventPrompt.Args, RandomEventPrompt.Result>(promptArgs);

            for (int i = 0; i < 5; i++)
            {
                var filter = new ItemFilter()
                {
                    MinLevel = _randomGen.CalcObjectLevel(target),
                    Quality = _randomGen.CalcObjectQuality(Qualities.Quality.Good),
                    Tags = new[] { _randomGen.PickTag(Protos.TagSet.ItemChest) },
                };
                _itemGen.GenerateItem(source, filter);
            }

            _itemGen.GenerateItem(source, Protos.Item.PotionOfCureCorruption);
            _itemGen.GenerateItem(source, Protos.Item.PlatinumCoin, amount: _rand.Next(3) + 2);
            _mes.Display(Loc.GetString("Elona.Common.SomethingIsPut"));

            _saveLoad.QueueAutosave();

            return TurnResult.NoResult;
        }
    }
}