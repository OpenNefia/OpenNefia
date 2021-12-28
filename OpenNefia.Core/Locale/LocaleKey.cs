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
            if (this.Key == string.Empty)
            {
                return new LocaleKey(other);
            }

            return new LocaleKey(Key + "." + other);
        }

        public override string ToString() => this.Key;

        public static implicit operator string(LocaleKey key) => key.Key;
        public static implicit operator LocaleKey(string key) => new LocaleKey(key);
    }
}