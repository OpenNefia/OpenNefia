using System.Text;
using OpenNefia.Core.Prototypes;
using NLua;

namespace OpenNefia.Core.Locale
{
    internal class LocalizationEnv : IDisposable
    {
        internal Lua Lua;
        internal Dictionary<string, string> StringStore = new Dictionary<string, string>();
        internal Dictionary<string, LuaFunction> FunctionStore = new Dictionary<string, LuaFunction>();

        private LuaTable _FinalizedKeys => (LuaTable)Lua["_FinalizedKeys"];

        public LocalizationEnv()
        {
            Lua = SetupLua();
        }

        public void Clear()
        {
            Lua.Dispose();
            Lua = SetupLua();
            StringStore.Clear();
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
            Lua["_LANGUAGE_CODE"] = (string)language;
            Lua.DoFile("Assets/Core/Lua/LocaleEnv.lua");
            foreach (var file in Directory.EnumerateFiles($"Assets/Elona/Locale/{language}", "*.lua", opts))
            {
                Lua.DoFile(file);
            }
            Lua.DoString("_Finalize()");
            foreach (KeyValuePair<object, object> pair in _FinalizedKeys)
            {
                var key = pair.Key;
                var value = pair.Value;

                if (value.GetType() == typeof(LuaFunction))
                {
                    FunctionStore[key.ToString()!] = (LuaFunction)value;
                }
                else
                {
                    StringStore[key.ToString()!] = value!.ToString()!;
                }
            }
        }

        public void Dispose()
        {
            this.Lua.Dispose();
        }
    }
}
