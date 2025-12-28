namespace AsitLib.Numerics.Tests
{
    [TestClass]
    public class NormalizedRangeTests
    {
        [TestMethod]
        public void StartEndConstructor()
        {
            NormalizedRange range = new NormalizedRange(1, 5);
            range.Start.Should().Be(1);
            range.End.Should().Be(5);
            range.Lenght.Should().Be(4);
        }

        [TestMethod]
        public void StartEndConstructor_InvalidRange()
        {
            Invoking(() => new NormalizedRange(5, 1)).Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void Equals_SameValues_ReturnsTrue()
        {
            NormalizedRange range1 = new NormalizedRange(1, 5);
            NormalizedRange range2 = new NormalizedRange(1, 5);

            range1.Should().Be(range2);
        }

        [TestMethod]
        public void Equals_DifferentValues_ReturnsFalse()
        {
            NormalizedRange range1 = new NormalizedRange(1, 5);
            NormalizedRange range2 = new NormalizedRange(2, 6);

            range1.Should().NotBe(range2);
        }

        [TestMethod]
        public void IsEmpty_EmptyRange_ReturnsTrue()
        {
            NormalizedRange range = new NormalizedRange(1, 1);
            range.IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void IsEmpty_NotEmptyRange_ReturnsFalse()
        {
            NormalizedRange range = new NormalizedRange(1, 5);
            range.IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(1)]
        [DataRow(2)]
        public void Contains_ValueInRange_ReturnsTrue(int value)
        {
            NormalizedRange range = new NormalizedRange(1, 5);
            range.Contains(value).Should().BeTrue();
        }

        [TestMethod]
        [DataRow(6)]
        [DataRow(0)]
        [DataRow(10)]
        [DataRow(-1)]
        public void Contains_ValueNotInRange_ReturnsFalse(int value)
        {
            NormalizedRange range = new NormalizedRange(1, 5);
            range.Contains(value).Should().BeFalse();
        }

        [TestMethod]
        public void AsRange()
        {
            NormalizedRange range = new NormalizedRange(1, 5);
            range.ToString().Should().Be("1..5");
        }

        [TestMethod]
        public void ImplicitOperator()
        {
            NormalizedRange range = new NormalizedRange(1, 5);
            Range convertedRange = range;
            convertedRange.ToString().Should().Be("1..5");
        }

        [TestMethod]
        public void FromValues_AssignsValidStart()
        {
            NormalizedRange range = NormalizedRange.FromValues(5, 1);
            range.Start.Should().Be(1);
            range.End.Should().Be(5);
        }

        [TestMethod]
        public void CreateFrom()
        {
            Range range = new Range(1, 5);
            int[] values = [1, 2, 3, 4, 5];
            NormalizedRange normalizedRange = NormalizedRange.CreateFrom(range, values);
            normalizedRange.Start.Should().Be(1);
            normalizedRange.End.Should().Be(5);
        }

        [TestMethod]
        public void Empty_HasNoLenght()
        {
            NormalizedRange.Empty.Lenght.Should().Be(0);
        }

        [TestMethod]
        public void Shift_SpecifiedAmount_ReturnsShiftedRange()
        {
            NormalizedRange range = new NormalizedRange(5, 10);
            NormalizedRange shiftedRange = range.Shift(3);
            shiftedRange.Start.Should().Be(8);
            shiftedRange.End.Should().Be(13);
        }

        [TestMethod]
        public void Union_OverlappingRanges_ReturnsUnionRange()
        {
            NormalizedRange range1 = new NormalizedRange(5, 10);
            NormalizedRange range2 = new NormalizedRange(8, 15);
            NormalizedRange unionRange = range1.Union(range2);
            unionRange.Start.Should().Be(5);
            unionRange.End.Should().Be(15);
        }

        [TestMethod]
        public void Union_NonOverlappingRanges_ReturnsCombinedRange()
        {
            NormalizedRange range1 = new NormalizedRange(5, 10);
            NormalizedRange range2 = new NormalizedRange(15, 20);
            NormalizedRange unionRange = range1.Union(range2);
            unionRange.Start.Should().Be(5);
            unionRange.End.Should().Be(20);
        }

        [TestMethod]
        public void DistanceTo_OverlappingRanges_ReturnsZero()
        {
            NormalizedRange range1 = new NormalizedRange(5, 10);
            NormalizedRange range2 = new NormalizedRange(8, 12);
            int distance = range1.DistanceTo(range2);
            distance.Should().Be(0);
        }

        [TestMethod]
        public void DistanceTo_NonOverlappingRanges_ReturnsPositiveGap()
        {
            NormalizedRange range1 = new NormalizedRange(5, 10);
            NormalizedRange range2 = new NormalizedRange(15, 20);
            int distance = range1.DistanceTo(range2);
            distance.Should().Be(5);
        }

        [TestMethod]
        public void Clamp_ValueBelowRange_ReturnsStart()
        {
            NormalizedRange range = new NormalizedRange(5, 10);
            int clampedValue = range.Clamp(3);
            clampedValue.Should().Be(5);
        }

        [TestMethod]
        public void Clamp_ValueAboveRange_ReturnsEndMinusOne()
        {
            NormalizedRange range = new NormalizedRange(5, 10);
            int clampedValue = range.Clamp(12);
            clampedValue.Should().Be(9);
        }

        [TestMethod]
        public void ContainsRange_RangeFullyContained_ReturnsTrue()
        {
            NormalizedRange outerRange = new NormalizedRange(5, 15);
            NormalizedRange innerRange = new NormalizedRange(8, 10);
            outerRange.ContainsRange(innerRange).Should().BeTrue();
        }

        [TestMethod]
        public void ContainsRange_RangeNotFullyContained_ReturnsFalse()
        {
            NormalizedRange outerRange = new NormalizedRange(5, 15);
            NormalizedRange innerRange = new NormalizedRange(8, 16);
            outerRange.ContainsRange(innerRange).Should().BeFalse();
        }

        [TestMethod]
        public void Intersect_RangesOverlap_ReturnsIntersection()
        {
            NormalizedRange range1 = new NormalizedRange(5, 10);
            NormalizedRange range2 = new NormalizedRange(8, 12);
            NormalizedRange intersection = range1.Intersect(range2);
            intersection.Start.Should().Be(8);
            intersection.End.Should().Be(10);
        }

        [TestMethod]
        public void Intersect_RangesDoNotOverlap_ReturnsEmpty()
        {
            NormalizedRange range1 = new NormalizedRange(5, 10);
            NormalizedRange range2 = new NormalizedRange(12, 15);
            NormalizedRange intersection = range1.Intersect(range2);
            intersection.Should().Be(NormalizedRange.Empty);
        }
    }
}
