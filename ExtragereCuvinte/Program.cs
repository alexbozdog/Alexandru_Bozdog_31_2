using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

string root =
    @"C:\Users\Alex\Desktop\ReutersDataSet\Reuters\Reuters_34";

string outputFolder = Path.Combine(root, "Rezultate");
Directory.CreateDirectory(outputFolder);

string[] ExtractWords(string text)
{
    return Regex.Matches(text.ToLowerInvariant(), @"\p{L}+")
        .Cast<Match>()
        .Select(match => match.Value)
        .ToArray();
}

foreach (string folder in new[] { "Training", "Testing" })
{
    string inputFolder = Path.Combine(root, folder);

    if (!Directory.Exists(inputFolder))
    {
        Console.WriteLine($"Folder inexistent: {inputFolder}");
        continue;
    }

    string outputFile = Path.Combine(
        outputFolder, folder.ToLowerInvariant() + ".txt");

    int fileCount = 0;
    long wordCount = 0;

   
    using StreamWriter writer =
        new StreamWriter(outputFile, false, Encoding.UTF8);

    foreach (string file in Directory.EnumerateFiles(
        inputFolder, "*.XML", SearchOption.AllDirectories)
        .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
    {
        try
        {
            XDocument document = XDocument.Load(file);

           
            string text = string.Join(" ",
                document.Descendants("text")
                    .Descendants("p")
                    .Select(p => p.Value));

           
            var metadata = document.Descendants("metadata");

            string metadataText = string.Join(" ",
                metadata.SelectMany(m =>
                    m.DescendantNodes().OfType<XText>())
                    .Select(t => t.Value));

            string metadataCodes = string.Join(" ",
                metadata.Descendants("code")
                    .Select(c => (string?)c.Attribute("code"))
                    .OfType<string>());

            string[] words = ExtractWords(
                text + " " + metadataText + " " + metadataCodes);

            foreach (string word in words)
            {
                writer.WriteLine(word);
            }

            fileCount++;
            wordCount += words.Length;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Eroare în {Path.GetFileName(file)}: {ex.Message}");
        }
    }

    Console.WriteLine(
        $"{folder}: {fileCount} fișiere, {wordCount} cuvinte.");
    Console.WriteLine($"Salvat în: {outputFile}");
}

Console.WriteLine("Extragerea s-a terminat.");