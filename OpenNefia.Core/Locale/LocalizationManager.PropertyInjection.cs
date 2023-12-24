using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Locale
{
    public partial class LocalizationManager
    {
        public void DoLocalize(object o, LocaleKey key)
        {
            foreach (var field in o.GetType().GetAllPropertiesAndFields())
            {
                if (field.TryGetAttribute<LocalizeAttribute>(out var fieldAttr))
                    DoLocalizeField(o, key, field, fieldAttr);
            }
        }

        private void DoLocalizeField(object? o, LocaleKey baseKey, AbstractFieldInfo field, LocalizeAttribute fieldAttr)
        {
            string keyFrag = fieldAttr?.RootLocaleKey ?? field.Name;
            var nextKey = baseKey.With(keyFrag);

            // Override the field's [Localize] attribute if [Localize] is declared on the field's type.
            if (field.GetType().TryGetCustomAttribute<LocalizeAttribute>(out var typeAttr))
            {
                nextKey = typeAttr.RootLocaleKey ?? throw new ArgumentNullException($"[Localize] attribute declared on type {field.GetType()} had no locale key declared.");
            }

            if (typeof(ILocalizable).IsAssignableFrom(field.FieldType))
            {
                var localizable = (ILocalizable)field.GetValue(o)!;
                if (localizable != null)
                    localizable.Localize(baseKey.With(keyFrag));
                return;
            }

            if (field.FieldType == typeof(string))
            {
                field.SetValue(o, GetString(nextKey));
            }
            else if (field.FieldType.IsGenericType)
            {
                var genericType = field.FieldType.GetGenericTypeDefinition();
                if (genericType == typeof(Dictionary<,>))
                {
                    LocalizeDictionary(field, nextKey);
                }
                else
                {
                    throw new Exception($"Cannot localize field of generic type {field.FieldType}");
                }
            }
            else
            {
                throw new Exception($"Cannot localize field of type {field.FieldType}");
            }
        }

        private void LocalizeDictionary(AbstractFieldInfo field, LocaleKey localeKey)
        {
            var luaTable = _lua.GetTable("_Collected." + localeKey);

            if (luaTable == null)
            {
                throw new Exception($"Lua table at key {localeKey} not found.");
            }

            var ty = field.FieldType;
            var keyTy = ty.GetGenericArguments()[0];
            var valueTy = ty.GetGenericArguments()[1];

            var dict = Activator.CreateInstance(ty)!;

            var methodAdd = dict.GetType().GetMethod("Add")!;
            foreach (KeyValuePair<object, object> pair in luaTable)
            {
                object key = Convert.ChangeType(pair.Key, keyTy);
                object value;

                if (valueTy == typeof(string))
                {
                    value = pair.Value.ToString()!;
                }
                else
                {
                    throw new Exception($"Cannot convert to a localized object of type {valueTy}");
                }

                methodAdd.Invoke(dict, new object?[] { key, value });
            }

            field.SetValue(null, dict);
        }
    }

    public static class LocaleHelpers
    {
        public static LocaleKey GetBaseLocaleKey(this Type type)
        {
            var attr = type.GetCustomAttribute<LocalizeAttribute>();
            if (attr == null)
            {
                return LocaleKey.Empty;
            }

            return attr.RootLocaleKey != null ? new(attr.RootLocaleKey) : LocaleKey.Empty;
        }

        public static IEnumerable<FieldInfo> GetLocalizableFields(this Type type)
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(field => field.GetCustomAttribute<LocalizeAttribute>() != null);
        }
    }
}
