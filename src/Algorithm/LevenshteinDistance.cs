using System;

class LevenshteinDistance
{
    public static int Compute(string s1, string s2)
    {
        int[,] d = new int[s1.Length + 1, s2.Length + 1];

        // Initialize the distance matrix
        for (int i = 0; i <= s1.Length; i++)
        {
            d[i, 0] = i;
        }

        for (int j = 0; j <= s2.Length; j++)
        {
            d[0, j] = j;
        }

        // Compute the Levenshtein distance
        for (int i = 1; i <= s1.Length; i++)
        {
            for (int j = 1; j <= s2.Length; j++)
            {
                int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;

                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[s1.Length, s2.Length];
    }

    public static double ComputeSimilarity(string s1, string s2)
    {
        int distance = Compute(s1, s2);
        int maxLen = Math.Max(s1.Length, s2.Length);
        if (maxLen == 0) return 100.0; // Both strings are empty

        double similarity = (1.0 - ((double)distance / maxLen)) * 100.0;
        return similarity;
    }
}
