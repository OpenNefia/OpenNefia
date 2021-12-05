using NUnit.Framework;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Tests.Core.ContentPack
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(VirtualWritableDirProvider_Tests))]
    public class VirtualWritableDirProvider_Tests : OpenNefiaUnitTest
    {
        [Test]
        public void TestWriteThenRead()
        {
            var writer = new VirtualWritableDirProvider();

            var path = new ResourcePath("/file.txt");

            using (var stream = writer.OpenWrite(path))
            {
                using (var streamWriter = new StreamWriter(stream))
                {
                    streamWriter.Write("Scut!");
                }

                Assert.That(stream.Length, Is.EqualTo(5));
                Assert.That(stream.Position, Is.EqualTo(5));
            }

            using (var stream = writer.OpenRead(path))
            {
                Assert.That(stream.Length, Is.EqualTo(5));
                Assert.That(stream.Position, Is.EqualTo(0));

                using (StreamReader streamReader = new StreamReader(stream))
                {
                    var text = streamReader.ReadToEnd();
                    Assert.That(text, Is.EqualTo("Scut!"));
                }
            }
        }
    }
}
