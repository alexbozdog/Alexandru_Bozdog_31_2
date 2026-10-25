
internal static class PorterStemmer
{
    private static bool Consonant(string s, int i) => s[i] switch
    {
        'a' or 'e' or 'i' or 'o' or 'u' => false,
        'y' => i == 0 || !Consonant(s, i - 1),
        _ => true
    };
    private static int Measure(string s)
    {
        int count = 0;
        for (int i = 1; i < s.Length; i++)
            if (!Consonant(s, i - 1) && Consonant(s, i)) count++;
        return count;
    }
    private static bool HasVowel(string s) => Enumerable.Range(0, s.Length).Any(i => !Consonant(s, i));
    private static bool DoubleConsonant(string s) => s.Length >= 2 && s[^1] == s[^2] && Consonant(s, s.Length - 1);
    private static bool Cvc(string s) => s.Length >= 3 && Consonant(s, s.Length - 3)
        && !Consonant(s, s.Length - 2) && Consonant(s, s.Length - 1) && s[^1] is not ('w' or 'x' or 'y');

    public static string Stem(string word)
    {
        if (word.Length <= 2) return word;
        string s = word;
        if (s.EndsWith("sses", StringComparison.Ordinal)) s = s[..^2];
        else if (s.EndsWith("ies", StringComparison.Ordinal)) s = s[..^2];
        else if (!s.EndsWith("ss", StringComparison.Ordinal) && s.EndsWith('s')) s = s[..^1];

        if (s.EndsWith("eed", StringComparison.Ordinal))
        {
            if (Measure(s[..^3]) > 0) s = s[..^1];
        }
        else
        {
            int suffix = s.EndsWith("ed", StringComparison.Ordinal) ? 2
                : s.EndsWith("ing", StringComparison.Ordinal) ? 3 : 0;
            if (suffix > 0 && HasVowel(s[..^suffix]))
            {
                s = s[..^suffix];
                if (s.EndsWith("at", StringComparison.Ordinal) || s.EndsWith("bl", StringComparison.Ordinal) || s.EndsWith("iz", StringComparison.Ordinal)) s += "e";
                else if (DoubleConsonant(s) && s[^1] is not ('l' or 's' or 'z')) s = s[..^1];
                else if (Measure(s) == 1 && Cvc(s)) s += "e";
            }
        }
        if (s.EndsWith('y') && HasVowel(s[..^1])) s = s[..^1] + "i";
        s = Replace(s, Step2, 0);
        s = Replace(s, Step3, 0);
        foreach (string suffix in Step4)
        {
            if (!s.EndsWith(suffix, StringComparison.Ordinal)) continue;
            string stem = s[..^suffix.Length];
            if (Measure(stem) > 1 && (suffix != "ion" || stem.EndsWith('s') || stem.EndsWith('t'))) s = stem;
            break;
        }
        if (s.EndsWith('e'))
        {
            string stem = s[..^1];
            int m = Measure(stem);
            if (m > 1 || (m == 1 && !Cvc(stem))) s = stem;
        }
        if (s.EndsWith('l') && DoubleConsonant(s) && Measure(s) > 1) s = s[..^1];
        return s;
    }
    private static string Replace(string s, (string Suffix, string Replacement)[] rules, int minMeasure)
    {
        foreach (var rule in rules)
        {
            if (!s.EndsWith(rule.Suffix, StringComparison.Ordinal)) continue;
            string stem = s[..^rule.Suffix.Length];
            return Measure(stem) > minMeasure ? stem + rule.Replacement : s;
        }
        return s;
    }
    private static readonly (string, string)[] Step2 =
    [
        ("ational", "ate"), ("tional", "tion"), ("enci", "ence"), ("anci", "ance"),
        ("izer", "ize"), ("abli", "able"), ("alli", "al"), ("entli", "ent"), ("eli", "e"),
        ("ousli", "ous"), ("ization", "ize"), ("ation", "ate"), ("ator", "ate"),
        ("alism", "al"), ("iveness", "ive"), ("fulness", "ful"), ("ousness", "ous"),
        ("aliti", "al"), ("iviti", "ive"), ("biliti", "ble")
    ];
    private static readonly (string, string)[] Step3 =
    [("icate", "ic"), ("ative", ""), ("alize", "al"), ("iciti", "ic"), ("ical", "ic"), ("ful", ""), ("ness", "")];
    private static readonly string[] Step4 =
    ["al", "ance", "ence", "er", "ic", "able", "ible", "ant", "ement", "ment", "ent", "ion", "ou", "ism", "ate", "iti", "ous", "ive", "ize"];
}
