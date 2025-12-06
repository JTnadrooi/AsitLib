using Microsoft.VisualStudio.TestTools.UnitTesting;
using AsitLib;
using System;

namespace AsitLib.Tests
{
    [TestClass]
    public class DefaultDictionaryTests
    {
        [TestMethod]
        public void TryGetValue_FoundKey()
        {
            DefaultDictionary<int, string> dictionary = new DefaultDictionary<int, string>(() => "default");
            dictionary[1] = "custom value";
            bool result = dictionary.TryGetValue(1, out string value);

            Assert.IsTrue(result);
            Assert.AreEqual("custom value", value);
        }

        [TestMethod]
        public void TryGetValue_NotFoundKey_AlwaysSucceeds()
        {
            DefaultDictionary<int, string> dictionary = new DefaultDictionary<int, string>(() => "default");
            bool result = dictionary.TryGetValue(1, out string value);

            Assert.IsTrue(result);
            Assert.AreEqual("default", value);
        }

        [TestMethod]
        public void GetValue_FoundKey()
        {
            DefaultDictionary<int, string> dictionary = new DefaultDictionary<int, string>(() => "default");
            dictionary[1] = "custom value";

            Assert.AreEqual("custom value", dictionary[1]);
        }

        [TestMethod]
        public void GetValue_NotFoundKey()
        {
            DefaultDictionary<int, string> dictionary = new DefaultDictionary<int, string>(() => "default");

            Assert.AreEqual("default", dictionary[1]);
        }
    }
}
