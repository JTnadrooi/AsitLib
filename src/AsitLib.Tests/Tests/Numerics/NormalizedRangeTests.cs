namespace AsitLib.Numerics.Tests
{
    [TestClass]
    public class NormalizedRangeTests
    {
        [TestMethod]
        public void NormalizedRange_Constructor_ValidRange()
        {
            NormalizedRange range = new NormalizedRange(1, 5);
            range.Start.Should().Be(1);
            range.End.Should().Be(5);
            range.Lenght.Should().Be(4);
        }

        [TestMethod]
        public void NormalizedRange_Constructor_InvalidRange()
        {
            Invoking(() => new NormalizedRange(5, 1)).Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void NormalizedRange_Equals_SameValues_ReturnsTrue()
        {
            NormalizedRange range1 = new NormalizedRange(1, 5);
            NormalizedRange range2 = new NormalizedRange(1, 5);

            range1.Should().Be(range2);
        }

        [TestMethod]
        public void NormalizedRange_Equals_DifferentValues_ReturnsFalse()
        {
            NormalizedRange range1 = new NormalizedRange(1, 5);
            NormalizedRange range2 = new NormalizedRange(2, 6);

            range1.Should().NotBe(range2);
        }

        [TestMethod]
        public void NormalizedRange_IsEmpty_True()
        {
            NormalizedRange range = new NormalizedRange(1, 1);
            range.IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void NormalizedRange_IsEmpty_False()
        {
            NormalizedRange range = new NormalizedRange(1, 5);
            range.IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void NormalizedRange_Contains_True()
        {
            NormalizedRange range = new NormalizedRange(1, 5);
            range.Contains(3).Should().BeTrue();
        }

        [TestMethod]
        public void NormalizedRange_Contains_False()
        {
            NormalizedRange range = new NormalizedRange(1, 5);
            range.Contains(6).Should().BeFalse();
        }

        [TestMethod]
        public void NormalizedRange_AsRange()
        {
            NormalizedRange range = new NormalizedRange(1, 5);
            range.ToString().Should().Be("1..5");
        }

        [TestMethod]
        public void NormalizedRange_ImplicitOperator()
        {
            NormalizedRange range = new NormalizedRange(1, 5);
            Range convertedRange = range;
            convertedRange.ToString().Should().Be("1..5");
        }

        [TestMethod]
        public void NormalizedRange_GetFromValues()
        {
            NormalizedRange range = NormalizedRange.GetFromValues(5, 1);
            range.Start.Should().Be(1);
            range.End.Should().Be(5);
        }

        [TestMethod]
        public void NormalizedRange_CreateFromFactory()
        {
            Range range = new Range(1, 5);
            int[] values = { 1, 2, 3, 4, 5 };
            NormalizedRange normalizedRange = NormalizedRange.CreateFromFactory(range, values);
            normalizedRange.Start.Should().Be(1);
            normalizedRange.End.Should().Be(5);
        }
    }
}
