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

namespace AsitLib.Encoders
{
    public class ASIT1W : Encoding
    {
        //public override int GetByteCount(char[] chars, int index, int count) => GetBytes(chars).Length;
        
        public override int GetByteCount(char[] chars, int index, int count) => chars.Length;
        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            string main = chars.ToJoinedString();
            main = main[charIndex..(charIndex + charCount)];
            string toret = String.Empty;
            byte[] toretbytes;
            //Console.WriteLine(chars.ToJoinedString());
            int id = 0;
            string[] dictionary = String.Join("", chars).Split(' ').GroupBy(
                s => s, 
                s => s,
                (value, values) => new
                {
                    Value = value,
                    Count = values.Count(),
                }).Select(a =>
                {
                    if (a.Count <= 1) return String.Empty;
                    else
                    {
                        id++;
                        return "|" + id.ToString() + a.Value;
                    }
                }).Where(s => s != String.Empty).ToArray();
            toret += dictionary.ToJoinedString().Length.ToString().Length;
            if(toret.Length != 1) throw new InvalidCastException();
            toret += dictionary.ToJoinedString().Length;
            toret += dictionary.ToJoinedString();
            Console.WriteLine(dictionary.ToJoinedString());
            foreach(string s in dictionary)
            {
                string _id = s.Split("|")[0];
                string value = s.Split("|")[1];
                main = main.Replace(value, _id);
            }
            toret += main;
            //Console.WriteLine("\n" + toret);
            toretbytes = Encoding.UTF8.GetBytes(toret);
            Array.Copy(toretbytes, 0, bytes, byteIndex, bytes.Length);

            //using (StringReader stringReader = new StringReader(chars.ToJoinedString()))
            //{
            //    for (int i = 0; i < lenght; i++)
            //    {
            //        char current = (char)stringReader.Read();
            //        Console.Write(current);
            //    }
            //}
            return bytes.Length;
        }
        public override int GetCharCount(byte[] bytes, int index, int count) => bytes.Length;
        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            string main = Encoding.UTF8.GetString(bytes);
            return 0;
        }
        public override int GetMaxByteCount(int charCount) => charCount;
        public override int GetMaxCharCount(int byteCount) => byteCount;
        public override string GetString(byte[] bytes) => GetChars(bytes).ToJoinedString();
    }
}
