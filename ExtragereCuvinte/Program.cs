using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

if (args.Contains("--self-test"))
{
    PipelineTests.Run();
    return;
}

string root = args.Length > 0 ? Path.GetFullPath(args[0])
    : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../Reuters/Reuters_34"));
if (!Directory.Exists(root))
    throw new DirectoryNotFoundException(root);
string[] files = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
    .Where(p => Path.GetExtension(p).Equals(".xml", StringComparison.OrdinalIgnoreCase))
    .OrderBy(p => Path.GetRelativePath(root, p), StringComparer.Ordinal).ToArray();
if (files.Length == 0)
    throw new InvalidOperationException("Nu exista articole XML in " + root);

var vocabulary = new Dictionary<string, int>(StringComparer.Ordinal);
string output = Path.Combine(root, "Rezultate", "Vectorizare");
Directory.CreateDirectory(output);
var vectors = new List<string>();
using var articles = new StreamWriter(Path.Combine(output, "articole.tsv"), false, new UTF8Encoding(false));
using var processed = new StreamWriter(Path.Combine(output, "articole_prelucrate.tsv"), false, new UTF8Encoding(false));
articles.WriteLine("rand\tfisier\titemid\tset\tcuvinte_pastrate\ttermeni_distincti");
processed.WriteLine("rand\tfisier\tradacini");
long total = 0;
int row = 0;
foreach (string file in files)
{
   
    XDocument document = XDocument.Load(file);
    string text = string.Join(" ", document.Descendants("text").Descendants("p").Select(p => p.Value));
    if (!document.Descendants("text").Any())
        throw new InvalidDataException("Lipseste textul articolului: " + file);
    string[] terms = TextProcessing.Terms(text);
    var counts = new SortedDictionary<int, int>();
    foreach (string term in terms)
    {
        if (!vocabulary.TryGetValue(term, out int index))
        {
            index = vocabulary.Count;
            vocabulary.Add(term, index);
        }
        counts[index] = counts.GetValueOrDefault(index) + 1;
    }
    vectors.Add(string.Join(" ", counts.Select(kv => $"{kv.Key}:{kv.Value}")));
    string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
    string set = relative.StartsWith("Training/", StringComparison.OrdinalIgnoreCase) ? "Training"
        : relative.StartsWith("Testing/", StringComparison.OrdinalIgnoreCase) ? "Testing" : "Corpus";
    row++;
    articles.WriteLine($"{row}\t{relative}\t{(string?)document.Root?.Attribute("itemid")}\t{set}\t{terms.Length}\t{counts.Count}");
    processed.WriteLine($"{row}\t{relative}\t{string.Join(" ", terms)}");
    total += terms.Length;
}
using (var writer = new StreamWriter(Path.Combine(output, "dictionar_si_vectori.txt"), false, new UTF8Encoding(false)))
{
    writer.WriteLine("DICTIONAR GLOBAL");
    writer.WriteLine("indice\tradacina");
    foreach (var term in vocabulary)
        writer.WriteLine($"{term.Value}\t{term.Key}");
    writer.WriteLine();
    writer.WriteLine("VECTORI RARI");
    foreach (string vector in vectors)
        writer.WriteLine(vector);
}

File.Delete(Path.Combine(output, "dictionar_global.tsv"));
File.Delete(Path.Combine(output, "vectori_rari.txt"));
File.WriteAllText(Path.Combine(output, "raport.txt"),
    $"Articole: {row}\nTermeni in dictionar: {vocabulary.Count}\nCuvinte pastrate: {total}\n" +
    "Prelucrare: corpul articolului, litere mici, tokenizare, stopwords engleza, stemming Porter.\n" +
    "Dictionar comun intregii colectii, inclusiv Training si Testing, indici de la 0, ordine de prima aparitie.\n" +
    "Fisier: dictionar_si_vectori.txt; mai intai DICTIONAR GLOBAL, apoi VECTORI RARI.\n" +
    "Al n-lea rand dupa VECTORI RARI corespunde articolului cu rand=n din articole.tsv.\n" +
    "Format: indice:frecventa bruta; componentele zero sunt omise; rand gol = articol fara termeni.\n",
    new UTF8Encoding(false));
Console.WriteLine($"{row} articole; {vocabulary.Count} termeni; {total} cuvinte pastrate.\nRezultate: {output}");

internal static class TextProcessing
{
    
    private static readonly HashSet<string> Stopwords = new((
        "a about above after again against all am an and any are aren as at be because been before being below between both " +
        "but by can couldn d did didn do does doesn doing don down during each few for from further had hadn has hasn " +
        "have haven having he her here hers herself him himself his how i if in into is isn it its itself just ll m ma " +
        "me mightn more most mustn my myself needn no nor not now o of off on once only or other our ours ourselves out " +
        "over own re s same shan she should shouldn so some such t than that the their theirs them themselves then there " +
        "these they this those through to too under until up ve very was wasn we were weren what when where which while " +
        "who whom why will with won would wouldn y you your yours yourself yourselves").Split(' '), StringComparer.Ordinal);

    public static string[] Terms(string text) => Regex.Matches(text.ToLowerInvariant(), @"\p{L}+")
        .Select(m => m.Value).Where(w => !Stopwords.Contains(w))
        .Select(PorterStemmer.Stem).ToArray();
}
