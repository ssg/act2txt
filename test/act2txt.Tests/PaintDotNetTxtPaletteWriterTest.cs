using NUnit.Framework;
using System.Drawing;

namespace act2txt.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class PaintDotNetTxtPaletteWriterTest
    {
        [Test]
        public void WriteColor_WritesColor()
        {
            var writer = new StringWriter();
            var pdnWriter = new PaintDotNetTxtPaletteWriter(writer);
            pdnWriter.WriteColor(Color.FromArgb(0x10, 0xAB, 0xFF));
            var result = writer.ToString();
            Assert.That(result, Is.EqualTo("FF10ABFF\r\n"));
        }

        [Test]
        public void WritePalette_WritesAllColors()
        {
            var pal = new Color[]
            {
                Color.FromArgb(0x10, 0xAB, 0xFF),
                Color.FromArgb(0x01, 0x02, 0x03),
            };
            var writer = new StringWriter();
            var pdnWriter = new PaintDotNetTxtPaletteWriter(writer);
            pdnWriter.WritePalette(pal);
            var result = writer.ToString();
            Assert.That(result, Is.EqualTo("FF10ABFF\r\nFF010203\r\n"));
        }

        [Test]
        public void WriteCommentLine_WritesComment()
        {
            var writer = new StringWriter();
            var pdnWriter = new PaintDotNetTxtPaletteWriter(writer);
            pdnWriter.WriteCommentLine("test");
            var result = writer.ToString();
            Assert.That(result, Is.EqualTo("; test\r\n"));
        }
    }
}
