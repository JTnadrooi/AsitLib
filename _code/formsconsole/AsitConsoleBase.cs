using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
using System.Runtime.Versioning;
using System.IO;
using AsitLib;
#nullable enable

namespace AsitLib.FormConsole
{
    /// <summary>
    /// A class to help with converting <see cref="Console"/> Applications to a <see cref="Form"/> enviorment.
    /// </summary>
    /// <typeparam name="Tbase">A type that implements TextBoxBase.</typeparam>
    public class AsitConsole<Tbase> : IDisposable where Tbase : TextBoxBase
    {
        private TextWriter _out;
        private TextReader _in;
        private TextWriter _error;
        private TextWriter _warning;

        private Theme _defTheme;
        private bool _reading = false;
        private string _readen = string.Empty;
        private string _buffer = string.Empty;
        private KeyEventArgs _lastKey = new KeyEventArgs(Keys.Sleep);
        private bool disposedValue;

        /// <summary>
        /// Base <see cref="TextBox"/>.
        /// </summary>
        public Tbase Base { get; internal set; }
        /// <summary>
        /// How many ms it will take for the Reading methods to check for input.
        /// </summary>
        public int Frequency { get; set; }
        /// <summary>
        /// Create a new <see cref="AsitConsole{Tbase}"/> from a <see cref="TextBox"/>.
        /// </summary>
        /// <param name="_base"><see cref="TextBox"/> to convert to a <see cref="AsitConsole{Tbase}"/>.</param>
        public AsitConsole(Tbase _base)
        {
            Base = _base;
            Base.ReadOnly = true;
            Base.KeyDown += BaseKeyDown;
            Base.TextChanged += SetCursor;
            Base.GotFocus += SetCursor;
            _defTheme = ConsoleUtils.ExportTheme(_base);
            _in = new TextBoxTextReader<Tbase>(Base);
            _out = new TextBoxTextWriter<Tbase>(Base);
            _warning = new TextBoxTextWriter<Tbase>(Base);
            _error = new TextBoxTextWriter<Tbase>(Base);
            Frequency = 25;
            TreatControlCAsInput = false;
            //Base.Enter += SetCursor;
            //Base.Click += SetCursor;
            //Base.DoubleClick += SetCursor;
            //Base.GotFocus += SetCursor;
        }
        /// <summary>
        /// Gets or sets a value indicating whether the combination of the Control modifier key and C console key (Ctrl+C) is treated as ordinary input or as an interruption that is handled by the operating system.
        /// </summary>
        public bool TreatControlCAsInput { get; set; }
        private void SetCursor(object? sender, EventArgs e) => Base.SelectionStart = Base.Text.Length;
        /// <summary>
        /// Gets a value indicating if the CAPSLOCK toggle is turned on or off.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Mirroring default behavior.")]
        public bool CapsLock => Console.CapsLock;
        /// <summary>
        /// Background color of the AsitConsole. (<see cref="ConsoleColor"/>)
        /// </summary>
        public ConsoleColor BackgroundColor
        {
            get { return Used.ColorToConsoleColor(Base.BackColor); }
            set { Base.BackColor = Used.DrawingColor(value); }
        }
        /// <summary>
        /// Foreground color of the AsitConsole. (<see cref="ConsoleColor"/>)
        /// </summary>
        public ConsoleColor ForegroundColor
        {
            get { return Used.ColorToConsoleColor(Base.ForeColor); }
            set { Base.ForeColor = Used.DrawingColor(value); }
        }
        /// <summary>
        /// Gets a value indicating if the NUMLOCK toggle is turned on or off.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Mirroring default behavior.")]
        public bool NumberLock => Console.NumberLock;
        /// <summary>
        /// Gets a value indicating if the cursor is visible.
        /// </summary>
        public bool CursorVisible
        {
            get { throw new NotImplementedException(); }
        }
        /// <summary>
        /// Gets a value that indicates whether warning has been redirected from the standard <see cref="Warning"/> stream.
        /// </summary>
        public bool IsWarningRedirected { get; internal set; }
        /// <summary>
        /// The warning <see cref="TextWriter"/>.
        /// </summary>
        public TextWriter Warning
        {
            get
            {
                if (_warning == null) throw new ArgumentNullException();
                else return _warning;
            }
        }
        /// <summary>
        /// Set the <see cref="Warning"/> stream to a new <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="newWarning">The new <see cref="TextWriter"/>.</param>
        public void SetWarning(TextWriter newWarning)
        {
            _warning = newWarning;
            IsWarningRedirected = true;
        }
        /// <summary>
        /// Gets a value that indicates whether error has been redirected from the standard <see cref="Error"/> stream.
        /// </summary>
        public bool IsErrorRedirected { get; internal set; }
        /// <summary>
        /// The error textwriter.
        /// </summary>
        public TextWriter Error
        {
            get 
            {
                if (_error == null) throw new ArgumentNullException();
                else return _error;
            }
        }
        /// <summary>
        /// Set the <see cref="Error"/> stream to a new <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="newError">The new <see cref="Error"/> <see cref="TextWriter"/>.</param>
        public void SetError(TextWriter newError)
        {
            _error = newError;
            IsErrorRedirected = true;
        }
        /// <summary>
        /// Gets a value that indicates whether input has been redirected from the standard <see cref="In"/> stream.
        /// </summary>
        public bool IsInputRedirected { get; internal set; }
        /// <summary>
        /// The <see cref="In"/> <see cref="TextReader"/>.
        /// </summary>
        public TextReader In
        {
            get
            {
                if (_in == null) throw new ArgumentNullException();
                else return _in;
            }
        }
        /// <summary>
        /// Set the <see cref="In"/> stream to a new <see cref="TextReader"/>.
        /// </summary>
        /// <param name="newIn">The new <see cref="In"/> <see cref="TextReader"/>.</param>
        public void SetIn(TextReader newIn)
        {
            _in = newIn;
            IsInputRedirected = true;
        }
        /// <summary>
        /// Gets a value that indicates whether <see cref="Out"/> has been redirected from the standard <see cref="Out"/> stream.
        /// </summary>
        public bool IsOutputRedirected { get; internal set; }
        /// <summary>
        /// The output <see cref="TextWriter"/>.
        /// </summary>
        public TextWriter Out
        {
            get
            {
                if (_out == null) throw new ArgumentNullException();
                else return _out;
            }
        }
        /// <summary>
        /// Set the <see cref="Out"/> stream to a new <see cref="TextWriter"/>..
        /// </summary>
        /// <param name="newOut">The new <see cref="Out"/> <see cref="TextWriter"/>.</param>
        public void SetOut(TextWriter newOut)
        {
            _out = newOut;
            IsOutputRedirected = true;
        }
        /// <summary>
        /// <see cref="Title"/> of this <see cref="AsitConsole{Tbase}"/>. Gets and Sets the <see cref="Form.Text"/> value of the parent <see cref="Form"/>.
        /// </summary>
        public string? Title
        {
            get => (Used.GetParentForm(this.Base) ?? throw new Exception("Asitconsole doesnt come from a form.")).Text;
            set => (Used.GetParentForm(this.Base) ?? throw new Exception("Asitconsole doesnt come from a form.")).Text = value;
        }
        /// <summary>
        /// Export the <see cref="Theme"/> of this <see cref="AsitConsole{Tbase}"/>>.
        /// </summary>
        /// <returns>The <see cref="Theme"/> of this <see cref="AsitConsole{Tbase}"/></returns>
        public Theme GetTheme() => ConsoleUtils.ExportTheme(Base);
        /// <summary>
        /// Import a <see cref="Theme"/> to this <see cref="AsitConsole{Tbase}"/>.
        /// </summary>
        /// <param name="theme">The <see cref="Theme"/> to import.</param>
        public void SetTheme(Theme theme) => ConsoleUtils.ImportTheme(theme, Base);
        /// <summary>
        /// Reset the <see cref="Theme"/>.
        /// </summary>
        public void SetTheme() => SetTheme(_defTheme);
        private void BaseKeyDown(object sender, KeyEventArgs e)
        {
            _lastKey = e;
            if (_reading)
            {
                _buffer += Used.KeyCodeToUnicode(e.KeyCode);
                if (e.KeyCode == Keys.Enter)
                {
                    _readen = _buffer;
                    _buffer = string.Empty;
                }
            }
            if (!TreatControlCAsInput)
            {
                if (e.KeyData.ToString() == "C, Control")
                {
                    CancelKeyPress?.Invoke(this, new AsitConsoleCancelEventArgs<Tbase>(true, ConsoleSpecialKey.ControlC, this));
                    Environment.Exit(-1073741510);
                }
                else if (e.KeyData.ToString() == "Pause, Control")
                {
                    CancelKeyPress?.Invoke(this, new AsitConsoleCancelEventArgs<Tbase>(true, ConsoleSpecialKey.ControlBreak, this));
                    Environment.Exit(-1073741510);
                }
            }
            //^^This is not yet confirmed, I don't have a break key.. Todo; get a keyboard.
        }
        /// <summary>
        /// Write a value to this <see cref="AsitConsole{Tbase}"/>.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="value">Value to write.</param>
        public void Write<T>(T value) => Out.Write(value);
        /// <summary>
        /// Write a lineending to the AsitConsole.
        /// </summary>
        public void WriteLine() => Out.Write("\n");
        /// <summary>
        /// Write a value to the <see cref="AsitConsole{Tbase}"/>. and end it with a line ending (<see langword="\n"/>).
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="value">Value to write.</param>
        public void WriteLine<T>(T value) => Out.WriteLine(value);
        /// <summary>
        /// Read user input.
        /// </summary>
        /// <returns></returns>
        public string ReadLine() => In.ReadLine() ?? throw new ArgumentNullException("Invalid user input.");
        public int Read() => In.Read();
        /// <summary>
        /// Read user input from a <see langword="class"/> that implements <see cref="TextBoxBase"/>.
        /// </summary>
        /// <param name="base">A <see langword="class"/> that implements <see cref="TextBoxBase"/>.</param>
        /// <returns>User input.</returns>
        public string ReadLine(TextBoxBase @base)
        {
            string rd;
            Base.KeyDown -= BaseKeyDown;
            @base.KeyDown += BaseKeyDown;
            rd = ReadLine();
            @base.KeyDown -= BaseKeyDown;
            Base.KeyDown += BaseKeyDown;
            return rd;
        }
        /// <summary>
        /// Clear the <see cref="AsitConsole{Tbase}"/>.
        /// </summary>
        public void Clear() => Base.Text = string.Empty;
        /// <summary>
        /// Clear a line from the <see cref="AsitConsole{Tbase}"/>.
        /// </summary>
        /// <param name="linenum">The line to remove.</param>
        public void Clear(Index linenum)
        {
            List<string> towork = Base.Text.Split("\n").ToList();
            if (linenum.IsFromEnd) towork.RemoveAt(linenum.Value);
            else towork.RemoveAt(towork.Count - linenum.Value);
            Base.Text = string.Join("\n", towork);
        }
        /// <summary>
        /// Remove the lines in the <see cref="Range"/> from the <see cref="AsitConsole{Tbase}"/>.
        /// </summary>
        /// <param name="range"><see cref="Range"/> of lines.</param>
        public void Clear(Range range)
        {
            List<string> towork = Base.Text.Split("\n").ToList();
            for (int i = 0; i < range.GetOffsetAndLength(towork.Count).Length; i++)
            {
                towork[i + range.GetOffsetAndLength(towork.Count).Offset] = "\n";
            }
            Base.Text = string.Join("\n", towork.Where(s => s != "\n"));
        }
        /// <summary>
        /// Read <see cref="ConsoleKeyInfo"/> from the user input.
        /// </summary>
        /// <returns>A <see cref="ConsoleKeyInfo"/> read from the input.</returns>
        public ConsoleKeyInfo ReadKey()
        {
            ConsoleKeyInfo toret = new ConsoleKeyInfo();
            Task task = Task.Run(() =>
            {
                KeyEventArgs last = _lastKey;
                while (last == _lastKey) Thread.Sleep(Frequency);
                Enum.TryParse(_lastKey.KeyCode.ToString(), out ConsoleKey consoleKey);
                toret = new ConsoleKeyInfo(_lastKey.KeyCode.ToString()[0], consoleKey, _lastKey.Shift, _lastKey.Alt, _lastKey.Control);
            });
            task.Wait();
            return toret;
        }
        /// <summary>
        /// Read a <see cref="Keys"/> from the user input.
        /// </summary>
        /// <returns>A <see cref="Keys"/> read from the input.</returns>
        public Keys ReadFormsKey()
        {
            Keys toret = Keys.None;
            Task task = Task.Run(() =>
            {
                KeyEventArgs last = _lastKey;
                while (last == _lastKey) Thread.Sleep(Frequency);
                toret = last.KeyData;
            });
            task.Wait();
            return toret;
        }
        /// <summary>
        /// Set all <see cref="TextWriter"/> and <see cref="TextReader"/> objects to default.
        /// </summary>
        public void SetToDefault()
        {
            _in = new TextBoxTextReader<Tbase>(Base);
            _out = new TextBoxTextWriter<Tbase>(Base);
            _warning = new TextBoxTextWriter<Tbase>(Base);
            _error = new TextBoxTextWriter<Tbase>(Base);
        }
        /// <summary>
        /// Set <see cref="ForegroundColor"/> and <see cref="BackgroundColor"/> color of the <see cref="AsitConsole{Tbase}"/>.
        /// </summary>
        /// <param name="backColor">New <see cref="ForegroundColor"/>.</param>
        /// <param name="foreColor">New <see cref="BackgroundColor"/>.</param>
        public void SetColor(Color backColor, Color foreColor)
        {
            Base.ForeColor = foreColor;
            Base.BackColor = backColor;
        }
        /// <summary>
        /// Set <see cref="ForegroundColor"/> and <see cref="BackgroundColor"/> color of the <see cref="AsitConsole{Tbase}"/>.
        /// </summary>
        /// <param name="backColor">New <see cref="ForegroundColor"/>.</param>
        /// <param name="foreColor">New <see cref="BackgroundColor"/>.</param>
        public void SetColor(ConsoleColor backColor, ConsoleColor foreColor)
        {
            ForegroundColor = foreColor;
            BackgroundColor = backColor;
        }
        /// <summary>
        /// Play a beep sound.
        /// </summary>
        public void Beep() => Console.Beep();
        /// <summary>
        /// Play a beep sound with custom values.
        /// </summary>
        /// <param name="frequency">Frequency of the beep.</param>
        /// <param name="duration">Duration of the beep.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Mirroring default behavior.")]
        public void Beep(int frequency, int duration) => Console.Beep(frequency, duration);
        /// <summary>
        /// Occurs when the Control modifier key (Ctrl) and either the C console key (C) or the Break key are pressed simultaneously (Ctrl+C or Ctrl+Break).
        /// </summary>
        public event AsitConsoleCancelEventHandler<Tbase>? CancelKeyPress;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    
                }
                this.Warning?.Dispose();
                this.Error?.Dispose();
                this.In?.Dispose();
                this.Out?.Dispose();
                _readen = String.Empty;
                _lastKey = (KeyEventArgs)KeyEventArgs.Empty;
                _defTheme = Theme.Empty;
                _buffer = String.Empty;
                this.CancelKeyPress = null;
                this.Frequency = 0;
                disposedValue = true;
            }
        }
        /// <summary>
        /// Finalizer.
        /// </summary>
        ~AsitConsole() => Dispose(disposing: false);
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        
    }
}
