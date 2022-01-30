namespace OpenNefia.Core
{
    public struct LocaleKey
    {
        public string Key = string.Empty;

        public static readonly LocaleKey Empty = new("");

        public LocaleKey(string key)
        {
            Key = key;
        }

        public LocaleKey With(string other)
        {
            if (Key == string.Empty)
            {
                return new LocaleKey(other);
            }

            return new LocaleKey(Key + "." + other);
        }

        public LocaleKey GetParent()
        {
            if (Key == String.Empty)
                return Empty;

            int index = Key.LastIndexOf(".");
            if (index < 0)
                return Empty;

            return new LocaleKey(Key.Substring(0, index));
        }

        public override string ToString() => Key;

        public static implicit operator string(LocaleKey key) => key.Key;
        public static implicit operator LocaleKey(string key) => new LocaleKey(key);
    }
}