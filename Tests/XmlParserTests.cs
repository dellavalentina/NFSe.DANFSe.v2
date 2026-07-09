using System;
using System.IO;
using Xunit;
using NFSe.DANFSe.v2.Parser;
using NFSe.DANFSe.v2.Models;

namespace NFSe.DANFSe.v2.Tests
{
    public class XmlParserTests
    {
        private static readonly string SamplesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples");

        [Fact]
        public void TestParseNfseNormal()
        {
            string xmlPath = Path.Combine(SamplesPath, "danfse-normal.xml");
            Assert.True(File.Exists(xmlPath), $"O arquivo XML de teste não foi encontrado em: {xmlPath}");

            string xmlContent = File.ReadAllText(xmlPath);
            DanfseModel model = DanfseXmlParser.Parse(xmlContent);

            Assert.NotNull(model);
            Assert.Equal("18424", model.NNFSe);
            Assert.Equal("12345678909", model.Emitente.Documento); // CPF do emitente fictício
            Assert.Equal("PRESTADOR DE SERVICOS DUMMY LTDA", model.Emitente.Nome);
            Assert.Equal("1", model.TpAmb); // Produção
            Assert.False(model.IsCancelled);
            Assert.False(model.IsSubstituted);

            // Valores de ISS
            Assert.Equal("3.70", model.Valores.VIssqn);
            
            // IBS/CBS
            Assert.Equal("92.35", model.IbsCbs.VBC);
            Assert.Equal("0.09", model.IbsCbs.VIbsTot);
            Assert.Equal("0.83", model.IbsCbs.VCbs);
        }

        [Fact]
        public void TestParseNfseWithCancellationEvent()
        {
            string xmlPath = Path.Combine(SamplesPath, "danfse-normal.xml");
            string cancelXmlPath = Path.Combine(SamplesPath, "danfse-cancelamento.xml");

            Assert.True(File.Exists(xmlPath));
            Assert.True(File.Exists(cancelXmlPath));

            string xmlContent = File.ReadAllText(xmlPath);
            string cancelXmlContent = File.ReadAllText(cancelXmlPath);

            // Realiza o parse da nota
            DanfseModel model = DanfseXmlParser.Parse(xmlContent);
            Assert.False(model.IsCancelled);

            // Aplica o evento de cancelamento
            DanfseXmlParser.ApplyEvent(model, cancelXmlContent);
            Assert.True(model.IsCancelled);
            Assert.False(model.IsSubstituted);
        }
    }
}
