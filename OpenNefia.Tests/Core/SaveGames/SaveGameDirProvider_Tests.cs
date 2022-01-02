using NUnit.Framework;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Tests.Core.SaveGames
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(SaveGameDirProvider))]
    public class SaveGameDirProvider_Tests : OpenNefiaUnitTest
    {
        [Test]
        public void TestCreateAndCommitDirectory()
        {
            using var temp = new TempWritableDirProvider();
            using var committed = new TempWritableDirProvider();
            var save = new SaveGameDirProvider(temp, committed);

            var dir = new ResourcePath("/Dood");

            save.CreateDirectory(dir);

            Assert.Multiple(() =>
            {
                Assert.That(save.IsDirectory(dir), Is.True, "Save");
                Assert.That(temp.IsDirectory(dir), Is.True, "Temp");
                Assert.That(committed.IsDirectory(dir), Is.False, "Committed");
            });

            save.Commit();

            Assert.Multiple(() =>
            {
                Assert.That(save.IsDirectory(dir), Is.True, "Save");
                Assert.That(temp.IsDirectory(dir), Is.False, "Temp");
                Assert.That(committed.IsDirectory(dir), Is.True, "Committed");
            });
        }
        [Test]
        public void TestCreateAndCommitFile()
        {
            using var temp = new TempWritableDirProvider();
            using var committed = new TempWritableDirProvider();
            var save = new SaveGameDirProvider(temp, committed);

            var file = new ResourcePath("/Dood.txt");

            save.WriteAllText(file, "Dood");

            Assert.Multiple(() =>
            {
                Assert.That(save.Exists(file), Is.True, "Save");
                Assert.That(temp.Exists(file), Is.True, "Temp");
                Assert.That(committed.Exists(file), Is.False, "Committed");
            });

            save.Commit();

            Assert.Multiple(() =>
            {
                Assert.That(save.Exists(file), Is.True, "Save");
                Assert.That(temp.Exists(file), Is.False, "Temp");
                Assert.That(committed.Exists(file), Is.True, "Committed");
            });
        }
    }
}