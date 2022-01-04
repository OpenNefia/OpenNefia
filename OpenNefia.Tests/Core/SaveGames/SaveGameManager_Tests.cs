using NUnit.Framework;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Profiles;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Tests.Core.SaveGames
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(SaveGameManager))]
    public class SaveGameManager_Tests : OpenNefiaUnitTest
    {
        private TempWritableDirProvider? _tempDir;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            IoCManager.Resolve<ISerializationManager>().Initialize();
        }

        [SetUp]
        public void Setup()
        {
            _tempDir = new TempWritableDirProvider();

            var resourceManager = IoCManager.Resolve<IResourceManagerInternal>();
            resourceManager.Initialize(_tempDir);

            var profileManager = IoCManager.Resolve<IProfileManager>();
            profileManager.Initialize();

            var saveGameManager = IoCManager.Resolve<ISaveGameManagerInternal>();
            saveGameManager.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _tempDir?.Dispose();
            _tempDir = null;
        }

        [Test]
        public void TestCreateSaveInvalid()
        {
            var saveMan = IoCManager.Resolve<ISaveGameManager>();
            var header = new SaveGameHeader("testSave");

            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentException>(() => saveMan.CreateSave(new ResourcePath(""), header));
                Assert.Throws<ArgumentException>(() => saveMan.CreateSave(new ResourcePath("aux"), header));
                Assert.Throws<ArgumentException>(() => saveMan.CreateSave(new ResourcePath("LPT"), header));
                Assert.Throws<ArgumentException>(() => saveMan.CreateSave(new ResourcePath("\\"), header));
                Assert.Throws<ArgumentException>(() => saveMan.CreateSave(new ResourcePath(".."), header));
                Assert.Throws<ArgumentException>(() => saveMan.CreateSave(new ResourcePath("..\\a"), header));
                Assert.Throws<ArgumentException>(() => saveMan.CreateSave(new ResourcePath("a/b"), header));
                Assert.Throws<ArgumentException>(() => saveMan.CreateSave(new ResourcePath("a"), header));
            });
        }

        [Test]
        public void TestRegisterSave()
        {
            var profMan = IoCManager.Resolve<IProfileManager>();
            var saveMan = IoCManager.Resolve<ISaveGameManagerInternal>();
            var serMan = IoCManager.Resolve<ISerializationManager>();

            Assert.That(saveMan.AllSaves.Count(), Is.EqualTo(0));

            var versions = new Dictionary<string, Version>()
            {
                { "OpenNefia.Content", new Version(0, 30, 0) }
            };
            var header = new SaveGameHeader("ruin", new Version(0, 42, 0), "deadbeef", versions);

            var headerPath = new ResourcePath("/Saves") / "testSave" / "header.yml";

            profMan.CurrentProfile.CreateDirectory(headerPath.Directory);
            profMan.CurrentProfile.WriteSerializedData(headerPath, header, serMan, alwaysWrite: true);

            saveMan.RescanSaves();

            Assert.That(saveMan.AllSaves.Count(), Is.EqualTo(1));

            var save = saveMan.AllSaves.First();

            Assert.Multiple(() =>
            {
                Assert.That(save.Header.Name, Is.EqualTo("ruin"));
                Assert.That(save.Header.EngineVersion, Is.EqualTo(new Version(0, 42, 0)));
                Assert.That(save.Header.EngineCommitHash, Is.EqualTo("deadbeef"));
                Assert.That(save.Header.AssemblyVersions, Is.EquivalentTo(versions));
                Assert.That(save.SaveDirectory, Is.EqualTo(new ResourcePath("/testSave")));
                Assert.That(save.Files.Exists(new ResourcePath("/header.yml")));
                Assert.That(saveMan.ContainsSave(save), Is.True);
            });
        }

        [Test]
        public void TestCreateSave()
        {
            var resMan = IoCManager.Resolve<IResourceManager>();
            var saveMan = IoCManager.Resolve<ISaveGameManager>();
            var header = new SaveGameHeader("testSave");

            Assert.That(saveMan.AllSaves.Count(), Is.EqualTo(0));

            var testSaveDir = new ResourcePath("/testSave");
            var save = saveMan.CreateSave(testSaveDir, header);

            Assert.Multiple(() =>
            {
                Assert.That(save.Header.Name, Is.EqualTo(header.Name));
                Assert.That(save.SaveDirectory, Is.EqualTo(testSaveDir));
                Assert.That(save.Files.Exists(new ResourcePath("/header.yml")));
                Assert.That(saveMan.AllSaves.Count(), Is.EqualTo(1));
                Assert.That(saveMan.ContainsSave(save), Is.True);
            });
        }

        [Test]
        public void TestCreateSave_AlreadyExists()
        {
            var resMan = IoCManager.Resolve<IResourceManager>();
            var saveMan = IoCManager.Resolve<ISaveGameManager>();
            
            var header1 = new SaveGameHeader("testSave1");
            var header2 = new SaveGameHeader("testSave2");
            var testSavePath = new ResourcePath("/testSave");

            saveMan.CreateSave(testSavePath, header1);
            Assert.Throws<InvalidOperationException>(() => saveMan.CreateSave(testSavePath, header2));
        }

        [Test]
        public void TestDeleteSave()
        {
            var saveMan = IoCManager.Resolve<ISaveGameManager>();
            var header = new SaveGameHeader("testSave");

            var testSavePath = new ResourcePath("/testSave");
            var save = saveMan.CreateSave(testSavePath, header);

            Assert.That(saveMan.AllSaves.Count(), Is.EqualTo(1));

            saveMan.DeleteSave(save);

            Assert.That(saveMan.AllSaves.Count(), Is.EqualTo(0));
            Assert.That(saveMan.ContainsSave(save), Is.False);
        }
    }
}