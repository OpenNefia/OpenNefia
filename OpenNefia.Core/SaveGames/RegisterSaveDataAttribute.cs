using OpenNefia.Core.GameObjects;

namespace OpenNefia.Core.SaveGames
{
    /// <summary>
    /// Indicates this data should be tracked by the <see cref="ISaveGameSerializer"/>. This
    /// attribute must be used within implementers of <see cref="IEntitySystem"/> or an IoC dependency.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class RegisterSaveDataAttribute : Attribute
    {
        public string Key { get; }

        public RegisterSaveDataAttribute(string key)
        {
            Key = key;
        }
    }
}