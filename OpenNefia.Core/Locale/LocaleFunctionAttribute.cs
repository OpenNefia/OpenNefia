namespace OpenNefia.Core.Locale
{
    public class LocaleFunctionAttribute : Attribute
    {
        /// <summary>
        /// Name of this function in Lua.
        /// </summary>
        public string Name { get; }

        public LocaleFunctionAttribute(string name)
        {
            Name = name;
        }

    }
}