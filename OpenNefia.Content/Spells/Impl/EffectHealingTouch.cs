using OpenNefia.Content.Effects;
using OpenNefia.Content.Prototypes;

namespace OpenNefia.Content.Spells
{
    public sealed class EffectHealingTouch : EffectHeal
    {
        public EffectHealingTouch() : base()
        {
            Dice = new EffectDice(
                x: new("2 + spellLevel / 2"), 
                y: new("power / 18 + 5"),
                bonus: new("power / 10"), 
                extraVariables: new[]
                {
                    new SpellDiceVariables(Protos.Spell.HealingTouch)
                });
            MessageKey = "Elona.Effect.Heal.Apply.Normal";
        }
    }
}