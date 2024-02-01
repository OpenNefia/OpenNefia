using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Utility;
using OpenNefia.VisualAI.Block;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.VisualAI.Engine
{
    public static class VisualAIHelpers
    {
        public static Dictionary<string, Dictionary<string, VisualAIVariable>> GetBlockVariables(VisualAIBlock block)
        {
            return GetBlockVariables(block.Proto);
        }

        public static Dictionary<string, Dictionary<string, VisualAIVariable>> GetBlockVariables(VisualAIBlockPrototype proto)
        {
            var vars = new Dictionary<string, Dictionary<string, VisualAIVariable>>();

            void AddVariables(string group, object? target)
            {
                if (target == null)
                    return;

                vars!.Add(group, GetBlockVariablesFromObject(target));
            }

            AddVariables("targetFilter", proto.Target?.Filter);
            AddVariables("targetSource", proto.Target?.Source);
            AddVariables("targetOrdering", proto.Target?.Ordering);
            AddVariables("condition", proto.Condition);
            AddVariables("action", proto.Action);

            return vars;
        }

        private static Dictionary<string, VisualAIVariable> GetBlockVariablesFromObject(object obj)
        {
            var vars = new Dictionary<string, VisualAIVariable>();

            var type = obj.GetType();

            foreach (var property in type.GetAllProperties())
            {
                if (property.TryGetCustomAttribute<VisualAIVariableAttribute>(out var attr))
                {
                    vars.Add(property.Name.ToLowerCamelCase(), new VisualAIVariable(obj, property, attr));
                }
            }

            return vars;
        }

        private static Dictionary<string, Dictionary<string, object?>> ConvertBlockVariablesForLocalization(Dictionary<string, Dictionary<string, VisualAIVariable>> vars)
        {
            var newVars = new Dictionary<string, Dictionary<string, object?>>();

            foreach (var (key, values) in vars)
            {
                var newValues = new Dictionary<string, object?>();
                foreach (var (key2, variable) in values)
                {
                    newValues[key2] = variable.Value;
                }

                newVars.Add(key, newValues);
            }

            return newVars;
        }

        public static string FormatBlockDescription(VisualAIBlock block)
        {
            var variables = GetBlockVariables(block);
            var converted = ConvertBlockVariablesForLocalization(variables);
            return Loc.GetPrototypeString(block.ProtoID, "Name", ("variables", converted));
        }

        public static string FormatBlockDescription(VisualAIBlockPrototype blockProto)
        {
            var variables = GetBlockVariables(blockProto);
            var converted = ConvertBlockVariablesForLocalization(variables);
            return Loc.GetPrototypeString(blockProto, "Name", ("variables", converted));
        }
    }

    public sealed class VisualAIVariable
    {
        public VisualAIVariable(object parent, PropertyInfo property, VisualAIVariableAttribute attribute)
        {
            Attribute = attribute;
            Parent = parent;
            Property = property;
        }

        public object Parent { get; }
        public PropertyInfo Property { get; }
        public VisualAIVariableAttribute Attribute { get; }
        public object? Value => Property.GetValue(Parent);
    }
}
