using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Sequence;
using YamlDotNet.RepresentationModel;

namespace OpenNefia.Tests.Core.Prototypes.Theme
{
    [TestFixture]
    [TestOf(typeof(ThemeManager))]
    public class ThemeManager_Tests : OpenNefiaUnitTest
    {
        [Test]
        public void TestThemePrototypeOverrides()
        {
            IoCManager.Resolve<ISerializationManager>().Initialize();

            var themeManager = IoCManager.Resolve<IThemeManager>();
            themeManager.Initialize();

            var theme = @"
- id: TestTheme
  overrides:
  - type: ThemeTest
    id: Dummy
    first: 42
    second: [4, 5, 6]
    third:
      b: 999
";

            themeManager.LoadString(theme);

            var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
            prototypeManager.Initialize();

            var prototype = @"
- type: ThemeTest
  id: Dummy
  first: 5
  second: [1, 2, 3]
  third: 
    a: 123
    b: 456
";

            var prototypes = prototypeManager.LoadString(prototype);

            var result = (ThemeTestPrototype)prototypes[0];

            Assert.That(result.First, Is.EqualTo(42));
            Assert.That(result.Second, Is.EquivalentTo(new List<int> { 4, 5, 6 }));
            Assert.That(result.Third, Is.EquivalentTo(new Dictionary<string, int>() { { "a", 123 }, { "b", 999 } }));
        }
    }

    [Prototype("ThemeTest")]
    public class ThemeTestPrototype : IPrototype
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        [DataField]
        public int First { get; set; }

        [DataField]
        public List<int> Second { get; set; } = new();

        [DataField]
        public Dictionary<string, int> Third { get; set; } = new();
    }
}
