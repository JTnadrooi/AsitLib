using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace AsitLib
{
    public static class AsitTupleExtensions
    {
        public static object?[] GetItems(this ITuple tuple)
        {
            //if (!(tuple is ITuple)) throw new Exception();

            List<object?> toret2 = new List<object?>();
            for (int i = 0; i < tuple.Length; i++)
                toret2.Add(tuple[i]);
            return toret2.ToArray();

            //List<object?> toret = new List<object?>();
            //Type t = tuple.GetType();
            //if (t.IsValueType)
            //{
            //    var itemI = t.GetFields();
            //    foreach (var field in itemI)
            //        toret.Add(field.GetValue(tuple));
            //}
            //else
            //{
            //    var itemI = t.GetProperties();
            //    foreach (var property in itemI)
            //        toret.Add(property.GetValue(tuple));
            //}
            //return toret.ToArray();
        }
    }
}
