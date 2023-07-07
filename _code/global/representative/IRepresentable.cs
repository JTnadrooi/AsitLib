using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace AsitLib
{
    public interface IRepresentable
    {
        [return: NotNull]
        public string ToRepresentative();
    }
    public static class Representable
    {
        public static string ToString(this object? obj, PresentingLevel level)
        {
            if(obj as IRepresentable != null) return ToString(obj as IRepresentable, level);
            else return level switch
            {
                PresentingLevel.None => obj == null ? "Null" : string.Empty,
                PresentingLevel.Default => obj?.ToString() ?? throw new ArgumentNullException(nameof(obj)),
                _ => throw new ArgumentException(nameof(level) + ", " + nameof(obj) + "does not implement " + nameof(IRepresentable)),
            };
        }
        public static string ToString(this IRepresentable? representable, PresentingLevel level)
            => level switch
            {
                PresentingLevel.None => representable == null ? "Null" : string.Empty,
                PresentingLevel.Default => representable?.ToString() ?? throw new ArgumentNullException(nameof(representable)),
                PresentingLevel.Full => representable?.ToRepresentative() ?? throw new ArgumentNullException(nameof(representable)),
                _ => throw new ArgumentNullException(nameof(level)),
            };
    }
    public enum PresentingLevel
    {
        None = 0,
        Default = 5,
        Full = 10,
    }
}
