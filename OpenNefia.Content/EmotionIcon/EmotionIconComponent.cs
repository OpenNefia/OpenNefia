using OpenNefia.Content.World;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.EmotionIcon
{
    [RegisterComponent]
    public sealed class EmotionIconComponent : Component
    {
        [DataField]
        public string? EmotionIconId { get; set; }

        [DataField]
        public int TurnsRemaining { get; set; }
    }

    public static class EmotionIcons
    {
        public const string Happy = $"Elona.{nameof(Happy)}";
        public const string Silent = $"Elona.{nameof(Silent)}";
        public const string Skull = $"Elona.{nameof(Skull)}";
        public const string Bleed = $"Elona.{nameof(Bleed)}";
        public const string Blind = $"Elona.{nameof(Blind)}";
        public const string Confuse = $"Elona.{nameof(Confuse)}";
        public const string Dim = $"Elona.{nameof(Dim)}";
        public const string Fear = $"Elona.{nameof(Fear)}";
        public const string Sleep = $"Elona.{nameof(Sleep)}";
        public const string Paralyze = $"Elona.{nameof(Paralyze)}";
        public const string Eat = $"Elona.{nameof(Eat)}";
        public const string Heart = $"Elona.{nameof(Heart)}";
        public const string Angry = $"Elona.{nameof(Angry)}";
        public const string Item = $"Elona.{nameof(Item)}";
        public const string Notice = $"Elona.{nameof(Notice)}";
        public const string Question = $"Elona.{nameof(Question)}";
        public const string QuestTarget = $"Elona.{nameof(QuestTarget)}";
        public const string QuestClient = $"Elona.{nameof(QuestClient)}";
        public const string Insane = $"Elona.{nameof(Insane)}";
        public const string Party = $"Elona.{nameof(Party)}";
    }
}
