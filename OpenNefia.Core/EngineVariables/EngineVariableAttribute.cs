using OpenNefia.Core.GameObjects;

namespace OpenNefia.Core.EngineVariables
{
    /// <summary>
    /// Defines an engine variable to be loaded. 
    /// This attribute must be used within implementers of <see cref="IEntitySystem"/> or an IoC dependency.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class EngineVariableAttribute : Attribute
    {
        public string Key { get; }

        public EngineVariableAttribute(string key)
        {
            Key = key;
        }
    }
}