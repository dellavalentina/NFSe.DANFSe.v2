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

        [Theory]
        [InlineData("12345678000195", "12.345.678/0001-95")]
        [InlineData("12ABC34501DE35", "12.ABC.345/01DE-35")]
        [InlineData("12.abc.345/01de-35", "12.ABC.345/01DE-35")]
        [InlineData("12345678909", "123.456.789-09")]
        [InlineData("", "")]
        public void TestFormatDocumentoSuportaCnpjAlfanumericoECpf(string input, string expected)
        {
            var result = Formatters.FormatDocumento(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("0", "PIS/COFINS/CSLL - Não Retidos")]
        [InlineData("3", "PIS/COFINS/CSLL - Retidos")]
        [InlineData("4", "PIS/COFINS Retidos, CSLL Não Retido")]
        [InlineData("9", "COFINS Não Retido, PIS/CSLL Retidos")]
        public void TestFormatRetPisCofinsNovosCodigos(string input, string expected)
        {
            var result = Formatters.FormatRetPisCofins(input);
            Assert.Equal(expected, result);
        }
    }
}
