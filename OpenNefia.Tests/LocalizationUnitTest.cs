using NUnit.Framework;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Tests
{
    public class LocalizationUnitTest : OpenNefiaUnitTest
    {
        protected virtual PrototypeId<LanguagePrototype> TestingLanguage => LanguagePrototypeOf.English;

        protected override void OverrideIoC()
        {
            IoCManager.Register<ILocalizationManager, TestingLocalizationManager>(overwrite: true);

            // Load the required localization code in the Lua side.
        }

        [SetUp]
        public void Setup()
        {
            var locMan = IoCManager.Resolve<ILocalizationManager>();
            locMan.SwitchLanguage(TestingLanguage);
        }
    }

    public sealed class TestingLocalizationManager : LocalizationManager
    {
        public override string GetLocaleEnvScript()
        {
            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../OpenNefia.Core/Resources/Lua/Core/LocaleEnv.lua");
            return File.ReadAllText(filepath);
        }
    }
}
