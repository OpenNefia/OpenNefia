using System.Text;
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

        private static Lua SetupLua()
        {
            var lua = new Lua();
            lua.State.Encoding = Encoding.UTF8;
            return lua;
        }

        public void LoadAll(PrototypeId<LanguagePrototype> language)
        {
            var opts = new EnumerationOptions() { RecurseSubdirectories = true };
            _Lua["_LANGUAGE_CODE"] = (string)language;
            _Lua.DoFile("Assets/Core/Lua/LocaleEnv.lua");

            var path = new ResourcePath("/Elona/Locale") / language.ToString();

            var files = _resourceManager.ContentFindFiles(path).ToList().AsParallel()
                .Where(filePath => filePath.Extension == "lua");

            foreach (var file in files)
            {
                var str = _resourceManager.ContentFileReadAllText(file);
                _Lua.DoString(str);
            }

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
