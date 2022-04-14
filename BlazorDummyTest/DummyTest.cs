using Xunit;

namespace BlazorDummyTest
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var expected = "Blazor is nice";
            var actual = "Blazor is nice";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Test2()
        {
            var expected = 1;
            var actual = 2;

            Assert.NotEqual(expected, actual);
        }
    }
}