using System.IO;
using NUnit.Framework;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Utility;

namespace OpenNefia.Tests.Core.ContentPack
{
    [TestFixture]
    public class ResourceManager_Tests : OpenNefiaUnitTest
    {
        private static Stream ZipStream => typeof(ResourceManager_Tests).Assembly
            .GetManifestResourceStream("OpenNefia.Tests.Core.ContentPack.ZipTest.zip")!;

        private static readonly byte[] Data =
        {
            0x56, 0x75, 0x6c, 0x6b, 0x61, 0x6e, 0x20, 0x57, 0x68, 0x65, 0x6e
        };

        private static readonly string[] InvalidPaths =
        {
            // Reserved filenames like COM ports.
            "/foo/COM1",
            "COM2",
            "/COM3/foo",
            "COM4",
            "COM5",
            "COM6",
            "COM7",
            "COM8",
            "COM9",
            "LPT1",
            "LPT2",
            "LPT3",
            "LPT4",
            "LPT5",
            "LPT6",
            "LPT7",
            "LPT8",
            "LPT9",
            "NUL",
            "AUX",
            "CON",
            "PRN",
            // ? is banned.
            "/foo/bar?",
            // * is banned.
            "/foo*/baz",
            // | is banned.
            "/yay|bugs",
            // : is banned.
            "/yay: bugs",
            // " is banned.
            "/yay... \"bugs\"",
            // \0 is banned.
            "/\0",
            // \x01-\x1f are banned.
            "/\n",
        };

        [OneTimeSetUp]
        public void Setup()
        {
            var componentFactory = IoCManager.Resolve<IComponentFactory>();
            componentFactory.FinishRegistration();

            var stream = new MemoryStream(Data);
            var resourceManager = IoCManager.Resolve<IResourceManagerInternal>();
            resourceManager.MountStreamAt(stream, new ResourcePath("/a/b/c.dat"));
        }

        [Test]
        public void TestMountedStreamRead()
        {
            var resourceManager = IoCManager.Resolve<IResourceManagerInternal>();
            using (var stream = resourceManager.ContentFileRead("/a/b/c.dat"))
            {
                Assert.That(stream.CopyToArray(), Is.EqualTo(Data));
            }
        }

        [Test]
        public void TestInvalidPaths([ValueSource(nameof(InvalidPaths))] string path)
        {
            Assert.That(ResourceManager.IsPathValid(new ResourcePath(path)), Is.False);
        }

        [Test]
        public void TestInvalidPathsLowerCase([ValueSource(nameof(InvalidPaths))] string path)
        {
            Assert.That(ResourceManager.IsPathValid(new ResourcePath(path.ToLowerInvariant())), Is.False);
        }

        [Test]
        public void TestZipRead()
        {
            var resourceManager = IoCManager.Resolve<IResourceManagerInternal>();
            resourceManager.MountContentPack(ZipStream);

            var stream = resourceManager.ContentFileRead("/foo.txt");
            Assert.That(ReadString(stream), Is.EqualTo("Honk!! \n"));
        }

        [Test]
        public void TestZipFind()
        {
            var resourceManager = IoCManager.Resolve<IResourceManagerInternal>();
            resourceManager.MountContentPack(ZipStream);

            var found = resourceManager.ContentFindFiles("/bar/");

            Assert.That(found, Is.EquivalentTo(new[]
            {
                new ResourcePath("/bar/a.txt"),
                new ResourcePath("/bar/b.txt"),
            }));
        }

        private static string ReadString(Stream stream)
        {
            using var streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }
    }
}
