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
        public VisualAIVariableAttribute(float minValue = float.NaN, float maxValue = float.NaN, float incrementAmount = 1f)
        {
            // Working around not being able to pass nullable arguments to attribute constructors
            MinValue = float.IsNaN(minValue) ? null : minValue;
            MaxValue = float.IsNaN(maxValue) ? null : maxValue;
            StepAmount = incrementAmount;
        }

        public float? MinValue { get; }
        public float? MaxValue { get; }
        public float StepAmount { get; } = 1f;
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
