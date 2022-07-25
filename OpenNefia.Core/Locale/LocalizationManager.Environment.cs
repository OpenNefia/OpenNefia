using OpenNefia.Core.Prototypes;
using NLua;
using OpenNefia.Core.Utility;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.Reflection;
using System.Reflection;
using System.Linq.Expressions;
using System.Linq;
using OpenNefia.Core.Log;
using OpenNefia.Core.IoC;

namespace OpenNefia.Core.Locale
{
    public partial class LocalizationManager : IDisposable
    {
        private PrototypeId<LanguagePrototype> _currentLanguage;

        private Lua _lua = default!;
        internal Dictionary<string, string> _stringStore = new();
        internal Dictionary<string, List<string>> _listStore = new();
        internal Dictionary<string, LuaFunction> _functionStore = new();

        private Dictionary<string, MethodInfo> _sharedBuiltInFunctions = new();
        private Dictionary<PrototypeId<LanguagePrototype>, Dictionary<string, MethodInfo>> _builtInFunctions = new();

        private LuaTable _FinalizedKeys => (LuaTable)_lua["_FinalizedKeys"];

        public void Clear()
        {
            _lua?.Dispose();
            _lua = CreateLuaEnv();
            _stringStore.Clear();
        }

        private Lua CreateLuaEnv()
        {
            var lua = new Lua();
            lua.State.Encoding = EncodingHelpers.UTF8;
            AddContentRootsToSearchPath(lua);
            AddHelperFunctionsToEnv(lua);
            return lua;
        }

        private void AddContentRootsToSearchPath(Lua lua)
        {
            var path = (string)lua["package.path"];
            foreach (var root in _resourceManager.GetContentRoots())
            {
                path += $";{root / "?.lua"}";
            }
            lua["package.path"] = path;
        }

        private readonly Dictionary<string, LogLevel> LogLevelMap =
            EnumHelpers.EnumerateValues<LogLevel>().ToDictionary(l => l.ToString().ToLower(), l => l);

        private void AddHelperFunctionsToEnv(Lua lua)
        {
            lua["log"] = (string level, string message) =>
            {
                var logLevel = LogLevelMap.GetValueOr(level, LogLevel.Info);
                Logger.LogS(logLevel, "loc.env", message);
            };
        }

        /// <summary>
        /// Returns a string containing the localization logic in the Lua side.
        /// </summary>
        public virtual string GetLocaleEnvScript()
        {
            return _resourceManager.ContentFileReadAllText("/Lua/Core/LocaleEnv.lua");
        }

        public void SetLanguage(PrototypeId<LanguagePrototype> language)
        {
            _currentLanguage = language;

            Clear();

            _lua["_LANGUAGE_CODE"] = (string)language;

            var chunk = GetLocaleEnvScript();
            _lua.DoString(chunk);

            LoadBuiltinFunctions();
        }

        private void ScanBuiltInFunctions()
        {
            _builtInFunctions.Clear();
            _sharedBuiltInFunctions.Clear();

            foreach (var ty in _reflectionManager.FindTypesWithAttribute<RegisterLocaleFunctionsAttribute>())
            {
                var regAttr = ty.GetCustomAttribute<RegisterLocaleFunctionsAttribute>()!;

                foreach (var func in ty.GetMethods().Where(m => m.IsPublic && m.IsStatic))
                {
                    if (func.TryGetCustomAttribute(out LocaleFunctionAttribute? funcAttr))
                    {
                        if (regAttr.Language != null)
                        {
                            _builtInFunctions.GetOrNew(regAttr.Language.Value).Add(funcAttr.Name, func);
                        }
                        else
                        {
                            _sharedBuiltInFunctions.Add(funcAttr.Name, func);
                        }
                    }
                }
            }
        }

        // https://stackoverflow.com/a/40579063
        private static Delegate CreateDelegate(MethodInfo methodInfo, object? target)
        {
            Func<Type[], Type> getType;
            var isAction = methodInfo.ReturnType.Equals((typeof(void)));
            var types = methodInfo.GetParameters().Select(p => p.ParameterType);

            if (isAction)
            {
                getType = Expression.GetActionType;
            }
            else
            {
                getType = Expression.GetFuncType;
                types = types.Concat(new[] { methodInfo.ReturnType });
            }

            if (methodInfo.IsStatic)
            {
                return Delegate.CreateDelegate(getType(types.ToArray()), methodInfo);
            }

            return Delegate.CreateDelegate(getType(types.ToArray()), target!, methodInfo.Name);
        }

        private void LoadBuiltinFunctions()
        {
            var table = (LuaTable)_lua["_"];

            foreach (var (funcName, func) in _sharedBuiltInFunctions)
            {
                table[funcName] = CreateDelegate(func, null);
            }

            if (_builtInFunctions.TryGetValue(_currentLanguage, out var funcs))
            {
                foreach (var (funcName, func) in funcs)
                {
                    table[funcName] = CreateDelegate(func, null);
                }
            }
        }

        public void LoadAll(PrototypeId<LanguagePrototype> language, ResourcePath rootInContent)
        {
            var path = rootInContent / language.ToString();
            var files = _resourceManager.ContentFindFiles(path).ToList().AsParallel()
                .Where(filePath => filePath.Extension == "lua");

            foreach (var file in files)
            {
                LoadContentFile(file);
            }
        }

        public void LoadContentFile(ResourcePath luaFile)
        {
            try
            {
                var str = _resourceManager.ContentFileReadAllText(luaFile);
                LoadStringRaw(str);
            }
            catch (Exception ex)
            {
                Logger.ErrorS("loc", ex, $"Failed to load Lua file {luaFile}: {ex}");
            }
        }

        public void LoadString(string luaScript)
        {
            try
            {
                LoadStringRaw(luaScript);
            }
            catch (Exception ex)
            {
                Logger.ErrorS("loc", ex, $"Failed to load Lua string: {ex}");
            }
        }

        private void LoadStringRaw(string str)
        {
            _lua.DoString("_BeforeLoad()");
            _lua.DoString(str);
            _lua.DoString("_AfterLoad()");
        }

        public void Resync()
        {
            _lua.DoString("_Finalize()");

            foreach (KeyValuePair<object, object> pair in _FinalizedKeys)
            {
                var key = pair.Key.ToString()!;
                var value = pair.Value;

                var ty = value.GetType();

                if (ty == typeof(LuaFunction))
                {
                    _functionStore[key] = (LuaFunction)value;
                }
                else if (ty == typeof(LuaTable))
                {
                    var list = new List<string>();
                    foreach (KeyValuePair<object, object> subpair in (LuaTable)value)
                    {
                        list.Add(subpair.Value.ToString()!);
                    }
                    _listStore[key] = list;
                }
                else
                {
                    _stringStore[key] = value!.ToString()!;
                }
            }
        }

        public void Dispose()
        {
            this._lua.Dispose();
        }
    }
}
