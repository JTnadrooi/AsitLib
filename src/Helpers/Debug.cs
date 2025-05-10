using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
#nullable enable

namespace AsitLib.Debug
{

    /// <summary>
    /// A static <see langword="class"/> containing methods to help debugging.
    /// </summary>
    public class AsitDebugStream
    {
        public char TabFiller { get; }
        public char TabEnd { get; }
        public char PrefixFiller { get; }
        public TextWriter Out { get; set; }
        public TextReader In { get; set; }
        public bool IsEnabled
        {
            get => Priority > 0;
            set => Priority = value ? int.MaxValue : -1;    
        }
        public int Priority { get; set; }

        public int TabSize { get; }
        public int TabType { get; }

        public Encoding Encoding => Out.Encoding;

        private readonly Stopwatch?[] localStopwatches;
        
        public AsitDebugStream(bool enabled = true)
        {
            //Stopwatch sw = Stopwatch.StartNew();
            localStopwatches = new Stopwatch[10];
            
            PrefixFiller = ':';
            IsEnabled = enabled;

            TabSize = 4;
            TabFiller = '-';
            TabEnd = '^';
            TabType = 2;

            Out = Console.Out;
            In = Console.In;

            WriteLine("[s]initializing debug variables..");
            Succes();
        }
        public string ReadLine(int depth = 1)
        {
            if (Priority < depth) throw new Exception();
            Write(new string('\t', depth) + ">");
            return In.ReadLine()!;
        }
        public void Write<T>(T value, Stopwatch? stopwatch = null)
        {
            string s = value == null ? "Null" : value.ToString()!;
            int tabCount = s.Length - s.TrimStart('\t').Length;
            if (Priority < tabCount) return;
            string prefix = tabCount == 0 ? "^" + new string(TabFiller, TabSize - 1) : string.Empty;
            //prefix = string.Empty;
            bool newLine = false;
            s = s.TrimStart('\t');

            if (s.EndsWith('\n'))
            {
                s = s.TrimEnd('\n');
                newLine = true;
            }
            string tabs = tabCount != 0 ? new string(' ', (tabCount) * TabSize) + TabEnd + new string(TabFiller, TabSize - 1) : string.Empty;
            tabs = new string(' ', (tabCount) * TabSize) + TabEnd + new string(TabFiller, TabSize - 1);

            if (s.StartsWith("[s]"))
            {
                tabs = tabs[..^3];
                localStopwatches[tabCount] = Stopwatch.StartNew();
            }
            Out.Write(/*prefix +*/ tabs + s +
                (stopwatch == null ? string.Empty : "time taken: " + stopwatch!.ElapsedMilliseconds + "ms") +
                (newLine ? "\n" : string.Empty));
            Out.Flush();

            //Console.WriteLine((s.StartsWith('\t') ? "^" + new string(tabFiller, (s.Length - s.TrimStart('\t').Length) * tabSize - 1) + s.TrimStart('\t') : s)
            //    + (stopwatch == null ? string.Empty : " time taken: " + stopwatch!.ElapsedMilliseconds + "ms"));
        }
        public void WriteLine<T>(T value, Stopwatch? stopwatch = null) => Write((value ?? (T)(object)"Null").ToString()! + "\n", stopwatch);
        public void WriteLine(string s, Stopwatch? stopwatch = null) => Write(s + "\n", stopwatch);
        public void Succes(string tabs) => Succes(tabs.Count(c => c == '\t'));
        public void Succes(int depth = 1)
        {
            if (Priority < depth) return;
            if (localStopwatches[depth - 1] != null)
            {
                WriteLine(new string('\t', depth) + "succes. ", localStopwatches[depth - 1]);
                localStopwatches[depth - 1] = null;
            }
            else WriteLine(new string('\t', depth) + "succes. time taken: ??ms");
        }
        public void Fail(string tabs) => Fail(tabs.Count());
        public void Fail(int depth = 1)
        {
            if (Priority < depth) return;
            if (localStopwatches[depth - 1] != null)
            {
                WriteLine(new string('\t', depth) + "failure. ", localStopwatches[depth - 1]);
                localStopwatches[depth - 1] = null;
            }
            else WriteLine(new string('\t', depth) + "failure. time taken: ??ms");
        }
        public void WriteStripe(int aboveTabs, int lenght = 30, bool many = false, string trail = "")
        {
            if (!IsEnabled) return;
            if (many)
                Out.WriteLine(TabEnd + new string(TabFiller, lenght - 1).ReplaceAt(new AsitRange(((TabSize * aboveTabs) - 1)..((TabSize * (aboveTabs + 1)) - 1), (TabEnd + new string(TabFiller, lenght - 1)).Length), TabEnd) + trail);
            else
                Out.WriteLine(TabEnd + new string(TabFiller, lenght - 1).ReplaceAt((TabSize * aboveTabs) - 1, TabEnd) + trail);
            Out.Flush();
        }
        public void Help()
        {
            if (Priority < 0) return;
            Out.WriteLine("I guess this is the part where I link a nice help pdf file..." +
                "\nuhmm.." +
                "\n.." +
                "\nany second now..." +
                "\n(you're not waiting for anything.)");
            Out.Flush();
        }
    }
    public class TextWriterDebug : System.IO.TextWriter
    {
        public override System.Text.Encoding Encoding
        {
            get { return System.Text.Encoding.Default; }
        }

        //public override System.IFormatProvider FormatProvider
        //{ get; }
        //public override string NewLine
        //{ get; set; }

        public override void Close()
        {
            System.Diagnostics.Debug.Close();
            base.Close();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public override void Flush()
        {
            System.Diagnostics.Debug.Flush();
            base.Flush();
        }

        public override void Write(bool value)
        {
            System.Diagnostics.Debug.Write(value);
        }

        public override void Write(char value)
        {
            System.Diagnostics.Debug.Write(value);
        }

        public override void Write(char[] buffer)
        {
            System.Diagnostics.Debug.Write(buffer);
        }

        public override void Write(decimal value)
        {
            System.Diagnostics.Debug.Write(value);
        }

        public override void Write(double value)
        {
            System.Diagnostics.Debug.Write(value);
        }

        public override void Write(float value)
        {
            System.Diagnostics.Debug.Write(value);
        }

        public override void Write(int value)
        {
            System.Diagnostics.Debug.Write(value);
        }

        public override void Write(long value)
        {
            System.Diagnostics.Debug.Write(value);
        }

        public override void Write(object value)
        {
            System.Diagnostics.Debug.Write(value);
        }

        public override void Write(string value)
        {
            System.Diagnostics.Debug.Write(value);
        }

        public override void Write(uint value)
        {
            System.Diagnostics.Debug.Write(value);
        }

        public override void Write(ulong value)
        {
            System.Diagnostics.Debug.Write(value);
        }

        public override void Write(string format, object arg0)
        {
            System.Diagnostics.Debug.Write(string.Format(format, arg0));
        }

        public override void Write(string format, params object[] arg)
        {
            System.Diagnostics.Debug.Write(string.Format(format, arg));
        }

        public override void Write(char[] buffer, int index, int count)
        {
            string x = new string(buffer, index, count);
            System.Diagnostics.Debug.Write(x);
        }

        public override void Write(string format, object arg0, object arg1)
        {
            System.Diagnostics.Debug.Write(string.Format(format, arg0, arg1));
        }

        public override void Write(string format, object arg0, object arg1, object arg2)
        {
            System.Diagnostics.Debug.Write(string.Format(format, arg0, arg1, arg2));
        }

        public override void WriteLine()
        {
            System.Diagnostics.Debug.WriteLine(string.Empty);
        }

        public override void WriteLine(bool value)
        {
            System.Diagnostics.Debug.WriteLine(value);
        }

        public override void WriteLine(char value)
        {
            System.Diagnostics.Debug.WriteLine(value);
        }

        public override void WriteLine(char[] buffer)
        {
            System.Diagnostics.Debug.WriteLine(buffer);
        }

        public override void WriteLine(decimal value)
        {
            System.Diagnostics.Debug.WriteLine(value);
        }

        public override void WriteLine(double value)
        {
            System.Diagnostics.Debug.WriteLine(value);
        }

        public override void WriteLine(float value)
        {
            System.Diagnostics.Debug.WriteLine(value);
        }

        public override void WriteLine(int value)
        {
            System.Diagnostics.Debug.WriteLine(value);
        }

        public override void WriteLine(long value)
        {
            System.Diagnostics.Debug.WriteLine(value);
        }

        public override void WriteLine(object value)
        {
            System.Diagnostics.Debug.WriteLine(value);
        }

        public override void WriteLine(string value)
        {
            System.Diagnostics.Debug.WriteLine(value);
        }

        public override void WriteLine(uint value)
        {
            System.Diagnostics.Debug.WriteLine(value);
        }

        public override void WriteLine(ulong value)
        {
            System.Diagnostics.Debug.WriteLine(value);
        }

        public override void WriteLine(string format, object arg0)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(format, arg0));
        }

        public override void WriteLine(string format, params object[] arg)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(format, arg));
        }

        public override void WriteLine(char[] buffer, int index, int count)
        {
            string x = new string(buffer, index, count);
            System.Diagnostics.Debug.WriteLine(x);

        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(format, arg0, arg1));
        }

        public override void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(format, arg0, arg1, arg2));
        }

    }
}
