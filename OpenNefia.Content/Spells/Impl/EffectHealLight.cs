using OpenNefia.Content.Effects;
using OpenNefia.Content.Prototypes;

namespace OpenNefia.Content.Spells
{
    public sealed class EffectHealLight : EffectHeal
    {
        public EffectHealLight() : base()
        {
            Dice = new EffectDice(
                x: new("1 + spellLevel / 30"), 
                y: new("power / 40 + 5"),
                bonus: new("power / 30"), 
                extraVariables: new[]
                {
                    new SpellDiceVariables(Protos.Spell.SpellHealingTouch)
                });
            MessageKey = "Elona.Effect.Heal.Apply.Slightly";
        }
    }
}