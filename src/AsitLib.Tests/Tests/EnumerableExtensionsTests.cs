namespace AsitLib.Tests
{
    [TestClass]
    public class EnumerableExtensionsTests
    {
        [TestMethod]
        public void EndsWith_WhenSourceEndsWithSequence_ReturnsTrue()
        {
            int[] source = [1, 2, 3, 4, 5];
            int[] value = [4, 5];

            source.EndsWith(value).Should().BeTrue();
        }

        [TestMethod]
        public void EndsWith_WhenSourceDoesNotEndWithSequence_ReturnsFalse()
        {
            int[] source = [1, 2, 3, 4, 5];
            int[] value = [3, 4];

            source.EndsWith(value).Should().BeFalse();
        }

        [TestMethod]
        public void GetShallowCopy_ReturnsArrayWithSameElements()
        {
            int[] source = [1, 2, 3, 4, 5];

            source.GetShallowCopy().Should().Equal(source);
        }

        [TestMethod]
        public void SwitchIndexes_SwapsElementsAtSpecifiedIndexes()
        {
            int[] source = [1, 2, 3, 4, 5];

            source.SwitchIndexes(1, 3);

            source[1].Should().Be(4);
            source[3].Should().Be(2);
        }

        [TestMethod]
        public void SqueezeIndexes_MovesNullsToEndPreservingOrder()
        {
            int?[] source = [1, null, 2, null, 3];

            int?[] result = source.SqueezeIndexes();

            result.Should().Equal([1, 2, 3, null, null]);
        }

        [TestMethod]
        public void GetFirstIndexWhere_WhenMatchExists_ReturnsIndex()
        {
            int[] source = [1, 2, 3, 4, 5];

            int result = source.GetFirstIndexWhere(x => x == 3);

            result.Should().Be(2);
        }

        [TestMethod]
        public void GetFirstIndexWhere_WhenNoMatchExists_ThrowsEx()
        {
            int[] source = [1, 2, 3, 4, 5];

            Invoking(() => source.GetFirstIndexWhere(x => x == 6)).Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void ToJoinedString_JoinsValuesIncludingNulls()
        {
            int?[] source = [1, null, 3, 4];

            source.ToJoinedString(',').Should().Be("1,null,3,4");
        }
    }
}
