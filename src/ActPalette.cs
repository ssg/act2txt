using System.Drawing;
using System.Runtime.InteropServices;

namespace act2txt;

// this isn't used for reading data from the file
// directly, so no struct field alignment directive
// is necessary.
public struct ActExtraData
{
    public ushort NumColors;
    public ushort TransparentIndex;
}

public class ActPalette
{
    public const int MaxLengthInBytes = 772; // https://web.archive.org/web/20211101165406/https://www.adobe.com/devnet-apps/photoshop/fileformatashtml/
    public const int MaxColors = 256;

    public IReadOnlyList<Color> Colors { get; }
    public ActExtraData? ExtraData { get; } = null;

    public ActPalette(ReadOnlyMemory<byte> buffer)
    {
        int len = buffer.Length;
        if (len > MaxLengthInBytes)
        {
            throw new ArgumentException("Buffer is too large", nameof(buffer));
        }

        if (len < 3)
        {
            Colors = [];
            return;
        }

        bool hasFullData = len == MaxLengthInBytes;

        int numColors = hasFullData ? MaxColors : len / 3;
        var colors = new Color[numColors];
        Colors = colors;
        var span = buffer.Span;
        for (int i = 0, offset = 0; i < numColors; i++, offset += 3)
        {
            colors[i] = Color.FromArgb(span[offset], span[offset + 1], 
                span[offset + 2]);
        }

        if (hasFullData)
        {
            int offset = numColors * 3;
            ExtraData = new ActExtraData()
            {
                NumColors = (ushort)(span[offset] + (span[offset + 1] << 8)),
                TransparentIndex = (ushort)(span[offset + 2] 
                                            + (span[offset + 3] << 8)),
            };
        }
    }
}
