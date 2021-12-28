using OpenNefia.Core.Prototypes;
using NLua;
using OpenNefia.Core.Utility;
using OpenNefia.Core.ContentPack;

namespace OpenNefia.Core.Locale
{
    internal class LocalizationEnv : IDisposable
    {
        private readonly IResourceManager _resourceManager;

        internal Lua _Lua;
        internal Dictionary<string, string> _StringStore = new Dictionary<string, string>();
        internal Dictionary<string, LuaFunction> _FunctionStore = new Dictionary<string, LuaFunction>();

        private LuaTable _FinalizedKeys => (LuaTable)_Lua["_FinalizedKeys"];

        public LocalizationEnv(IResourceManager resourceManager)
        {
            _resourceManager = resourceManager;

            _Lua = SetupLua();
        }

        public void Clear()
        {
            _Lua.Dispose();
            _Lua = SetupLua();
            _StringStore.Clear();
        }

        private Lua SetupLua()
        {
            var lua = new Lua();
            lua.State.Encoding = EncodingHelpers.UTF8;
            AddContentRootsToSearchPath(lua);
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

        public void SetLanguage(PrototypeId<LanguagePrototype> language)
        {
            Clear();

            _Lua["_LANGUAGE_CODE"] = (string)language;

            var chunk = _resourceManager.ContentFileReadAllText("/Lua/Core/LocaleEnv.lua");
            _Lua.DoString(chunk);
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
            var str = _resourceManager.ContentFileReadAllText(luaFile);
            _Lua.DoString(str);
        }

        public void LoadString(string luaScript)
        {
            _Lua.DoString(luaScript);
        }

        public void Resync()
        {
            _Lua.DoString("_Finalize()");
            foreach (KeyValuePair<object, object> pair in _FinalizedKeys)
            {
                var key = pair.Key;
                var value = pair.Value;

                if (value.GetType() == typeof(LuaFunction))
                {
                    _FunctionStore[key.ToString()!] = (LuaFunction)value;
                }
                else
                {
                    _StringStore[key.ToString()!] = value!.ToString()!;
                }
            }
        }

        public void Dispose()
        {
            this._Lua.Dispose();
        }
    }
}
