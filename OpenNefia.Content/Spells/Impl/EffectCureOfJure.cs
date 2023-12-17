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
    public sealed class EffectCureOfJure : EffectHeal
    {
        public EffectCureOfJure() : base()
        {
            Dice = new EffectDice(
                x: new("5 + spellLevel / 10"), 
                y: new("power / 7 + 5"),
                bonus: new("power / 2"),
                extraVariables: new[] {
                    new SpellDiceVariables(Protos.Spell.CureOfEris)
                });
            MessageKey = "Elona.Effect.Heal.Apply.Completely";
        }
    }
}