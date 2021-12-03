using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.UI;
using OpenNefia.Core.Utility;
using System.Reflection;
using NLua;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.ContentPack;

namespace OpenNefia.Core.Locale
{
    public interface ILocalizationManager
    {
        PrototypeId<LanguagePrototype> Language { get; }

        void Initialize();

        bool IsFullwidth();
        void SwitchLanguage(PrototypeId<LanguagePrototype> language);

        void DoLocalize(object o, LocaleKey key);

        LocaleFunc<T> GetFunction<T>(LocaleKey key);
        LocaleFunc<T1, T2> GetFunction<T1, T2>(LocaleKey key);
        string GetString(LocaleKey key);
    }

    public class LocalizationManager : ILocalizationManager
    {
        [Dependency] private readonly IUiLayerManager _uiLayers = default!;
        [Dependency] private readonly IResourceManager _resourceManager = default!;

        private LocalizationEnv _env = null!;

        public void Initialize()
        {
            _env = new LocalizationEnv(_resourceManager);

            SwitchLanguage(LanguagePrototypeOf.English);
        }

        public PrototypeId<LanguagePrototype> Language { get; private set; } = LanguagePrototypeOf.English;
        
        public void SwitchLanguage(PrototypeId<LanguagePrototype> language)
        {
            Language = language;
            _env.Clear();
            _env.LoadAll(language);

            foreach (var layer in _uiLayers.ActiveLayers)
            {
                layer.Localize(layer.GetType()!.FullName!);
            }
        }

        public string GetString(LocaleKey key)
        {
            var result = _env._StringStore.GetValueOrDefault(key);
            if (result != null)
            {
                return result;
            }
            return $"<Missing key: {key}>";
        }

        public LocaleFunc<T> GetFunction<T>(LocaleKey key)
        {
            var luaFunc = _env._FunctionStore.GetValueOrDefault(key);
            if (luaFunc == null)
            {
                var luaString = _env._StringStore.GetValueOrDefault(key);
                if (luaString == null)
                {
                    return (_) => $"<Missing key: {key}>";
                }
                else
                {
                    return (_) => luaString;
                }
            }

            return (arg) =>
            {
                var result = luaFunc.Call(arg)[0];
                return $"{result}";
            };
        }

        public LocaleFunc<T1, T2> GetFunction<T1, T2>(LocaleKey key)
        {
            var luaFunc = _env._FunctionStore.GetValueOrDefault(key);
            if (luaFunc == null)
            {
                var luaString = _env._StringStore.GetValueOrDefault(key);
                if (luaString == null)
                {
                    return (_, _) => $"<Missing key: {key}>";
                }
                else
                {
                    return (_, _) => luaString;
                }
            }

            return (arg1, arg2) =>
            {
                var result = luaFunc.Call(arg1, arg2)[0];
                return $"{result}";
            };
        }

        public bool IsFullwidth()
        {
            return Language == LanguagePrototypeOf.Japanese;
        }

        public void DoLocalize(object o, LocaleKey key)
        {
            foreach (var field in o.GetType().GetLocalizableFields())
            {
                DoLocalizeField(o, key, field);
            }
        }

        private void DoLocalizeField(object? o, LocaleKey baseKey, FieldInfo field)
        {
            var attr = field.GetLocalizeAttribute();

            string keyFrag = attr?.Key ?? field.Name;

            if (typeof(ILocalizable).IsAssignableFrom(field.FieldType))
            {
                var localizable = (ILocalizable)field.GetValue(o)!;
                localizable.Localize(baseKey.With(keyFrag));
                return;
            }

            var nextKey = baseKey.With(keyFrag);

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
                else if (genericType == typeof(LocaleFunc<>) || genericType == typeof(LocaleFunc<,>))
                {
                    var method = GetFunctionMethod(field);
                    if (method == null)
                    {
                        throw new Exception($"Cannot localize to function of type {field.FieldType}");
                    }
                    field.SetValue(o, method.Invoke(null, new object[] { nextKey }));
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

        private MethodInfo? GetFunctionMethod(FieldInfo field)
        {
            var genericArgs = field.FieldType.GetGenericArguments();
            return typeof(LocalizationManager).GetMethods().FirstOrDefault(
                x => x.Name.Equals(nameof(GetFunction)) &&
                x.IsGenericMethod && x.GetGenericArguments().Length == genericArgs.Length)
                ?.MakeGenericMethod(genericArgs);
        }

        private void LocalizeDictionary(FieldInfo field, LocaleKey localeKey)
        {
            var luaTable = _env._Lua.GetTable(localeKey);
            
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
                else if (valueTy.IsGenericType && valueTy.GetGenericTypeDefinition() == typeof(LocaleFunc<>))
                {
                    var method = typeof(LocalizationManager).GetMethod(nameof(MakeLocaleFunc), BindingFlags.Static | BindingFlags.NonPublic)!.MakeGenericMethod(valueTy.GetGenericArguments());
                    value = method.Invoke(null, new object[] { pair.Value, valueTy })!;
                }
                else
                {
                    throw new Exception($"Cannot convert to a localized object of type {valueTy}");
                }

                methodAdd.Invoke(dict, new object?[] { key, value });
            }

            field.SetValue(null, dict);
        }

        private LocaleFunc<T> MakeLocaleFunc<T>(object luaValue, Type localeFuncType)
        {
            var luaFunc = luaValue as LuaFunction;
            if (luaFunc == null)
            {
                var str = luaValue.ToString()!;
                return (_) => str;
            }
            return (arg) =>
            {
                var result = luaFunc.Call(arg)[0];
                return $"{result}";
            };
        }
    }
    
    public static class LocaleHelpers
    {
        public static LocaleKey GetBaseLocaleKey(this Type type) => type.FullName!;

        public static ILocalizeAttribute? GetLocalizeAttribute(this MemberInfo member)
        {
            return member.GetCustomAttributes()
                .Select(attr => attr as ILocalizeAttribute)
                .WhereNotNull()
                .FirstOrDefault();
        }

        public static IEnumerable<FieldInfo> GetLocalizableFields(this Type type)
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(field => field.GetLocalizeAttribute() != null);
        }
    }
}
