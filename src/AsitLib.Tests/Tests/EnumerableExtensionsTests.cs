using Microsoft.VisualStudio.TestTools.UnitTesting;
using AsitLib;
using System;
using System.Linq;

namespace AsitLib.Tests
{
    [TestClass]
    public class EnumerableExtensionsTests
    {
        [TestMethod]
        public void EndsWith_ShouldReturnTrue_WhenArrayEndsWithValue()
        {
            int[] source = { 1, 2, 3, 4, 5 };
            int[] value = { 4, 5 };

            bool result = source.EndsWith(value);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void EndsWith_ShouldReturnFalse_WhenArrayDoesNotEndWithValue()
        {
            int[] source = { 1, 2, 3, 4, 5 };
            int[] value = { 3, 4 };

            bool result = source.EndsWith(value);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GetShallowCopy_ShouldReturnIdenticalArray()
        {
            int[] source = { 1, 2, 3, 4, 5 };

            int[] result = source.GetShallowCopy();

            CollectionAssert.AreEqual(source, result);
        }

        [TestMethod]
        public void SwitchIndexes_ShouldSwitchElements()
        {
            int[] source = { 1, 2, 3, 4, 5 };

            source.SwitchIndexes(1, 3);

            Assert.AreEqual(4, source[1]);
            Assert.AreEqual(2, source[3]);
        }

        [TestMethod]
        public void SqueezeIndexes_ShouldRemoveNullsAndKeepNonNullValues()
        {
            int?[] source = { 1, null, 2, null, 3 };

            int?[] result = source.SqueezeIndexes();

            CollectionAssert.AreEqual(new int?[] { 1, 2, 3, null, null }, result);
        }


        [TestMethod]
        public void GetFirstIndexWhere_ShouldReturnCorrectIndex()
        {
            int[] source = { 1, 2, 3, 4, 5 };

            int result = source.GetFirstIndexWhere(x => x == 3);

            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void GetFirstIndexWhere_ShouldThrowException_WhenNoMatch()
        {
            int[] source = { 1, 2, 3, 4, 5 };

            Assert.Throws<InvalidOperationException>(() => source.GetFirstIndexWhere(x => x == 6));
        }

        [TestMethod]
        public void ToJoinedString_ShouldReturnJoinedString()
        {
            int?[] source = { 1, null, 3, 4 };

            string result = source.ToJoinedString(',');

            Assert.AreEqual("1,null,3,4", result);
        }
    }
}
