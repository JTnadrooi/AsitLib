using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
using System.Runtime.Versioning;
using System.Diagnostics;

namespace AsitLib.FormConsole
{
    /// <summary>
    /// A <see langword="static"/> <see langword="class"/> containing multiple utils for the <see cref="FormConsole"/> namespace.
    /// </summary>
    public static class ConsoleUtils
    {
        /// <summary>
        /// Export a <see cref="Theme"/> from a <see langword="class"/> that implements <see cref="TextBoxBase"/>.
        /// </summary>
        /// <typeparam name="T">A <see cref="Type"/> that implements <see cref="TextBoxBase"/>.</typeparam>
        /// <param name="base"><see cref="TextBox"/> to get a <see cref="Theme"/> from.</param>
        /// <returns>A <see cref="Theme"/> exported from the <see cref="TextBox"/></returns>
        public static Theme ExportTheme<T>(T @base) where T : TextBoxBase => new Theme(@base.ForeColor, @base.BackColor, @base.Font);
        /// <summary>
        /// Import a <see cref="Theme"/> to the <see langword="class"/> that implements <see cref="TextBoxBase"/>.
        /// </summary>
        /// <typeparam name="T">A <see cref="Type"/> that implements <see cref="TextBoxBase"/>.</typeparam>
        /// <param name="theme"><see cref="Theme"/> to import.</param>
        /// <param name="base"><see cref="TextBox"/> to import the <see cref="Theme"/> to.</param>
        public static void ImportTheme<T>(this Theme theme, T @base) where T : TextBoxBase
        {
            @base.ForeColor = theme.ForeColor;
            @base.BackColor = theme.BackColor;
            @base.Font = theme.Font;
        }
    }
}
