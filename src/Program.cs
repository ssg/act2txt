using System.Reflection;
namespace act2txt;

public static class Program
{
    const int maxPaintDotNetColors = 96;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
    // there's no chance executing assembly would be null.
    private static readonly string version = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

    private static int usage()
    {
        Console.WriteLine($"act2txt v{version} - (c) 2021 SSG");
        Console.WriteLine("Converts Adobe Photoshop ACT Palette files to Paint.NET palette TXT format");
        Console.WriteLine("Usage: act2txt [-f] inputfile outputfile");
        Console.WriteLine("  -f    Overwrite output file if it exists");
        return 1;
    }

    public static int Main(string[] args)
    {                
        if (!parseArgs(args, out var files))
        {
            return 1;
        }

        long len = files.input.Length;
        if (len < 3 || (len < ActPalette.MaxLengthInBytes && len % 3 != 0))
        {
            return abort("Input file length isn't correct for ACT");
        }

        if (len > ActPalette.MaxLengthInBytes)
        {
            Console.WriteLine("WARNING: Input file is too big. Only parsing spec-supported length");
            len = ActPalette.MaxLengthInBytes;
        }

        var buffer = new byte[len];
        var inputStream = files.input.OpenRead();
        int readBytes = inputStream.Read(buffer, 0, (int)len);
        if (readBytes != len)
        {
            return abort("Couldn't read file");
        }
        inputStream.Close();
        var act = new ActPalette(buffer);
        Console.WriteLine($"Parsed {act.Colors.Count} colors");
        if (act.Colors.Count > maxPaintDotNetColors)
        {
            Console.WriteLine($"Pain.NET supports only {maxPaintDotNetColors}, the rest may be ignored by the app");
        }

        writeActToTxt(act, files.output);
        Console.WriteLine($"Written output to {files.output.FullName}");
        return 0;
    }

    static void writeActToTxt(ActPalette act, FileInfo output)
    {
        var writer = output.CreateText();
        var pw = new PaintDotNetTxtPaletteWriter(writer);
        pw.WriteCommentLine($"Created by act2txt v{version} - https://github.com/ssg/act2txt");
        if (act.ExtraData is ActExtraData xd)
        {
            pw.WriteCommentLine($"ACT number of colors = {xd.NumColors}");
            pw.WriteCommentLine($"ACT transparent color index = {xd.TransparentIndex}");
        }
        pw.WritePalette(act.Colors);
        writer.Close();
    }

    static int abort(string msg)
    {
        Console.WriteLine(msg);
        return 1;
    }

    static bool fail(string message)
    {
        Console.WriteLine(message);
        return false;
    }

    static bool parseArgs(string[] args, out (FileInfo input, FileInfo output) files)
    {
        if (args.Length is < 2 or > 3)
        {
            _ = usage();
            Environment.Exit(1);
        }

        files = default;
        int argIndex = 0;
        bool overwrite = false;
        if (args.Length == 3)
        {
            if (args[0] == "-f")
            {
                overwrite = true;
                argIndex++;
            }
            else
            {
                return fail($"Invalid option: {args[0]}");
            }
        }
        files.input = new FileInfo(args[argIndex]);
        if (!files.input.Exists)
        {
            return fail($"Input file not found");
        }

        files.output = new FileInfo(args[argIndex + 1]);
        return overwrite || !files.output.Exists || fail("Output file already exists");
    }
}

