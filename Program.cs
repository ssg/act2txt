﻿using System.Reflection;

const long maxActFileLength = 772; // https://web.archive.org/web/20211101165406/https://www.adobe.com/devnet-apps/photoshop/fileformatashtml/
const int maxColors = 256;
const int maxPaintDotNetColors = 96;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
string version = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
ExtraData? extraData = null;

if (args.Length != 2)
{
    Console.WriteLine($"act2txt v{version} - (c) 2021 SSG");
    Console.WriteLine("Converts Adobe Photoshop ACT Palette files to Paint.NET palette TXT format");
    abort("Usage: act2txt inputfile outputfile");
}

var inputFile = new FileInfo(args[0]);
if (!inputFile.Exists)
{
    abort($"Input file not found");
}

var outputFile = new FileInfo(args[1]);
if (outputFile.Exists)
{
    abort("Output file already exists");
}

long len = inputFile.Length;
if (len < 3)
{
    abort("Input file is too small");
}

if (len % 3 != 0 || len > maxActFileLength)
{
    Console.WriteLine("Input file doesn't seem to be a valid ACT file, but anyway...");
}

int numColors = len == maxActFileLength ? maxColors : ((int)len) / 3;
Console.WriteLine($"Processing {numColors} colors");
if (numColors > maxPaintDotNetColors)
{
    Console.WriteLine($"Paint.NET only supports {maxPaintDotNetColors}, it will ignore the rest, fyi");
}

var inputStream = inputFile.OpenRead();
var buffer = new byte[numColors * 3];

inputStream.Read(buffer);
if (len == maxActFileLength)
{
    extraData = readExtraData();
}
inputStream.Close();

var writer = File.CreateText(outputFile.FullName);
writer.WriteLine($"; Created by act2txt v{version} - https://github.com/ssg/act2txt");
if (extraData is not null)
{
    writer.WriteLine($"; ACT number of colors = {extraData?.NumColors}");
    writer.WriteLine($"; ACT transparent color index = {extraData?.TransparentIndex}");
}

for (int i = 0; i < buffer.Length; i += 3)
{
    writer.WriteLine($"FF{buffer[i]:X2}{buffer[i + 1]:X2}{buffer[i + 2]:X2}");
}
writer.Close();

Console.WriteLine($"Written output to {outputFile.FullName}");
Environment.Exit(0);

ExtraData readExtraData()
{
    var tempBuffer = new byte[4];
    inputStream.Read(tempBuffer); // guaranteed to succeed
    return new ExtraData()
    {
        NumColors = tempBuffer[0] + (tempBuffer[1] << 8),
        TransparentIndex = tempBuffer[2] + (tempBuffer[3] << 16),
    };
}

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