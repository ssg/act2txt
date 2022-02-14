using System.Drawing;

namespace act2txt;

public class PaintDotNetTxtPaletteWriter
{
    public PaintDotNetTxtPaletteWriter(TextWriter writer)
    {
        Writer = writer;
    }

    public TextWriter Writer { get; }

    public void WritePalette(IReadOnlyList<Color> colors)
    {
        foreach (var color in colors)
        {
            WriteColor(color);
        }
    }

    public void WriteColor(Color color)
    {
        Writer.WriteLine($"{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}");
    }

    public void WriteCommentLine(string text)
    {
        Writer.WriteLine($"; {text}");
    }
}
