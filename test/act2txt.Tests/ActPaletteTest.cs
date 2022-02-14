using NUnit.    Framework;
using System.Drawing;
using System.Security.Cryptography;

namespace act2txt.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class ActPaletteTest
    {
        private static readonly TestCaseData[] shortTestData = new TestCaseData[]
        {
            new TestCaseData(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 })
            {
                ExpectedResult = new Color[] { 
                    Color.FromArgb(1, 2, 3), 
                    Color.FromArgb(4, 5, 6),
                    Color.FromArgb(7, 8, 9),
                    Color.FromArgb(10, 11, 12),
                    Color.FromArgb(13, 14, 15),
                },
            },
            new TestCaseData(Array.Empty<byte>())
            {
                ExpectedResult = Array.Empty<Color>(),
            },
            new TestCaseData(new byte[] { 1, 2, 3 })
            {
                ExpectedResult = new Color[] {
                    Color.FromArgb(1, 2, 3),
                },
            },

        };

        [Test]
        [TestCaseSource(nameof(shortTestData))]
        public IReadOnlyList<Color> Ctor_ShorterFile_ReturnsExpectedColors(byte[] buffer)
        {
            var pal = new ActPalette(buffer);
            Assert.That(pal.ExtraData, Is.Null, "ExtraData can only exist on full length ACT data");
            return pal.Colors;
        }

        [Test]
        public void Ctor_FullSize_HasExtraData()
        {
            var stream = getFullSizeStream();
            var pal = new ActPalette(stream.ToArray());
            Assert.That(pal.Colors.Count, Is.EqualTo(ActPalette.MaxColors));
            if (pal.ExtraData is not null)
            {
                Assert.That(pal.ExtraData.Value.NumColors, Is.EqualTo(12345));
                Assert.That(pal.ExtraData.Value.TransparentIndex, Is.EqualTo(6789));
            }
            else
            {
                Assert.Fail("ExtraData is null");
            }
        }

        [Test]
        public void Ctor_LargeBuffer_Throws()
        {
            var buffer = new byte[ActPalette.MaxLengthInBytes + 1];
            _ = Assert.Throws<ArgumentException>(() => new ActPalette(buffer));
        }

        [Test]
        public void Ctor_Fuzz()
        {
            for (int i = 0; i < 1000; i++)
            {
                int bufLen = Random.Shared.Next(ActPalette.MaxLengthInBytes);
                var buffer = RandomNumberGenerator.GetBytes(bufLen);
                Assert.DoesNotThrow(() => new ActPalette(buffer));
            }
        }

        private static MemoryStream getFullSizeStream()
        {
            // build input buffer
            var stream = new MemoryStream();
            for (int i = 0; i < ActPalette.MaxColors; i++)
            {
                stream.Write(new byte[] { 1, 2, 3 });
            }
            var extraData = new ActExtraData()
            {
                NumColors = 12345,
                TransparentIndex = 6789,
            };
            unsafe
            {
                var span = new Span<byte>(&extraData, sizeof(ActExtraData));
                stream.Write(span);
            }
            return stream;
        }
    }
}