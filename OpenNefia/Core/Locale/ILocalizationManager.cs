using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Locale
{
    public interface ILocalizationManager
    {
        void DoLocalize(object o, LocaleKey key);
        LocaleFunc<T> GetFunction<T>(LocaleKey key);
        LocaleFunc<T1, T2> GetFunction<T1, T2>(LocaleKey key);
        string GetString(LocaleKey key);
        bool IsFullwidth();
        void SwitchLanguage(PrototypeId<LanguagePrototype> language);
    }
}
