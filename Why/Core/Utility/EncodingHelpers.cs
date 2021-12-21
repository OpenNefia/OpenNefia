using System.Text;

namespace Why.Core.Utility
{
    public static class EncodingHelpers
    {
        /// <summary>
        /// Custom version of <see cref="Encoding.UTF8"/> that DOESN'T do BOMs.
        /// </summary>
        public static readonly Encoding UTF8 = new UTF8Encoding(false);
    }
}
