using JetBrains.Annotations;
using OpenNefia.Core.Formulae;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations
{
    [TypeSerializer]
    public class FormulaSerializer : ITypeSerializer<Formula, ValueDataNode>
    {
        public Formula Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null,
            Formula rawValue = default)
        {
            return new Formula(node.Value);
        }

        private Dictionary<string, double> _vars = new();

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            var engine = dependencies.Resolve<IFormulaEngine>();
            
            // Suppress formula error logging.
            var sawmill = Logger.GetSawmill("formulae");
            var oldLevel = sawmill.Level;
            sawmill.Level = LogLevel.Fatal;

            try
            {
                engine.CalculateRaw(new Formula(node.Value), _vars);
                return new ValidatedValueNode(node);
            }
            catch (Jace.ParseException ex)
            {
                return new ErrorNode(node, "Formula parse error: " + ex.Message);
            }
            catch (Exception)
            {
                // Ignore execution errors, they're dependent on runtime values.
            }
            finally
            {
                sawmill.Level = oldLevel;
            }

            return new ValidatedValueNode(node);
        }

        public DataNode Write(ISerializationManager serializationManager, Formula value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return new ValueDataNode(value.Body);
        }

        [MustUseReturnValue]
        public Formula Copy(ISerializationManager serializationManager, Formula source, Formula target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return new(source.Body);
        }

        public bool Compare(ISerializationManager serializationManager, Formula left, Formula right, bool skipHook,
            ISerializationContext? context = null)
        {
            return left == right;
        }
    }
}
