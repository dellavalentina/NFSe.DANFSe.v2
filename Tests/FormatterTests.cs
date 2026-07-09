using System;
using Xunit;
using NFSe.DANFSe.v2.Helpers;

namespace NFSe.DANFSe.v2.Tests
{
    public class FormatterTests
    {
        [Theory]
        [InlineData("2026-06-25", "25/06/2026")]
        [InlineData("2026-06-25-03:00", "25/06/2026")]
        [InlineData("2026-06-25Z", "25/06/2026")]
        [InlineData("", "-")]
        [InlineData(null, "-")]
        [InlineData("data-invalida", "data-invalida")]
        public void TestFormatDate(string input, string expected)
        {
            var result = Formatters.FormatDate(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("2026-06-25T19:36:59-03:00", "25/06/2026 19:36:59")]
        [InlineData("2026-06-25T14:16:56Z", "25/06/2026 14:16:56")]
        [InlineData("2026-06-25T12:00:00", "25/06/2026 12:00:00")]
        [InlineData("", "-")]
        [InlineData(null, "-")]
        [InlineData("data-hora-invalida", "data-hora-invalida")]
        public void TestFormatDateTime(string input, string expected)
        {
            var result = Formatters.FormatDateTime(input);
            Assert.Equal(expected, result);
        }
    }
}
