internal static class PipelineTests
{
    public static void Run()
    {
        var examples = new Dictionary<string, string>
        {
            ["caresses"] = "caress", ["ponies"] = "poni", ["cats"] = "cat",
            ["agreed"] = "agre", ["plastered"] = "plaster", ["motoring"] = "motor",
            ["hopping"] = "hop", ["tanned"] = "tan", ["falling"] = "fall",
            ["hissing"] = "hiss", ["fizzed"] = "fizz", ["failing"] = "fail",
            ["filing"] = "file", ["happy"] = "happi", ["sky"] = "sky",
            ["relational"] = "relat", ["conditional"] = "condit", ["vietnamization"] = "vietnam",
            ["triplicate"] = "triplic", ["formative"] = "form", ["goodness"] = "good",
            ["replacement"] = "replac", ["adoption"] = "adopt", ["probate"] = "probat",
            ["rate"] = "rate", ["cease"] = "ceas", ["controll"] = "control", ["roll"] = "roll"
        };
        foreach (var (word, expected) in examples)
            if (PorterStemmer.Stem(word) != expected)
                throw new Exception($"{word}: asteptat {expected}, obtinut {PorterStemmer.Stem(word)}");
        string[] terms = TextProcessing.Terms("The students are parking cars; students parked a car.");
        if (string.Join(" ", terms) != "student park car student park car")
            throw new Exception("Prelucrarea textului este incorecta.");
        var counts = terms.GroupBy(t => t).ToDictionary(g => g.Key, g => g.Count());
        if (counts.Count != 3 || counts.Values.Any(v => v != 2))
            throw new Exception("Frecventele trebuie sa includa repetitiile.");
        if (TextProcessing.Terms("the and 123 !!!").Length != 0)
            throw new Exception("Articolul fara termeni trebuie sa produca un vector gol.");
        Console.WriteLine("OK: 28 exemple Porter, prelucrare text, frecvente si articol fara termeni.");
    }
}
