using AsitLib;
using AsitLib.IO;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
#nullable enable


namespace AsitLib.StringContructor
{
    /// <summary>
    /// A <see langword="struct"/> that modifies a <see cref="string"/> to suit the <see cref="StringContructor"/> namespace.
    /// </summary>
    public struct GString
    {
        /// <summary>
        /// May this <see cref="GString"/> only exist on the end of a <see cref="string"/>?
        /// </summary>
        public bool FromEnd { get; }
        /// <summary>
        /// May this <see cref="GString"/> only exist on the start of a <see cref="string"/>?
        /// </summary>
        public bool FromStart { get; }
        /// <summary>
        /// Value this <see cref="GString"/> holds, excludes indicators.
        /// </summary>
        public string Value { get; }
        /// <summary>
        /// Weight of this <see cref="GString"/>, more weight equals more chance of this being roled when picking a random <see cref="GString"/>
        /// in the <see cref="StringConstructorUtils.GetFromGSArray(GString[], int, Random, StringContructSearchMode)"/> method.
        /// </summary>
        public int Weight { get; set; }
        /// <summary>
        /// Contruct a <see cref="GString"/> from a <see cref="char"/> and add specified <paramref name="weight"/>.
        /// </summary>
        /// <param name="c">A <see cref="char"/> to contruct a new <see cref="GString"/> from.</param>
        /// <param name="weight">Set the <see cref="GString.Weight"/> property.</param>
        /// <exception cref="InvalidCastException"></exception>
        public GString(char c, int weight) : this(c.ToString(), weight) { }
        /// <summary>
        /// Contruct a <see cref="GString"/> from a <see cref="char"/>.
        /// </summary>
        /// <param name="c">A <see cref="char"/> to contruct a new <see cref="GString"/> from.</param>
        /// <exception cref="InvalidCastException"></exception>
        public GString(char c) : this(c.ToString()) { }
        /// <summary>
        /// Contruct a <see cref="GString"/> from a <see cref="string"/>.
        /// </summary>
        /// <param name="s">A <see cref="string"/> to contruct a new <see cref="GString"/> from.</param>
        /// <exception cref="InvalidCastException"></exception>
        public GString(string s) : this(s, 128) { }
        /// <summary>
        /// Contruct a <see cref="GString"/> from a <see cref="string"/> and add specified <paramref name="weight"/>.
        /// </summary>
        /// <param name="s">A <see cref="string"/> to contruct a new <see cref="GString"/> from.</param>
        /// <param name="weight">Set the <see cref="GString.Weight"/> property.</param>
        /// <exception cref="InvalidCastException"></exception>
        public GString(string s, int weight)
        {
            if (s == null) throw new NullReferenceException("S is null.");
            if (s == String.Empty) throw new InvalidOperationException("Can't create a GString from a empty string.");
            if (s.Any(c => c < 32)) throw new InvalidCastException("Forbidden chacters detected.");
            if (s == "..") throw new InvalidCastException("\"..\" is invalid.");
            Value = s;
            Weight = weight;
            if (s.EndsWith(".."))
            {
                FromStart = true;
                Value = Value[..^2];
            }
            else FromStart = false;
            if (s.StartsWith(".."))
            {
                FromEnd = true;
                Value = Value[2..];
            }
            else FromEnd = false;
            if (FromEnd && FromStart) throw new InvalidCastException("GString cant be FromEnd & FromStart simultaneously.");
           
        }
        public readonly bool Char => Value.Length == 1;
        public override readonly string ToString()
        {
            if (FromEnd) return ".." + Value;
            else if (FromStart) return Value + "..";
            return Value;
        }
        public static implicit operator string(GString ths) => ths.Value;
        public static implicit operator GString(string s) => new GString(s);
        public static implicit operator GString(char c) => new GString(c);
    }
    /// <summary>
    /// Configuration used when creating <see cref="string"/> objects in the <see cref="StringConstructorUtils.Generate(AsitContructorConfig)"/> method.
    /// </summary>
    public struct AsitContructorConfig
    {
        private int? accuracy;
        private int? attempts;
        private int? seed;
        private int? lenght;
        private bool? discardWeight;
        private GString[]? occurences;
        private Func<string, bool> validator;
        private Func<string, string> manipulator;
        public AsitContructorConfig(AsitContructorConfig @base)
        {
            accuracy = @base.Accuracy;
            attempts = @base.Attempts;
            seed = @base.Seed;
            lenght = @base.Lenght;
            discardWeight = @base.DiscardWeight;
            occurences = @base.Occurrences;
            validator = @base.Validator;
            manipulator = @base.Manipulator;
        }
        /// <summary>
        /// How accurate the procces of finding the random <see cref="GString"/> will be. <br/><strong>2 is ~10 times as slow as 1.</strong>
        /// </summary>
        public int Accuracy { readonly get => accuracy ?? 1; set => accuracy = value; }
        /// <summary>
        /// Lenght of the constructed <see cref="string"/> in <see cref="GString"/> objects.
        /// </summary>
        public int Lenght { readonly get => lenght ?? 7; set => lenght = value; }
        /// <summary>
        /// How many attemps will be taken to find a suitable constructed <see cref="string"/>.
        /// </summary>
        public int Attempts { readonly get => attempts ?? 100; set => attempts = value; }
        /// <summary>
        /// <see cref="Func{T, TResult}"/> that edits the generated <see cref="string"/> before it gets passed throught the <see cref="Validator"/>.
        /// </summary>
        public Func<string, string> Manipulator { readonly get => manipulator ?? new Func<string, string>((str) => str); set => manipulator = value; }
        /// <summary>
        /// <see cref="Func{T, TResult}"/> that checks if the generated <see cref="string"/> suits the user's needs.
        /// </summary>
        public Func<string, bool> Validator { readonly get => validator ?? new Func<string, bool>((str) => true); set => validator = value; }
        /// <summary>
        /// Seed for the <see cref="Random"/> used by the <see cref="StringConstructorUtils.Generate(AsitContructorConfig)"/> method.
        /// </summary>
        public int Seed { readonly get => seed ?? -1; set => seed = value; }
        /// <summary>
        /// If <see langword="true"/>, the <see cref="GString.Weight"/> property of all the <see cref="GString"/> objects used to construct the string gets ignored.
        /// </summary>
        public bool DiscardWeight { readonly get => discardWeight ?? false; set => discardWeight = value; }
        /// <summary>
        /// <see cref="Array"/> of <see cref="GString"/> objects that are used for the contruction.
        /// </summary>w
        public GString[] Occurrences { readonly get => occurences ?? StringConstructorUtils.Learn("nadrooi").ChangeValues(i => 1); set => occurences = value; }
        
    }
    /// <summary>
    /// Thrown when values given to contruct a <see cref="string"/> are invalid. <br/>
    /// More exact reasons can be of the following;<br/>
    /// <i>-The <see cref="AsitContructorConfig"/> is invalid.</i><br/>
    /// <i>-The <see cref="AsitContructorConfig.Attempts"/> is to low resulting in a timeout.</i><br/>
    /// </summary>
    public class InvalidRequestException : Exception
    {
        public InvalidRequestException() { }
        public InvalidRequestException(string message) : base(message) { }
        public InvalidRequestException(string message, Exception inner) : base(message, inner) { }
        protected InvalidRequestException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    
}
