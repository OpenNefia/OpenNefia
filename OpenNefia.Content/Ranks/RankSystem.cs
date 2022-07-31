using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.UI;
using OpenNefia.Content.World;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Ranks
{
    public interface IRankSystem : IEntitySystem
    {
        IEnumerable<(PrototypeId<RankPrototype>, Rank)> EnumerateRanks();
        string? GetRankTitle(PrototypeId<RankPrototype> rankId, int? place);
        Rank GetRank(PrototypeId<RankPrototype> id);
        void SetRank(PrototypeId<RankPrototype> rankId, int newExp, bool showMessage = true);
        void ModifyRank(PrototypeId<RankPrototype> rankId, int expDelta, int? placeLimit = null, bool showMessage = true);
    }

    [DataDefinition]
    public sealed class Rank
    {
        public const int ExpPerRankPlace = 100;
        public const int MinRankPlace = 1;
        public const int MaxRankPlace = 100;
        public const int MinRankExp = ExpPerRankPlace * MinRankPlace;
        public const int MaxRankExp = ExpPerRankPlace * MaxRankPlace;

        public Rank() {}

        public Rank(int experience, GameTimeSpan? timeUntilDecay)
        {
            Experience = experience;
            TimeUntilDecay = timeUntilDecay;
        }

        /// <summary>
        /// Current rank experience, from 1-10000. Lower numbers are higher ranks.
        /// </summary>
        /// 
        [DataField("experience")]
        private int _experience = MaxRankExp;

        public int Experience
        {
            get => _experience;
            set => _experience = Math.Clamp(value, MinRankExp, MaxRankExp);
        }

        /// <summary>
        /// Place of this rank. One place is gained for every 100 experience.
        /// </summary>
        public int Place => Experience / ExpPerRankPlace;

        [DataField]
        public GameTimeSpan? TimeUntilDecay { get; set; }
    }

    [DataDefinition]
    public sealed class RankData : Dictionary<PrototypeId<RankPrototype>, Rank> {}

    public sealed class RankSystem : EntitySystem, IRankSystem
    {
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        [RegisterSaveData("Elona.RankSystem.RankData")]
        private RankData RankData { get; } = new();

        public override void Initialize()
        {
            SubscribeEntity<MapOnTimePassedEvent>(ProcRankDecay, priority: EventPriorities.VeryLow);
        }

        private void ProcRankDecay(EntityUid uid, ref MapOnTimePassedEvent args)
        {
            if (args.DaysPassed <= 0)
                return;

            foreach (var (id, rank) in EnumerateRanks())
            {
                var proto = _protos.Index(id);
                if (proto.DecayPeriodDays != null && rank.TimeUntilDecay != null)
                {
                    rank.TimeUntilDecay = rank.TimeUntilDecay.Value - GameTimeSpan.FromHours(args.DaysPassed);
                    if (rank.TimeUntilDecay.Value.TotalSeconds < 0)
                    {
                        ModifyRank(id, -(rank.Experience / 12 + 100));
                        rank.TimeUntilDecay = GameTimeSpan.FromDays(proto.DecayPeriodDays.Value);
                    }
                }
            }
        }

        public IEnumerable<(PrototypeId<RankPrototype>, Rank)> EnumerateRanks()
        {
            // Enumerate in prototype order.
            foreach (var rank in _protos.EnumeratePrototypes<RankPrototype>())
            {
                var id = rank.GetStrongID();
                yield return (id, GetRank(id));
            }
        }

        public string? GetRankTitle(PrototypeId<RankPrototype> rankId, int? place)
        {
            if (place == null)
                place = GetRank(rankId).Place;

            int index;
            if (place == 1)
                index = 0;
            else if (place <= 5)
                index = 1;
            else if (place <= 10)
                index = 2;
            else if (place <= 80)
                index = place.Value / 15 + 3;
            else
                index = 9;

            if (!Loc.TryGetPrototypeString(rankId, $"Titles.{index}", out var title))
                return null;

            return title;
        }

        public Rank GetRank(PrototypeId<RankPrototype> rankId)
        {
            if (!RankData.ContainsKey(rankId))
            {
                var rankData = new Rank();
                RankData[rankId] = rankData;
            }
            return RankData[rankId];
        }

        public void SetRank(PrototypeId<RankPrototype> rankId, int newExp, bool showMessage = true)
        {
            SetRank(rankId, new Rank(newExp, GetRank(rankId).TimeUntilDecay), showMessage);
        }

        public void SetRank(PrototypeId<RankPrototype> rankId, Rank newRank, bool showMessage = true)
        {
            var oldRank = GetRank(rankId);

            RankData[rankId] = newRank;

            if (!showMessage)
                return;

            var oldPlace = oldRank.Place;
            var newPlace = newRank.Place;

            if (oldPlace != newPlace)
            {
                Color color;

                // 1st rank is better than 10th rank.
                if (newRank.Experience < oldRank.Experience)
                    color = UiColors.MesGreen;
                else
                    color = UiColors.MesPurple;

                var rankName = Loc.GetPrototypeString(rankId, "Name");
                var title = GetRankTitle(rankId, newPlace);
                _mes.Display(Loc.GetString("Elona.Rank.Changed", ("rankName", rankName), ("oldPlace", oldPlace), ("newPlace", newPlace), ("title", title)), color);
            }
            else if (newRank.Experience < oldRank.Experience)
            {
                _mes.Display(Loc.GetString("Elona.Rank.CloserToNextRank"), UiColors.MesGreen);
            }
        }

        public void ModifyRank(PrototypeId<RankPrototype> rankId, int expDelta, int? placeLimit = null, bool showMessage = true)
        {
            var oldRank = GetRank(rankId);
            var trueExpDelta = expDelta;
            GameTimeSpan? timeUntilDecay = null;

            if (expDelta > 0)
            {
                trueExpDelta = expDelta * ((int)Math.Pow(oldRank.Place + 20, 2)) / 2500;

                var daysUntilDecay = _protos.Index(rankId).DecayPeriodDays;
                if (daysUntilDecay != null)
                    timeUntilDecay = GameTimeSpan.FromDays(daysUntilDecay.Value);

                if (oldRank.Place <= 1)
                    return;

                if (placeLimit != null && trueExpDelta / Rank.ExpPerRankPlace > placeLimit.Value)
                    trueExpDelta = placeLimit.Value * Rank.ExpPerRankPlace;
            }

            SetRank(rankId, new Rank(oldRank.Experience - trueExpDelta, timeUntilDecay), showMessage);
        }
    }
}