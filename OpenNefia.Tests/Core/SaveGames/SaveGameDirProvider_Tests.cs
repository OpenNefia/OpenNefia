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

        [Test]
        public void TestCreateAndCommitFileInDir()
        {
            using var temp = new TempWritableDirProvider();
            using var committed = new TempWritableDirProvider();
            var save = new SaveGameDirProvider(temp, committed);

            var file = new ResourcePath("/Hoge/Piyo.txt");

            save.CreateDirectory(file.Directory);
            save.WriteAllText(file, "Piyo");

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

        [Test]
        public void TestDeleteCommittedFile()
        {
            using var temp = new TempWritableDirProvider();
            using var committed = new TempWritableDirProvider();
            var save = new SaveGameDirProvider(temp, committed);

            var file = new ResourcePath("/Hoge/Piyo.txt");
            var dir = file.Directory;

            // Existing save contains a file/directory.
            committed.CreateDirectory(dir);
            committed.WriteAllText(file, "Piyo");

            save.Delete(file);

            Assert.Multiple(() =>
            {
                Assert.That(save.Exists(file), Is.False, "Save");
                Assert.That(temp.Exists(file), Is.False, "Temp");
                Assert.That(committed.Exists(file), Is.True, "Committed");

                Assert.That(save.IsDirectory(dir), Is.True, "Save (directory)");
                Assert.That(temp.IsDirectory(dir), Is.False, "Temp (directory)");
                Assert.That(committed.IsDirectory(dir), Is.True, "Committed (directory)");
            });

            save.Commit();

            Assert.Multiple(() =>
            {
                Assert.That(save.Exists(file), Is.False, "Save");
                Assert.That(temp.Exists(file), Is.False, "Temp");
                Assert.That(committed.Exists(file), Is.False, "Committed");

                Assert.That(save.IsDirectory(dir), Is.True, "Save (directory)");
                Assert.That(temp.IsDirectory(dir), Is.False, "Temp (directory)");
                Assert.That(committed.IsDirectory(dir), Is.True, "Committed (directory)");
            });
        }

        [Test]
        public void TestDeleteCommittedDirectory()
        {
            using var temp = new TempWritableDirProvider();
            using var committed = new TempWritableDirProvider();
            var save = new SaveGameDirProvider(temp, committed);

            var file = new ResourcePath("/Hoge/Piyo.txt");
            var dir = file.Directory;

            // Existing save contains a file/directory.
            committed.CreateDirectory(dir);
            committed.WriteAllText(file, "Piyo");

            save.Delete(dir);

            Assert.Multiple(() =>
            {
                Assert.That(save.Exists(file), Is.False, "Save");
                Assert.That(temp.Exists(file), Is.False, "Temp");
                Assert.That(committed.Exists(file), Is.True, "Committed");

                Assert.That(save.IsDirectory(dir), Is.False, "Save (directory)");
                Assert.That(temp.IsDirectory(dir), Is.False, "Temp (directory)");
                Assert.That(committed.IsDirectory(dir), Is.True, "Committed (directory)");
            });

            save.Commit();

            Assert.Multiple(() =>
            {
                Assert.That(save.Exists(file), Is.False, "Save");
                Assert.That(temp.Exists(file), Is.False, "Temp");
                Assert.That(committed.Exists(file), Is.False, "Committed");

                Assert.That(save.IsDirectory(dir), Is.False, "Save (directory)");
                Assert.That(temp.IsDirectory(dir), Is.False, "Temp (directory)");
                Assert.That(committed.IsDirectory(dir), Is.False, "Committed (directory)");
            });
        }

        [Test]
        public void TestRenameCommittedFile()
        {
            using var temp = new TempWritableDirProvider();
            using var committed = new TempWritableDirProvider();
            var save = new SaveGameDirProvider(temp, committed);

            var oldFile = new ResourcePath("/Hoge/Piyo.txt");
            var oldDir = oldFile.Directory;

            committed.CreateDirectory(oldDir);
            committed.WriteAllText(oldFile, "Piyo");

            var newFile = new ResourcePath("/Fuga/Baz.txt");
            var newDir = newFile.Directory;

            save.CreateDirectory(newDir);
            save.Rename(oldFile, newFile);

            Assert.Multiple(() =>
            {
                Assert.That(save.Exists(oldFile), Is.False, "Save");
                Assert.That(temp.Exists(oldFile), Is.False, "Temp");
                Assert.That(committed.Exists(oldFile), Is.True, "Committed");

                Assert.That(save.Exists(newFile), Is.True, "Save (new)");
                Assert.That(temp.Exists(newFile), Is.True, "Temp (new)");
                Assert.That(committed.Exists(newFile), Is.False, "Committed (new)");
            });

            save.Commit();

            Assert.Multiple(() =>
            {
                Assert.That(save.Exists(oldFile), Is.False, "Save");
                Assert.That(temp.Exists(oldFile), Is.False, "Temp");
                Assert.That(committed.Exists(oldFile), Is.False, "Committed");

                Assert.That(save.Exists(newFile), Is.True, "Save (new)");
                Assert.That(temp.Exists(newFile), Is.False, "Temp (new)");
                Assert.That(committed.Exists(newFile), Is.True, "Committed (new)");
            });
        }
    }
}