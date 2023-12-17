using OpenNefia.Content.StatusEffects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Game;
using OpenNefia.Content.Effects;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Logic;
using OpenNefia.Content.UI;

namespace OpenNefia.Content.Spells
{
    public sealed class EffectHealCritical : EffectHeal
    {
        public EffectHealCritical() : base()
        {
            Dice = new EffectDice(
                x: new("2 + spellLevel / 26"), 
                y: new("power / 25 + 5"),
                bonus: new("power / 15"),
                extraVariables: new[] {
                    new SpellDiceVariables(Protos.Spell.CureOfEris)
                });
            MessageKey = "Elona.Effect.Heal.Apply.Normal";
        }
    }
}