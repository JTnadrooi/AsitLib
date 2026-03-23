using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsitLib.Autocompleting
{
    public sealed class CompletionCollection : IReadOnlyList<Completion>, ICollection<Completion>
    {
        public int Count => _completions.Count;
        public bool IsReadOnly => false;

        public Completion this[int index] => _completions[index];

        private List<Completion> _completions;

        public CompletionCollection()
        {
            _completions = new List<Completion>();
        }

        public CompletionCollection(IEnumerable<string> completions)
        {
            _completions = new List<Completion>(completions.Select(c => new Completion(c, 1)));
        }

        public CompletionCollection(IEnumerable<Completion> completions)
        {
            _completions = new List<Completion>(completions);
        }

        public void Add(Completion item)
        {
            _completions.Add(item);
        }

        public void Clear()
        {
            _completions.Clear();
        }

        public bool Contains(Completion item)
        {
            return _completions.Contains(item);
        }

        public void CopyTo(Completion[] array, int arrayIndex)
        {
            ((ICollection<Completion>)_completions).CopyTo(array, arrayIndex);
        }

        public IEnumerator<Completion> GetEnumerator()
        {
            return ((IEnumerable<Completion>)_completions).GetEnumerator();
        }

        public bool Remove(Completion item)
        {
            return _completions.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_completions).GetEnumerator();
        }
    }

    public class Autocompletion
    {
        public ImmutableArray<string> OrderedOptions { get; } // best first

        public Autocompletion(ImmutableArray<string> orderedOptions)
        {
            OrderedOptions = orderedOptions;
        }
    }

    public static class Autocomplete
    {
        public static Autocompletion Complete(string str, CompletionCollection completions, int? threshold = null, Func<string, Completion, float>? multiplierProvider = null)
        {
            str = str.Trim();

            if (completions.Count == 0)
            {
                throw new ArgumentException("Completion collection cannot be empty", nameof(completions));
            }

            if (str == string.Empty)
            {
                return new Autocompletion(completions
                   .OrderBy(c => c.Text.Length)
                   .ThenByDescending(c => c.Value)
                   .Select(c => c.Text)
                   .ToImmutableArray());
            }

            int[] sourceCodePoints = str.EnumerateRunes()
                .Select(r => r.Value)
                .ToArray();

            List<(string Text, double Score)> results = new List<(string Text, double Score)>(completions.Count);

            foreach (Completion completion in completions)
            {
                int distance = DamerauLevenshteinDistance(sourceCodePoints, completion.CodePoints, threshold ?? Math.Max(3, Math.Max(sourceCodePoints.Length, completion.CodePoints.Length) / 2));

                if (distance != int.MaxValue)
                {
                    double score = completion.Value / (double)(distance + 1);

                    if (multiplierProvider is not null) score *= multiplierProvider.Invoke(str, completion);

                    //Console.WriteLine("s" + score + " " + completion.Text);

                    results.Add((completion.Text, score));
                }
            }

            return new Autocompletion(results
                .OrderByDescending(x => x.Score)
                .Select(x => x.Text).ToImmutableArray());
        }

        /// <summary>
        /// Computes the Damerau-Levenshtein Distance between two strings, represented as arrays of
        /// integers, where each integer represents the code point of a character in the source string.
        /// Includes an optional threshhold which can be used to indicate the maximum allowable distance.
        /// </summary>
        /// <param name="source">An array of the code points of the first string</param>
        /// <param name="target">An array of the code points of the second string</param>
        /// <param name="threshold">Maximum allowable distance</param>
        /// <returns>Int.MaxValue if threshhold exceeded; otherwise the Damerau-Leveshteim distance between the strings.</returns>
        private static int DamerauLevenshteinDistance(Span<int> source, Span<int> target, int threshold = int.MaxValue)
        {
            static void Swap(ref Span<int> a, ref Span<int> b)
            {
                Span<int> temp = a;
                a = b;
                b = temp;
            }

            int length1 = source.Length;
            int length2 = target.Length;

            if (Math.Abs(length1 - length2) > threshold) return int.MaxValue;

            if (length1 > length2)
            {
                Swap(ref target, ref source);
                (length1, length2) = (length2, length1);
            }

            const int StackAllocLimit = 256;
            Span<int> dCurrent = length1 + 1 <= StackAllocLimit ? stackalloc int[length1 + 1] : new int[length1 + 1];
            Span<int> dMinus1 = length1 + 1 <= StackAllocLimit ? stackalloc int[length1 + 1] : new int[length1 + 1];
            Span<int> dMinus2 = length1 + 1 <= StackAllocLimit ? stackalloc int[length1 + 1] : new int[length1 + 1];

            for (int i = 0; i <= length1; i++)
            {
                dCurrent[i] = i;
            }

            int jm1 = 0;

            for (int j = 1; j <= length2; j++)
            {
                Span<int> temp = dMinus2;
                dMinus2 = dMinus1;
                dMinus1 = dCurrent;
                dCurrent = temp;

                int minDistance = int.MaxValue;
                dCurrent[0] = j;

                int im1 = 0;
                int im2 = -1;

                for (int i = 1; i <= length1; i++)
                {
                    int cost = source[im1] == target[jm1] ? 0 : 1;

                    int del = dCurrent[im1] + 1;
                    int ins = dMinus1[i] + 1;
                    int sub = dMinus1[im1] + cost;

                    int min = del;
                    if (ins < min) min = ins;
                    if (sub < min) min = sub;

                    if (i > 1 && j > 1 && source[im2] == target[jm1] && source[im1] == target[j - 2])
                    {
                        int trans = dMinus2[im2] + cost;
                        if (trans < min) min = trans;
                    }

                    dCurrent[i] = min;

                    if (min < minDistance) minDistance = min;
                    im1++;
                    im2++;
                }

                jm1++;
                if (minDistance > threshold) return int.MaxValue;
            }

            int result = dCurrent[length1];
            return result > threshold ? int.MaxValue : result;
        }
    }

    public readonly struct Completion : IEquatable<Completion>
    {
        public string Text { get; }
        public int Value { get; }
        public int[] CodePoints { get; }

        public Completion(string text, int value)
        {
            Text = text;
            Value = value;
            CodePoints = text.EnumerateRunes().Select(r => r.Value).ToArray();
        }

        public bool Equals(Completion other)
        {
            return Value == other.Value && Text == other.Text;
        }

        public override bool Equals(object? obj)
        {
            return obj is Completion other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Text, Value);
        }

        public override string ToString()
        {
            return $"{{Completion: '{Text}', Value: '{Value}'}}";
        }

        public static bool operator ==(Completion left, Completion right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Completion left, Completion right)
        {
            return !left.Equals(right);
        }
    }
}
