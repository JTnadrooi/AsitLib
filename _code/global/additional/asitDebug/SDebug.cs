using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
#nullable enable

namespace AsitLib.Debug
{
    /// <summary>
    /// A static <see langword="class"/> containing methods to help debugging.
    /// </summary>
    public static class AsitDebug
    {
        /// <summary>
        /// The debug <see cref="TextWriter"/> used for writing methods. <br/>
        /// <i><see langword="null"/> if targetting .</i>
        /// </summary>
        public static TextWriter? Writer { get; set; }
        /// <summary>
        /// Call <see cref="DebugDisplay(object?)"/> on all given <paramref name="values"/>.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="values">The values to enumerate through.</param>
        public static void Enumerate<T>(IEnumerable<T> values)
        {
            foreach (T t in values)
                DebugDisplay(t);
        }
        /// <summary>
        /// Display the <see cref="object.ToString"/> return value via the <see cref="Writer"/>.
        /// </summary>
        /// <param name="o">The <see cref="object"/> to display using their <see cref="object.ToString"/> method.</param>
        public static void DebugDisplay(this object? o)
        {
            if(Writer == null)
            {
                if (o == null) Console.WriteLine("Null");
                else Console.WriteLine(o.ToString());
            }
            else
            {
                if (o == null) Writer.WriteLine("Null");
                else Writer.WriteLine(o.ToString());
            }
        }
    }
}
