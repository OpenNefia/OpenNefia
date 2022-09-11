using NLua;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Locale
{
    public static class LuaHelpers
    {
        public static bool TryGetValue(this LuaTable table, string key, [NotNullWhen(true)] out object? obj)
        {
            obj = table[key];
            return obj != null;
        }

        public static bool TryGetValue(this LuaTable table, int key, [NotNullWhen(true)] out object? obj)
        {
            obj = table[key];
            return obj != null;
        }

        public static bool TryGetString(this LuaTable table, string key, [NotNullWhen(true)] out string? str)
        {
            if (!TryGetValue(table, key, out var obj))
            {
                str = null;
                return false;
            }

            str = obj as string;
            return str != null;
        }

        public static bool TryGetTable(this LuaTable table, string key, [NotNullWhen(true)] out LuaTable? resultTable)
        {
            if (!TryGetValue(table, key, out var obj))
            {
                resultTable = null;
                return false;
            }

            resultTable = obj as LuaTable;
            return resultTable != null;
        }

        public static string GetStringOrEmpty(this LuaTable table, string key)
        {
            if (TryGetValue(table, key, out var obj))
                return $"{obj}";

            return "<empty>";
        }

        public static string? GetStringOrNull(this LuaTable table, string key)
        {
            if (TryGetValue(table, key, out var obj))
                return $"{obj}";

            return null;
        }

        public static bool GetBoolean(this LuaTable table, string key, bool @default)
        {
            if (TryGetValue(table, key, out var obj) && obj is bool b)
                return b;

            return @default;
        }
    }
}
