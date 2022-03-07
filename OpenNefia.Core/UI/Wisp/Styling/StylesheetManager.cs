using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Utility;
using NLua;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.Log;

namespace OpenNefia.Core.UI.Wisp.Styling
{
    public interface IStylesheetManager
    {
        void Initialize();
    }

    public sealed partial class StylesheetManager : IStylesheetManager
    {
        [Dependency] private readonly IWispManager _wispManager = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;
        [Dependency] private readonly IReflectionManager _reflectionManager = default!;
        [Dependency] private readonly IGraphics _graphics = default!;

        public void Initialize()
        {
            var sheet = ParseStylesheet(new("/Stylesheets/Default.lua"));
            _wispManager.Stylesheet = sheet;

            _graphics.OnWindowFocused += WindowFocusedChanged;

            WatchResources();
        }

        private Lua CreateLuaEnv()
        {
            var lua = new Lua();
            lua.State.Encoding = EncodingHelpers.UTF8;
            AddContentRootsToSearchPath(lua);
            return lua;
        }

        private void AddContentRootsToSearchPath(Lua lua)
        {
            var path = (string)lua["package.path"];
            foreach (var root in _resourceCache.GetContentRoots())
            {
                path += $";{root / "?.lua"}";
            }
            lua["package.path"] = path;
        }

        private void DoContentFile(Lua lua, ResourcePath path)
        {
            var str = _resourceCache.ContentFileReadAllText(path);
            lua.DoString(str);
        }

        private Stylesheet ParseStylesheet(ResourcePath luaFile)
        {
            var rules = new List<StyleRule>();

            using (var lua = CreateLuaEnv())
            {
                DoContentFile(lua, new("/Lua/Core/StylesheetEnv.lua"));
                DoContentFile(lua, new("/Lua/Core/CLRPackage.lua"));
                DoContentFile(lua, luaFile);

                var rulesTable = (LuaTable)lua["_DeclaredRules"];

                foreach (KeyValuePair<object, object> pair in rulesTable)
                {
                    ParseElementRules(rules, (LuaTable)pair.Value);
                }
            }

            return new Stylesheet(rules);
        }

        private void ParseElementRules(List<StyleRule> styleRules, LuaTable tableValue, SelectorElement? parentSelector = null)
        {
            var elementTypeName = tableValue["name"];
            if (elementTypeName is not string elementTypeNameStr)
            {
                throw new StylesheetLoadException($"Could not find element class with type '{elementTypeName}'");
            }

            Type? elementType;
            if (elementTypeNameStr == "_")
            {
                elementType = null;
            }
            else if (!_reflectionManager.TryLooseGetType(elementTypeNameStr, out elementType))
            {
                throw new StylesheetLoadException($"Could not find element class with type '{elementTypeNameStr}'");
            }

            SelectorElement selectorElement;

            if (tableValue["selector"] is LuaTable selectorTable)
            {
                selectorElement = ParseSingleRule(elementType, selectorTable);
            }
            else
            {
                selectorElement = new SelectorElement(elementType, new List<string>(), null, new List<string>());
            }

            Logger.InfoS("stylesheet", $"Parse element rules: {selectorElement} ({parentSelector})");

            if (tableValue["properties"] is not LuaTable propertiesTable)
            {
                throw new StylesheetLoadException($"Missing properties for selector on element '{elementTypeName}'");
            }

            ParseStyleRules(styleRules, selectorElement, propertiesTable, parentSelector);
            ParseNestedForms(styleRules, propertiesTable, selectorElement);
        }

        private void ParseRule(List<StyleRule> styleRules, LuaTable selectorTable, SelectorElement selectorElement)
        {
            var newSelector = ParseSingleRule(null, selectorTable, selectorElement, inheritType: true);

            Logger.InfoS("stylesheet", $"Parse rule: {selectorElement} -> {newSelector}");

            if (selectorTable["properties"] is not LuaTable propertiesTable)
            {
                throw new StylesheetLoadException($"Missing properties for nested rule '{selectorTable["body"]}'");
            }

            ParseStyleRules(styleRules, selectorElement, propertiesTable);
            ParseNestedForms(styleRules, propertiesTable, newSelector);
        }

        private SelectorElement ParseSingleRule(Type? elementType, LuaTable selectorTable, SelectorElement? parentSelector = null, bool inheritType = false)
        {
            var classes = (LuaTable)selectorTable["classes"];
            var pseudos = (LuaTable)selectorTable["pseudos"];

            var elementClasses = classes.Values.Cast<string>().ToHashSet();
            var pseudoClasses = pseudos.Values.Cast<string>().ToHashSet();
            var elementId = selectorTable["elementId"] as string;

            if (parentSelector != null)
            {
                if (inheritType)
                    elementType = parentSelector.ElementType;

                if (parentSelector.ElementClasses != null)
                    elementClasses.AddRange(parentSelector.ElementClasses);

                if (parentSelector.PseudoClasses != null)
                    pseudoClasses.AddRange(parentSelector.PseudoClasses);

                if (parentSelector.ElementId != null)
                {
                    if (elementId != null)
                    {
                        throw new StylesheetLoadException($"Overwriting element ID {parentSelector.ElementId} with {elementId}");
                    }

                    elementId = parentSelector.ElementId;
                }
            }

            return new SelectorElement(elementType, elementClasses, elementId, pseudoClasses);
        }

        private List<StyleProperty> ParseProperties(List<StyleRule> styleRules, SelectorElement selectorElement, LuaTable propertiesTable)
        {
            var styleProperties = new List<StyleProperty>();

            foreach (KeyValuePair<object, object> pair in propertiesTable)
            {
                var key = pair.Key;

                if (key is string propertyName)
                {
                    if (pair.Value is LuaTable propertyValueTable)
                    {
                        var propertyValueType = (string)propertyValueTable["type"];

                        if (propertyValueType == "font")
                        {
                            styleProperties.Add(new StyleProperty(propertyName, new FontSpec(20)));
                        }
                        else if (propertyValueType == "literal")
                        {
                            styleProperties.Add(new StyleProperty(propertyName, propertyValueTable["value"]));
                        }
                        else
                        {
                            throw new StylesheetLoadException($"Invalid property table type for '{propertyName}': {propertyValueType}");
                        }
                    }
                    else if (pair.Value is string propertyValueString)
                    {
                        if (propertyValueString.StartsWith("#"))
                        {
                            styleProperties.Add(new StyleProperty(propertyName, Color.FromHex(propertyValueString)));
                        }
                        else
                        {
                            styleProperties.Add(new StyleProperty(propertyName, propertyValueString));
                        }
                    }
                    else
                    {
                        styleProperties.Add(new StyleProperty(propertyName, pair.Value));
                    }
                }
            }

            return styleProperties;
        }

        private void ParseStyleRules(List<StyleRule> styleRules, SelectorElement selectorElement, LuaTable propertiesTable, SelectorElement? parentSelector = null)
        {
            List<StyleProperty> styleProperties = ParseProperties(styleRules, selectorElement, propertiesTable);

            if (styleProperties.Count > 0)
            {
                Selector selector = selectorElement;

                if (parentSelector != null)
                {
                    selector = new SelectorChild(parentSelector, selectorElement);
                }

                var styleRule = new StyleRule(selector, styleProperties);

                Logger.InfoS("stylesheet", $"Parsed style rule:\n{styleRule}");

                styleRules.Add(styleRule);
            }
        }

        private void ParseNestedForms(List<StyleRule> styleRules, LuaTable propertiesTable, SelectorElement selectorElement)
        {
            foreach (KeyValuePair<object, object> pair in propertiesTable)
            {
                if (pair.Key is long)
                {
                    if (pair.Value is LuaTable propertyTable)
                    {
                        var type = (string)propertyTable["type"];

                        if (type == "Rule")
                        {
                            ParseRule(styleRules, propertyTable, selectorElement);
                        }
                        else if (type == "Element")
                        {
                            ParseElementRules(styleRules, propertyTable, selectorElement);
                        }
                        else
                        {
                            throw new StylesheetLoadException($"Invalid nested element type: {type}");
                        }
                    }
                    else
                    {
                        throw new StylesheetLoadException("Invalid nested element");
                    }
                }
            }
        }
    }

    public class StylesheetLoadException : Exception
    {
        public StylesheetLoadException(string? message) : base(message)
        {
        }
    }
}
