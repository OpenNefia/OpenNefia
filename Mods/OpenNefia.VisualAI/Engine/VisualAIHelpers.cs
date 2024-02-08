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
using OpenNefia.Core.Configuration;
using OpenNefia.Core.Serialization.Manager;

namespace OpenNefia.VisualAI.Engine
{
    public sealed record class VisualAIVariableSet(Dictionary<string, Dictionary<string, VisualAIVariable>> Variables);

    public static class VisualAIHelpers
    {
        public static VisualAIBlock CreateBlockFromPrototype(VisualAIBlockPrototype proto, ISerializationManager? ser)
        {
            IoCManager.Resolve(ref ser);

            var target = ser.Copy(proto.Target);
            var condition = ser.Copy(proto.Condition);
            var action = ser.Copy(proto.Action);

            var block = new VisualAIBlock(proto.GetStrongID(), target, condition, action);
            block.InjectDependencies();

            return block;
        }

        public static VisualAIVariableSet GetBlockVariables(IVisualAIVariableTargets block)
        {
            var vars = new Dictionary<string, Dictionary<string, VisualAIVariable>>();

            void AddVariables(string group, object? target)
            {
                if (target == null)
                    return;

                var variables = GetBlockVariablesFromObject(target);
                if (variables.Count == 0)
                    return;

                vars!.Add(group, variables);
            }

            AddVariables("targetFilter", block.Target?.Filter);
            AddVariables("targetSource", block.Target?.Source);
            AddVariables("targetOrdering", block.Target?.Ordering);
            AddVariables("condition", block.Condition);
            AddVariables("action", block.Action);

            return new(vars);
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

        private static Dictionary<string, Dictionary<string, object?>> ConvertBlockVariablesForLocalization(VisualAIVariableSet vars)
        {
            var newVars = new Dictionary<string, Dictionary<string, object?>>();

            foreach (var (key, values) in vars.Variables)
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

        public static string FormatBlockDescription(VisualAIBlock block, VisualAIVariableSet? variables = null)
        {
            variables ??= block.Variables;
            var converted = ConvertBlockVariablesForLocalization(variables);
            return Loc.GetPrototypeString(block.ProtoID, "Name", ("variables", converted));
        }

        public static string FormatBlockDescription(VisualAIBlockPrototype blockProto, VisualAIVariableSet? variables = null)
        {
            variables ??= blockProto.Variables;
            var converted = ConvertBlockVariablesForLocalization(variables);
            return Loc.GetPrototypeString(blockProto, "Name", ("variables", converted));
        }
    }

    /// <summary>
    /// A variable that can be configured in the UI.
    /// Similar to <see cref="BaseConfigMenuUICell"/> but more general in implementation.
    /// </summary>
    public interface IDynamicVariable
    {
        public object Parent { get; }
        public Type Type { get; }
        public object? Value { get; set; }
    }

    public sealed class VisualAIVariable : IDynamicVariable
    {
        public VisualAIVariable(object parent, PropertyInfo property, VisualAIVariableAttribute attribute)
        {
            Attribute = attribute;
            Parent = parent;
            Property = property;
        }

        public object Parent { get; }
        public PropertyInfo Property { get; }
        public Type Type => Property.PropertyType;
        public VisualAIVariableAttribute Attribute { get; }
        public object? Value
        {
            get => Property.GetValue(Parent);
            set => Property.SetValue(Parent, value);
        }
    }
}
