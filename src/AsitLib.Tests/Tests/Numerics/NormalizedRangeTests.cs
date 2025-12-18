namespace AsitLib.Numerics.Tests
{
    [TestClass]
    public class NormalizedRangeTests
    {
        [TestMethod]
        public void NormalizedRange_Constructor_ValidRange()
        {
            NormalizedRange range = new NormalizedRange(1, 5);
            Assert.AreEqual(1, range.Start);
            Assert.AreEqual(5, range.End);
            Assert.AreEqual(4, range.Lenght);
        }

        [TestMethod]
        public void NormalizedRange_Constructor_InvalidRange()
        {
            Assert.Throws<InvalidOperationException>(() => new NormalizedRange(5, 1));
        }

        [TestMethod]
        public void NormalizedRange_Equals_True()
        {
            NormalizedRange range1 = new NormalizedRange(1, 5);
            NormalizedRange range2 = new NormalizedRange(1, 5);
            Assert.IsTrue(range1.Equals(range2));
        }

        [TestMethod]
        public void NormalizedRange_Equals_False()
        {
            NormalizedRange range1 = new NormalizedRange(1, 5);
            NormalizedRange range2 = new NormalizedRange(2, 6);
            Assert.IsFalse(range1.Equals(range2));
        }

        [TestMethod]
        public void NormalizedRange_IsEmpty_True()
        {
            NormalizedRange range = new NormalizedRange(1, 1);
            Assert.IsTrue(range.IsEmpty);
        }

        [TestMethod]
        public void NormalizedRange_IsEmpty_False()
        {
            NormalizedRange range = new NormalizedRange(1, 5);
            Assert.IsFalse(range.IsEmpty);
        }

        [TestMethod]
        public void NormalizedRange_Contains_True()
        {
            NormalizedRange range = new NormalizedRange(1, 5);
            Assert.IsTrue(range.Contains(3));
        }

        [TestMethod]
        public void NormalizedRange_Contains_False()
        {
            NormalizedRange range = new NormalizedRange(1, 5);
            Assert.IsFalse(range.Contains(6));
        }

        [TestMethod]
        public void NormalizedRange_AsRange()
        {
            NormalizedRange range = new NormalizedRange(1, 5);
            Assert.AreEqual("1..5", range.ToString());
        }

        [TestMethod]
        public void NormalizedRange_ImplicitOperator()
        {
            NormalizedRange range = new NormalizedRange(1, 5);
            Range convertedRange = range;
            Assert.AreEqual("1..5", convertedRange.ToString());
        }

        [TestMethod]
        public void NormalizedRange_GetFromValues()
        {
            NormalizedRange range = NormalizedRange.GetFromValues(5, 1);
            Assert.AreEqual(1, range.Start);
            Assert.AreEqual(5, range.End);
        }

        [TestMethod]
        public void NormalizedRange_CreateFromFactory()
        {
            Range range = new Range(1, 5);
            int[] values = { 1, 2, 3, 4, 5 };
            NormalizedRange normalizedRange = NormalizedRange.CreateFromFactory(range, values);
            Assert.AreEqual(1, normalizedRange.Start);
            Assert.AreEqual(5, normalizedRange.End);
        }
    }
}
