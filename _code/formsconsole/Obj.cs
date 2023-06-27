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
using System.IO;
using AsitLib;
#nullable enable

namespace AsitLib.FormConsole
{
    /// <summary>
    /// Provides data for the CancelKeyPress event. This class cannot be inherited.
    /// </summary>
    /// <typeparam name="CBase"></typeparam>
    public sealed class AsitConsoleCancelEventArgs<CBase> : EventArgs where CBase : TextBoxBase
    {
        public bool Cancel { get; set; }
        public ConsoleSpecialKey SpecialKey { get; }
        public AsitConsole<CBase> ParentConsole { get; }
        public AsitConsoleCancelEventArgs(bool cancel, ConsoleSpecialKey specialKey, AsitConsole<CBase> parentConsole)
        {
            Cancel = cancel;
            SpecialKey = specialKey;
            ParentConsole = parentConsole;
        }
    }
    /// <summary>
    /// Represents the method that will handle the CancelKeyPress event of a AsitConsole.
    /// </summary>
    /// <typeparam name="CBase">Type of the parent AsitConsole.</typeparam>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A AsitConsoleCancelEventArgs object that contains the event data.</param>
    public delegate void AsitConsoleCancelEventHandler<CBase>(object sender, AsitConsoleCancelEventArgs<CBase> e) where CBase : TextBoxBase;
    /// <summary>
    /// Define <see cref="System.Drawing.Font"/> and color pallete.
    /// </summary>
    public struct Theme
    {
        /// <summary>
        /// Foreground <see cref="Color"/> set by this <see cref="Theme"/>.
        /// </summary>
        public Color ForeColor { get; set; }
        /// <summary>
        /// Background <see cref="Color"/> set by this <see cref="Theme"/>.
        /// </summary>
        public Color BackColor { get; set; }
        /// <summary>
        /// <see cref="System.Drawing.Font"/> set by this <see cref="Theme"/>.
        /// </summary>
        public Font? Font { get; set; }
        /// <summary>
        /// Create a new <see cref="Theme"/> with set values.
        /// </summary>
        /// <param name="forecolor">Foreground <see cref="Color"/>.</param>
        /// <param name="backcolor">Background <see cref="Color"/>.</param>
        /// <param name="font"><see cref="System.Drawing.Font"/> of the new <see cref="Theme"/>.</param>
        public Theme(Color forecolor, Color backcolor, Font? font)
        {
            ForeColor = forecolor;
            BackColor = backcolor;
            Font = font;
        }
        /// <summary>
        /// A empty <see cref="Theme"/>.
        /// </summary>
        public static Theme Empty = new Theme(Color.Empty, Color.Empty, null);
    }
    /// <summary>
    /// A <see cref="TextWriter"/> that writes to a <see cref="TextBox"/>.
    /// </summary>
    /// <typeparam name="TBase">Type of the <see cref="TextBox"/>.</typeparam>
    public class TextBoxTextWriter<TBase> : TextWriter where TBase : TextBoxBase
    {
        /// <summary>
        /// Base <see cref="TextBoxBase"/>.
        /// </summary>
        public TBase Base { get; }
        public override Encoding Encoding => Encoding.UTF8;
        /// <summary>
        /// Create a new <see cref="TextBoxTextWriter{TBase}"/> that points to a <see cref="TextBox"/>.
        /// </summary>
        /// <param name="base"><see cref="TextBox"/> for this <see cref="TextWriter"/></param>
        public TextBoxTextWriter(TBase @base)
        {
            Base = @base;
            base.NewLine = "\n";
        }
        public override void Write(char value)
        {
            if (!Base.Multiline && (value == '\n' || value == '\r')) throw new InvalidOperationException();
            Base.Text += value;
        }
    }
    /// <summary>
    /// A <see cref="TextReader"/> that reads from a <see cref="TextBox"/>.<br/><strong>Locks Thread until user input has been provided.</strong>
    /// </summary>
    /// <typeparam name="TBase">Type of the Textbox.</typeparam>
    public class TextBoxTextReader<TBase> : TextReader where TBase : TextBoxBase
    {
        private bool _reading = false;
        private string _readen = string.Empty;
        private string _buffer = string.Empty;
        /// <summary>
        /// How many Miliseconds the <see cref="TextBoxTextReader{TBase}"/> will wait before checking for input. <i>This loops.</i> 
        /// </summary>
        public int Frequency { get; set; } = 25;
        /// <summary>
        /// The <see cref="TextBox"/> this <see cref="TextReader"/> reads from.
        /// </summary>
        public TBase Base { get; }
        /// <summary>
        /// Construct a new <see cref="TextBoxTextReader{TBase}"/> with set <see cref="Frequency"/>.
        /// </summary>
        /// <param name="base">The <see cref="TextBox"/> this <see cref="TextReader"/> reads from.</param>
        /// <param name="frequency">How many Miliseconds the <see cref="TextBoxTextReader{TBase}"/> will wait before checking for input. <i>This loops.</i> </param>
        public TextBoxTextReader(TBase @base, int frequency) : this(@base) => Frequency = frequency;
        /// <summary>
        /// Construct a new <see cref="TextBoxTextReader{TBase}"/> with set <see cref="Frequency"/>.
        /// </summary>
        /// <param name="base">The <see cref="TextBox"/> this <see cref="TextReader"/> reads from.</param>
        public TextBoxTextReader(TBase @base)
        {
            Base = @base;
            Base.KeyDown += BaseKeyDown;
        }
        public override int Read(char[] buffer, int index, int count)
        {
            if ((index + count) > buffer.Length) throw new Exception();
            string toret = "";
            Used.InvokeEx(Base, x => x.ReadOnly = false);
            toret = Used.NullToEmptyString(ReadLine());
            Used.InvokeEx(Base, x => x.ReadOnly = true);
            if(count < toret.Length)
            toret = toret[..count];
            toret.ToCharArray().CopyTo(buffer, index);
            Array.Copy(buffer.Select(b =>
            {
                if (b == Char.MinValue) return '0';
                else return b;
            }).ToArray(), buffer, buffer.Length);
            //buffer.Where(b => b != '\0').ToArray().CopyTo(buffer, 0);
            //Debug.WriteLine(buffer.Length);
            //Debug.WriteLine("Final result: " + String.Join("--", buffer) + ", L; " + String.Join("--", buffer).Length + "");
            return toret.Length;
        }
        public override string? ReadLine()
        {
            string toret = "";
            Used.InvokeEx(Base, x => x.ReadOnly = false);
            Task task = Task.Run(() =>
            {
                _reading = true;
                while (_readen == string.Empty) Thread.Sleep(Frequency);
                _reading = false;
                toret = _readen[..^1];
                _readen = string.Empty;
                _buffer = string.Empty;
            });
            task.Wait();
            Used.InvokeEx(Base, x => x.ReadOnly = true);
            if (toret == "") return null;
            else return toret;
        }
        public override int Peek() => -1;
        public override int Read()
        {
            string? rdl = ReadLine();
            if (rdl == null) return -1;
            if (rdl.Length > 0) return rdl[0];
            return -1;
        }
        public override string ReadToEnd() => Used.NullToEmptyString(ReadLine());
        public override int ReadBlock(char[] buffer, int index, int count) => Read(buffer, index, count);
        private void BaseKeyDown(object sender, KeyEventArgs e)
        {
            if (_reading)
            {
                _buffer += Used.KeyCodeToUnicode(e.KeyCode);
                if (e.KeyCode == Keys.Enter)
                {
                    _readen = _buffer;
                    _buffer = string.Empty;
                }
            }
        }
    }
}
