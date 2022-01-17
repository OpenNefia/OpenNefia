using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.UI
{
    public static class UiHelpers
    {
        public class DrawEntry : IDisposable
        {
            public DrawEntry(IAssetInstance assetInstance, float hpRatio, Vector2 screenPos)
            {
                Asset = assetInstance;
                HPRatio = hpRatio;
                ScreenPos = screenPos;
                BarQuad = Love.Graphics.NewQuad(0, 0, assetInstance.Width, assetInstance.Height, assetInstance.Width, assetInstance.Height);
                BarWidth = -1;
            }

            public IAssetInstance Asset { get; }
            public float HPRatio { get; }
            public Vector2 ScreenPos { get; }
            public Love.Quad BarQuad { get; }

            public float BarWidth { get; set; }

            public void Dispose()
            {
                BarQuad.Dispose();
            }
        }

        private static char[] splitChars = new char[] { ' ', '　' };

        public static string[] SplitString(string str, PrototypeId<LanguagePrototype> lang)
        {
            switch (lang)
            {
                case var jp when jp == LanguagePrototypeOf.Japanese:
                    return str.Select(x => $"{x}").ToArray();
                default:
                    return SplitStringDefault(str, splitChars);
            }
        }

        private static string[] SplitStringDefault(string str, char[] splitChars)
        {
            List<string> parts = new List<string>();
            int startIndex = 0;
            while (true)
            {
                int index = str.IndexOfAny(splitChars, startIndex);

                if (index == -1)
                {
                    parts.Add(str.Substring(startIndex));
                    return parts.ToArray();
                }

                string word = str.Substring(startIndex, index - startIndex);
                char nextChar = str.Substring(index, 1)[0];
                // Dashes and the likes should stick to the word occuring before it. Whitespace doesn't have to.
                if (char.IsWhiteSpace(nextChar))
                {
                    parts.Add(word);
                    parts.Add(nextChar.ToString());
                }
                else
                {
                    parts.Add(word + nextChar);
                }

                startIndex = index + 1;
            }
        }

        public static void DrawPercentageBar(DrawEntry entry, Vector2 pos, float barWidth, Vector2 drawSize)
        {
            var size = entry.Asset.Size;
            var lastWidth = barWidth;
            if (entry.BarWidth != barWidth)
            {
                entry.BarWidth = barWidth;
                entry.BarQuad.SetViewport(size.X - barWidth, 0, lastWidth, size.Y);
            }

            entry.Asset.Draw(entry.BarQuad, pos.X, pos.Y, drawSize.X, drawSize.Y);
        }
    }
}
