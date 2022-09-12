namespace OpenNefia.Core.Formulae
{
    public struct Formula
    {
        public string Body = string.Empty;

        public static readonly LocaleKey Empty = new("");

        public Formula(string body)
        {
            Body = body;
        }

        public override bool Equals(object? other)
        {
            return other is Formula otherFormula && Body.Equals(otherFormula.Body);
        }

        public bool Equals(Formula other)
        {
            return Body == other.Body;
        }

        public override int GetHashCode()
        {
            return Body.GetHashCode();
        }

        public static bool operator ==(Formula left, Formula right) =>
            left.Body == right.Body;
        public static bool operator !=(Formula left, Formula right) =>
            !(left == right);

        public override string ToString() => Body;
    }
}