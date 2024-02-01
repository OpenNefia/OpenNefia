using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.VisualAI.Block
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class VisualAIVariableAttribute : Attribute
    {
        public VisualAIVariableAttribute() : this(null, null, 1)
        {
        }

        public VisualAIVariableAttribute(object? minValue = null, object? maxValue = null, float incrementAmount = 1f)
        {
            MinValue = minValue;
            MaxValue = maxValue;
            IncrementAmount = incrementAmount;
        }

        public object? MinValue { get; }
        public object? MaxValue { get; }
        public float IncrementAmount { get; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class VisualAIChoicesVariableAttribute : Attribute
    {
        public VisualAIChoicesVariableAttribute(object[] choices)
        {
            Choices = choices;
        }

        public object[] Choices { get; }
    }
}
