using ControlPad.Utils;

namespace ControlPad.Tests
{
    public class IdAllocatorTests
    {
        [Fact]
        public void GetFreeId_ReturnsZero_WhenCollectionEmpty()
        {
            var result = IdAllocator.GetFreeId(Array.Empty<int>(), x => x);

            Assert.Equal(0, result);
        }

        [Fact]
        public void GetFreeId_ReturnsOne_WhenZeroExists()
        {
            var result = IdAllocator.GetFreeId(new[] { 0 }, x => x);

            Assert.Equal(1, result);
        }

        [Fact]
        public void GetFreeId_FindsGapInSequence()
        {
            var result = IdAllocator.GetFreeId(new[] { 0, 2, 3 }, x => x);

            Assert.Equal(1, result);
        }

        [Fact]
        public void GetFreeId_IgnoresNegativeIds_AndReturnsZeroWhenMissing()
        {
            var result = IdAllocator.GetFreeId(new[] { -10, -1, 4, 7 }, x => x);

            Assert.Equal(0, result);
        }

        [Fact]
        public void GetFreeId_HandlesLargeNonSequentialNumbers()
        {
            var result = IdAllocator.GetFreeId(new[] { 0, 1, 2, 1000, 5000 }, x => x);

            Assert.Equal(3, result);
        }
    }
}
