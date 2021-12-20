using NUnit.Framework;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.IoC;
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
    [TestFixture]
    [TestOf(typeof(SaveGameManager))]
    public class SaveGameManager_Tests : OpenNefiaUnitTest
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            IoCManager.Resolve<ISerializationManager>().Initialize();
        } 

        [SetUp]
        public void Setup()
        {
            var resourceManager = IoCManager.Resolve<IResourceManagerInternal>();
            resourceManager.Initialize(null);

            var saveGameManager = IoCManager.Resolve<ISaveGameManager>();
            saveGameManager.Initialize(null);
        }

        [Test]
        public void TestCreateSaveInvalid()
        {
            var saveMan = IoCManager.Resolve<ISaveGameManager>();
            var header = new SaveGameHeader("testSave");

            Assert.Throws<ArgumentException>(() => saveMan.CreateSave(new ResourcePath(""), header));
            Assert.Throws<ArgumentException>(() => saveMan.CreateSave(new ResourcePath("aux"), header));
            Assert.Throws<ArgumentException>(() => saveMan.CreateSave(new ResourcePath("LPT"), header));
            Assert.Throws<ArgumentException>(() => saveMan.CreateSave(new ResourcePath("\\"), header));
            Assert.Throws<ArgumentException>(() => saveMan.CreateSave(new ResourcePath(".."), header));
            Assert.Throws<ArgumentException>(() => saveMan.CreateSave(new ResourcePath("..\\a"), header));
            Assert.Throws<ArgumentException>(() => saveMan.CreateSave(new ResourcePath("a/b"), header));
            Assert.Throws<ArgumentException>(() => saveMan.CreateSave(new ResourcePath("a"), header));
        }

        [Test]
        public void TestCreateSave()
        {
            var resMan = IoCManager.Resolve<IResourceManager>();
            var saveMan = IoCManager.Resolve<ISaveGameManager>();
            var header = new SaveGameHeader("testSave");

            Assert.That(saveMan.AllSaves.Count(), Is.EqualTo(0));

            var testSavePath = new ResourcePath("/testSave");
            var save = saveMan.CreateSave(testSavePath, header);

            Assert.That(save.Header.Name, Is.EqualTo(header.Name));
            Assert.That(save.ScreenshotFile, Is.Null);
            Assert.That(save.SaveDirectory, Is.EqualTo(testSavePath));
            Assert.That(saveMan.AllSaves.Count(), Is.EqualTo(1));
            Assert.That(saveMan.ContainsSave(save), Is.True);
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
            var resMan = IoCManager.Resolve<IResourceManager>();
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
