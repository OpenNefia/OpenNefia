namespace OpenNefia.Core.Serialization
{
    /// <summary>
    /// Provides a method that gets executed after deserialization is complete and a method that gets executed before serialization
    /// </summary>
    public interface ISerializationHooks
    {
        /// <summary>
        /// Gets executed after deserialization is complete
        /// </summary>
        void AfterDeserialization() {}

        /// <summary>
        /// Gets executed before serialization
        /// </summary>
        void BeforeSerialization() {}

        /// <summary>
        /// Gets executed after deep comparison.
        /// </summary>
        /// <returns></returns>
        bool AfterCompare() { return true; }
    }
}
