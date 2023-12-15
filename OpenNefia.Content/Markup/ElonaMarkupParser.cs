using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Utility;
using Spectre.Console;
using System.Text.RegularExpressions;
using Color = OpenNefia.Core.Maths.Color;

namespace OpenNefia.Content.Markup
{
    /// <summary>
    /// Parses 1.22's line-based markup format. Each line can have directives
    /// like "&lt;size=12&gt;" at the start (there are no closing tags).
    /// These directives apply to the whole line. Example:
    /// <code>
    /// &lt;size=12&gt;&lt;color=#640064&gt;This is a purple line of text.
    /// &lt;style=bold&gt;This text is bold.
    /// This is plain text.
    /// </code>
    /// </summary>
    public class ElonaMarkupParser
    {
        public ElonaMarkupParser()
        {
        }

        private const int DefaultFontSize = 12;

        public ElonaMarkup Parse(string markupStr)
        {
            var result = new List<ElonaMarkupLine>();

            var lines = markupStr.ReplaceLineEndings("\n").Split("\n").ToList();
            if (lines.Count > 0)
                lines.Pop();

            foreach (string line in lines)
            {
                var parsedText = line.Trim();
                var fontSize = DefaultFontSize;
                var color = Color.Black;
                var style = FontStyle.None;

                if (line.StartsWith("<"))
                {
                    MatchCollection matches = Regex.Matches(line, @"<([^>]+)>");
                    foreach (Match match in matches)
                    {
                        var tagText = match.Groups[1].Value;
                        var split = tagText.Split("=");
                        if (split.Length == 2)
                        {
                            var directive = split[0];
                            var value = split[1];

                            switch (directive)
                            {
                                case "size":
                                    if (int.TryParse(value, out var parsedSize))
                                        fontSize = parsedSize;
                                    break;
                                case "color":
                                    color = Color.FromHex(value, Color.Black);
                                    break;
                                case "style":
                                    if (Enum.TryParse(value.FirstCharToUpper(), out FontStyle newStyle))
                                        style = newStyle;
                                    break;
                                default:
                                    Logger.ErrorS("elonaMarkup", $"Unknown markup directive {directive} = {value}!");
                                    break;
                            }
                        }
                        else
                        {
                            Logger.ErrorS("elonaMarkup", $"Invalid markup directive {tagText}!");
                        }

                        parsedText = parsedText.Replace(match.Value, "");
                    }
                }
                var font = new FontSpec(fontSize, fontSize, color, style: style);
                var parsedLine = new ElonaMarkupLine(parsedText, font);
                result.Add(parsedLine);
            }

            return new ElonaMarkup(result);
        }
    }
}