using System.Reflection;

const long maxLen = 772; // https://web.archive.org/web/20211101165406/https://www.adobe.com/devnet-apps/photoshop/fileformatashtml/
const int maxColors = 256;
const int maxPaintDotNetColors = 96;
string version = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
ExtraData? extraData = null;

if (args.Length != 2)
{
    Console.WriteLine($"act2txt v{version} - convert Adobe Palette files to Paint.Net palette - (c) 2021 SSG");
    abort("Usage: act2txt inputfile outputfile");
}

var inputFile = new FileInfo(args[0]);
if (!inputFile.Exists)
{
    abort($"Input file not found");
}

long len = inputFile.Length;
if (len < 3)
{
    abort("Input file too small");
}

if (len % 3 != 0 || len > maxLen)
{
    Console.WriteLine("Input file doesn't seem to be a valid ACT file, but anyway...");
}

int numColors;
if (len == maxLen)
{
    numColors = maxColors;
} 
else
{
    numColors = ((int)len) / 3;
}
Console.WriteLine($"Processing {numColors} colors");
if (numColors > maxPaintDotNetColors)
{
    Console.WriteLine($"Paint.Net only supports {maxPaintDotNetColors}, it will ignore the rest, fyi");
}

var inputStream = inputFile.OpenRead();
var buffer = new byte[numColors * 3];

inputStream.Read(buffer);
if (len == maxLen)
{
    var tempBuffer = new byte[4];
    inputStream.Read(tempBuffer);
    extraData = new ExtraData()
    {
        NumColors = tempBuffer[0] + (tempBuffer[1] << 8),
        TransparentIndex = tempBuffer[2] + (tempBuffer[3] << 16),
    };
}
inputStream.Close();

if (File.Exists(args[1]))
{
    abort("Output file already exists");
}

var writer = File.CreateText(args[1]);
writer.WriteLine($"; Created by act2txt v{version} - https://github.com/ssg/act2txt");
if (extraData is not null)
{
    writer.WriteLine($"; ACT reported number of colors = {extraData?.NumColors}");
    writer.WriteLine($"; ACT reported transparent color index = {extraData?.TransparentIndex}");
}

for (int i = 0; i < numColors; i++)
{
    int offset = i * 3;
    writer.WriteLine($"FF{buffer[offset]:X2}{buffer[offset + 1]:X2}{buffer[offset + 2]:X2}");
}
writer.Close();

Console.WriteLine($"Written output to {args[1]}");

static void abort(string msg)
{
    Console.WriteLine(msg);
    Environment.Exit(1);
}

struct ExtraData
{
    public int NumColors;
    public int TransparentIndex;
}