using RetriX.Shared.ExtensionMethods;
using Xunit;

namespace RetriX.Shared.Test.ExtensionMethods
{
    public class StringHasherTest
    {
        private const string TestString = nameof(TestString);

        [Fact]
        public void MD5Works()
        {
            var expectedvalue = "5b56f40f8828701f97fa4511ddcd25fb";
            Assert.Equal(expectedvalue, TestString.MD5());
            Assert.Equal(expectedvalue, TestString.MD5());
        }

        [Fact]
        public void SHA1Works()
        {
            var expectedvalue = "d598b03bee8866ae03b54cb6912efdfef107fd6d";
            Assert.Equal(expectedvalue, TestString.SHA1());
            Assert.Equal(expectedvalue, TestString.SHA1());
        }
    }
}
