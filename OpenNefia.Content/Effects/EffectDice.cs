using OpenNefia.Content.Combat;
using OpenNefia.Content.Spells;
using OpenNefia.Core.Formulae;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Serialization.Manager.Attributes;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Effects
{
    [ImplicitDataDefinitionForInheritors]
    public interface IEffectDice
    {
        IDice GetDice(EntityUid source, EntityUid target, EntityCoordinates coords, EntityUid? verb,
            EffectArgSet args);
    }
    
    public sealed class EffectDice : IEffectDice
    {
        public EffectDice() { }

        public EffectDice(Formula x, Formula y, Formula bonus, IEffectDiceVariables[]? extraVariables = null)
        {
            X = x;
            Y = y;
            Bonus = bonus;
            if (extraVariables != null)
                _extraVariables.AddRange(extraVariables);
        }

        [DataField]
        public Formula X { get; }

        [DataField]
        public Formula Y { get; }

        [DataField]
        public Formula Bonus { get; }

        [DataField("extraVariables")]
        private List<IEffectDiceVariables> _extraVariables { get; } = new();
        public IReadOnlyList<IEffectDiceVariables> ExtraVariables => _extraVariables;

        private readonly Dictionary<string, double> _variables = new();

        public IDice GetDice(EntityUid source, EntityUid target, EntityCoordinates coords, EntityUid? verb,
            EffectArgSet args)
        {
            _variables.Clear();
            _variables.Add("power", args.Power);
            foreach (var variable in _extraVariables)
                variable.InitVariables(source, target, coords, verb, args, _variables);

            var engine = IoCManager.Resolve<IFormulaEngine>();
            var x = (int)engine.Calculate(X, _variables);
            var y = (int)engine.Calculate(Y, _variables);
            var bonus = (int)engine.Calculate(Bonus, _variables);

            return new Dice(x, y, bonus);
        }
    }

    [ImplicitDataDefinitionForInheritors]
    public interface IEffectDiceVariables
    {
        void InitVariables(EntityUid source, EntityUid target, EntityCoordinates coords, EntityUid? verb,
            EffectArgSet args, Dictionary<string, double> variables);
    }

    public sealed class SpellDiceVariables : IEffectDiceVariables
    {
        public SpellDiceVariables() {}

        public SpellDiceVariables(PrototypeId<SpellPrototype> spellID)
        {
            SpellID = spellID;
        }

        [DataField]
        public PrototypeId<SpellPrototype> SpellID { get; }
        
        public void InitVariables(EntityUid source, EntityUid target, EntityCoordinates coords, EntityUid? verb, EffectArgSet args, Dictionary<string, double> variables)
        {
            var _spells = EntitySystem.Get<ISpellSystem>();
            variables.Add("spellLevel", _spells.Level(source, SpellID));
        }
    }
}