using OpenNefia.Content.Activity;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Memory;
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
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Book
{
    public interface ISpellbookSystem : IEntitySystem
    {
        public SpellbookReserveStates SpellbookReserveStates { get; }

        bool ProcSpellbookSuccess(EntityUid reader, EntityUid spellbook, int difficulty, int skillLevel);
        void FailToReadSpellbook(EntityUid reader, EntityUid spellbook, int difficulty, int skillLevel);

        /// <summary>
        /// Rolls the spellbook check, and if it fails then run the failure effects on the reader.
        /// </summary>
        bool TryToReadSpellbook(EntityUid reader, EntityUid spellbook, int difficulty, int skillLevel);
        TurnResult ReadSpellbook(EntityUid reader, EntityUid spellbook);
    }

    public enum SpellbookReserveState
    {
        NotReserved,
        Reserved
    }

    [DataDefinition]
    public sealed class SpellbookReserveStates : Dictionary<PrototypeId<EntityPrototype>, SpellbookReserveState> { }

    public sealed class SpellbookSystem : EntitySystem, ISpellbookSystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IStatusEffectSystem _effects = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IDamageSystem _damage = default!;
        [Dependency] private readonly IVisibilitySystem _vis = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly ISpellSystem _spells = default!;
        [Dependency] private readonly IActivitySystem _activities = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        [RegisterSaveData("Elona.SpellbookSystem.ReservedStates")]
        public SpellbookReserveStates SpellbookReserveStates { get; } = new();

        public override void Initialize()
        {
            SubscribeComponent<SpellbookComponent, GetVerbsEventArgs>(GetVerbs_Spellbook);
        }

        private void GetVerbs_Spellbook(EntityUid uid, SpellbookComponent component, GetVerbsEventArgs args)
        {
            args.OutVerbs.Add(new Verb(ReadInventoryBehavior.VerbTypeRead, "Read Spellbook", () => ReadSpellbook(args.Source, args.Target)));
        }

        public TurnResult ReadSpellbook(EntityUid reader, EntityUid spellbook)
        {
            if (_effects.HasEffect(reader, Protos.StatusEffect.Blindness))
            {
                _mes.Display(Loc.GetString("Elona.Read.CannotSee", ("reader", reader)));
                return TurnResult.Aborted;
            }

            if (!_stacks.TrySplit(spellbook, 1, out var split) || !TryComp<SpellbookComponent>(split, out var spellbookComp))
                return TurnResult.Aborted;

            var turns = _protos.Index(spellbookComp.SpellID).Difficulty / (2 + _skills.Level(reader, Protos.Skill.Literacy)) + 1;
            var activity = EntityManager.SpawnEntity(Protos.Activity.ReadingSpellbook, MapCoordinates.Global);
            Comp<ActivityReadingSpellbookComponent>(activity).Spellbook = split;
            _activities.StartActivity(reader, activity, turns);

            return TurnResult.Succeeded;
        }

        /// <inheritdoc/>
        public bool ProcSpellbookSuccess(EntityUid reader, EntityUid spellbook, int difficulty, int skillLevel)
        {
            // >>>>>>>> shade2/calculation.hsp:1079 *calcReadCheck	 ..
            if (_effects.HasEffect(reader, Protos.StatusEffect.Blindness))
                return false;

            if (_effects.HasEffect(reader, Protos.StatusEffect.Confusion) || _effects.HasEffect(reader, Protos.StatusEffect.Dimming))
            {
                if (!_rand.OneIn(4))
                    return false;
            }

            if (_rand.Next(_skills.Level(reader, Protos.Skill.Literacy) * skillLevel * 4 + 250) < _rand.Next(difficulty + 1))
            {
                if (_rand.OneIn(7))
                    return false;
                if (skillLevel * 10 < difficulty && _rand.Next(skillLevel * 10 + 1) < _rand.Next(difficulty + 1))
                    return false;
                if (skillLevel * 20 < difficulty && _rand.Next(skillLevel * 20 + 1) < _rand.Next(difficulty + 1))
                    return false;
                if (skillLevel * 30 < difficulty && _rand.Next(skillLevel * 30 + 1) < _rand.Next(difficulty + 1))
                    return false;
            }
            // <<<<<<<< shade2/calculation.hsp:1094 	if f=true:return true ..

            return true;
        }

        /// <inheritdoc/>
        public void FailToReadSpellbook(EntityUid reader, EntityUid spellbook, int difficulty, int skillLevel)
        {
            // >>>>>>>> shade2/calculation.hsp:1092 	if rnd(4)=0{ ...
            if (_rand.OneIn(4) && TryComp<SkillsComponent>(reader, out var skills))
            {
                _mes.Display(Loc.GetString("Elona.Magic.FailToCast.ManaIsAbsorbed", ("chara", reader)));
                if (_gameSession.IsPlayer(reader))
                    _damage.DamageMP(reader, skills.MaxMP);
                else
                    _damage.DamageMP(reader, skills.MaxMP / 3);
            }

            if (_rand.OneIn(4))
            {
                if (_vis.IsInWindowFov(reader))
                {
                    if (_effects.HasEffect(reader, Protos.StatusEffect.Confusion))
                        _mes.Display(Loc.GetString("Elona.Magic.FailToCast.IsConfusedMore", ("chara", reader)));
                    else
                        _mes.Display(Loc.GetString("Elona.Magic.FailToCast.TooDifficult"));
                }
                _effects.Apply(reader, Protos.StatusEffect.Confusion, 100);
            }

            if (_rand.OneIn(4))
            {
                _mes.Display(Loc.GetString("Elona.Magic.FailToCast.CreaturesAreSummoned", ("chara", reader)));
                var leader = _parties.GetLeaderOrNull(reader) ?? reader;
                var level = _levels.GetLevel(leader);

                if (TryMap(leader, out var map))
                {
                    for (var i = 0; i < 2; i++)
                    {
                        var filter = new CharaFilter()
                        {
                            MinLevel = _randomGen.CalcObjectLevel(level * 3 / 2 + 3),
                            Quality = _randomGen.CalcObjectQuality(Qualities.Quality.Normal),
                        };
                        var spawned = _charaGen.GenerateChara(leader, filter);
                        if (IsAlive(spawned) && _factions.GetRelationTowards(spawned.Value, leader) <= Relation.Enemy)
                            _factions.SetPersonalRelationTowards(spawned.Value, leader, Relation.Dislike);
                    }
                }
            }

            _mes.Display(Loc.GetString("Elona.Magic.FailToCast.DimensionDoorOpens", ("chara", reader)));
            _spells.Cast(Protos.Spell.SpellTeleport, reader);
            // <<<<<<<< shade2/calculation.hsp:1114 	return false ..
        }

        /// <inheritdoc/>
        public bool TryToReadSpellbook(EntityUid reader, EntityUid spellbook, int difficulty, int skillLevel)
        {
            if (ProcSpellbookSuccess(reader, spellbook, difficulty, skillLevel))
                return true;

            FailToReadSpellbook(reader, spellbook, difficulty, skillLevel);
            return false;
        }
    }
}