using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI
{
    public static class UiHelpers
    {
        // NOTE: Extremely expensive, cache this ASAP.
        private static IEnumerable<AbstractFieldInfo> GetChildAnnotatedFields(UiElement elem)
        {
            return elem.GetType().GetAllPropertiesAndFields()
                .Where(info => info.HasAttribute<ChildAttribute>());
        }

        /// <summary>
        /// Mapping from <see cref="UiElement"/> type to all fields/properties on that type
        /// annotated with <see cref="ChildAttribute"/>.
        /// </summary>
        private static Dictionary<Type, List<AbstractFieldInfo>> _childFieldsCache = new();

        public static void AddChildrenFromAttributesRecursive(UiElement parent)
        {
            if (!_childFieldsCache.TryGetValue(parent.GetType(), out var fields))
            {
                fields = GetChildAnnotatedFields(parent).ToList();
                _childFieldsCache[parent.GetType()] = fields;
            }

            foreach (var info in fields)
            {
                var child = info.GetValue(parent);
                if (child is not UiElement childElem)
                {
                    if (child != null)
                        Logger.WarningS("ui", $"Could not add child '{info.Name}' ({child}) to parent {nameof(UiElement)} {parent}");
                    continue;
                }

                if (childElem.Parent != null)
                    continue;

                parent.AddChild(childElem);
            }

            foreach (var child in parent.Children)
            {
                AddChildrenFromAttributesRecursive(child);
            }
        }

        public static void AddChildrenRecursive(this UiElement parent, UiElement child)
        {
            parent.AddChild(child);
            AddChildrenFromAttributesRecursive(child);
        }

        public class UiBarDrawableState
        {
            public UiBarDrawableState(IAssetInstance assetInstance, float hpRatio, Vector2 screenPos)
            {
                Asset = assetInstance;
                HPRatio = hpRatio;
                ScreenPos = screenPos;
                BarQuad = Love.Graphics.NewQuad(0, 0, assetInstance.PixelWidth, assetInstance.PixelHeight, assetInstance.PixelWidth, assetInstance.PixelHeight);
                BarWidth = -1;
            }

            public IAssetInstance Asset { get; }
            public float HPRatio { get; }

            /// <summary>
            /// Screen position in virtual pixels.
            /// </summary>
            public Vector2 ScreenPos { get; }

            public Love.Quad BarQuad { get; }

            public float BarWidth { get; set; }
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

                // Dashes and the like should stick to the word occuring before it. Whitespace doesn't have to.
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

        public static void DrawPercentageBar(float uiScale, UiBarDrawableState entry, Vector2 pos, float barWidth, Vector2 drawSize = default)
        {
            var size = entry.Asset.VirtualSize(uiScale);
            var lastWidth = barWidth * uiScale;

            if (!MathHelper.CloseToPercent(entry.BarWidth, barWidth))
            {
                entry.BarWidth = barWidth * uiScale;
                entry.BarQuad.SetViewport(size.X * uiScale - barWidth * uiScale, 0, lastWidth, entry.Asset.PixelHeight * uiScale);
            }

            entry.Asset.Draw(uiScale, entry.BarQuad, pos.X, pos.Y, drawSize.X, drawSize.Y);
        }

        public static string FormatPowerText(int grade, bool noBrackets = false)
        {
            grade = Math.Abs(grade);
            var gradeUnit = Loc.GetString("Elona.UI.Misc.PowerLevel");
            var s = string.Empty;

            for (int i = 0; i < grade; i++)
            {
                if (i >= 4)
                {
                    s += "+";
                    break;
                }

                s += gradeUnit;
            }

            if (!noBrackets)
            {
                s = "[" + s + "]";
            }

            return s;
        }
    }
}
