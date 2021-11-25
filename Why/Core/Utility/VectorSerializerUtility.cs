using System.Diagnostics.CodeAnalysis;

namespace Why.Core.Utility
{
    public class VectorSerializerUtility
    {
        private static char[] _separators = {',', 'x'};

        public static bool TryParseArgs(string value, int count, [NotNullWhen(true)] out string[]? args)
        {
            foreach (var separator in _separators)
            {
                args = value.Split(separator);
                if (args.Length == count)
                {
                    return true;
                }

            }

            args = null;
            return false;
        }
    }
}
