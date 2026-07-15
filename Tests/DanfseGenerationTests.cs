using System;
using System.IO;
using Xunit;
using NFSe.DANFSe.v2.Parser;
using NFSe.DANFSe.v2.Models;
using NFSe.DANFSe.v2.Rendering;

namespace NFSe.DANFSe.v2.Tests
{
    public class DanfseGenerationTests
    {
        private static readonly string SamplesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples");
        private static readonly string OutputPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Output"));
        private static readonly string AlternativeLogoPath = Path.Combine(SamplesPath, "logo-alternative.png");

        public DanfseGenerationTests()
        {
            if (!Directory.Exists(OutputPath))
            {
                Directory.CreateDirectory(OutputPath);
            }
        }

        private void SafeWriteAllBytes(string path, byte[] bytes)
        {
            try
            {
                File.WriteAllBytes(path, bytes);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Aviso: Não foi possível salvar o PDF em '{path}' porque o arquivo está em uso. Erro: {ex.Message}");
            }
        }

        private byte[]? LoadLogoBytes()
        {
            if (File.Exists(AlternativeLogoPath))
            {
                return File.ReadAllBytes(AlternativeLogoPath);
            }
            return null;
        }

        [Fact]
        public void TestGenerateDanfseNormal()
        {
            string xmlPath = Path.Combine(SamplesPath, "danfse-normal.xml");
            Assert.True(File.Exists(xmlPath));

            string xmlContent = File.ReadAllText(xmlPath);
            DanfseModel model = DanfseXmlParser.Parse(xmlContent);

            byte[]? logoBytes = null;

            byte[] pdfBytes = DanfsePdfRenderer.GeneratePdf(model, logoBytes, forceTestWatermark: true);
            Assert.NotEmpty(pdfBytes);

            string pdfOutputPath = Path.Combine(OutputPath, $"{model.NNFSe}-danfse.pdf");
            SafeWriteAllBytes(pdfOutputPath, pdfBytes);
            
            Assert.True(File.Exists(pdfOutputPath));
            Console.WriteLine($"PDF normal gerado com sucesso em: {pdfOutputPath}");
        }

        [Fact]
        public void TestGenerateDanfseAlternativeLogo()
        {
            string xmlPath = Path.Combine(SamplesPath, "danfse-normal.xml");
            Assert.True(File.Exists(xmlPath));

            string xmlContent = File.ReadAllText(xmlPath);
            DanfseModel model = DanfseXmlParser.Parse(xmlContent);

            byte[]? logoBytes = LoadLogoBytes();

            byte[] pdfBytes = DanfsePdfRenderer.GeneratePdf(model, logoBytes, forceTestWatermark: true);
            Assert.NotEmpty(pdfBytes);

            string pdfOutputPath = Path.Combine(OutputPath, $"{model.NNFSe}-danfse-alternative-logo.pdf");
            SafeWriteAllBytes(pdfOutputPath, pdfBytes);
            
            Assert.True(File.Exists(pdfOutputPath));
            Console.WriteLine($"PDF normal gerado com sucesso em: {pdfOutputPath}");
        }

        [Fact]
        public void TestGenerateDanfseHomologationAndCancelled()
        {
            string xmlPath = Path.Combine(SamplesPath, "danfse-normal.xml");
            string cancelXmlPath = Path.Combine(SamplesPath, "danfse-cancelamento.xml");

            Assert.True(File.Exists(xmlPath));
            Assert.True(File.Exists(cancelXmlPath));

            string xmlContent = File.ReadAllText(xmlPath);
            string cancelXmlContent = File.ReadAllText(cancelXmlPath);

            DanfseModel model = DanfseXmlParser.Parse(xmlContent);
            DanfseXmlParser.ApplyEvent(model, cancelXmlContent);

            // Força ambiente de Homologação para testar o cabeçalho vermelho de validade
            model.TpAmb = "2"; 

            byte[]? logoBytes = null;

            byte[] pdfBytes = DanfsePdfRenderer.GeneratePdf(model, logoBytes);
            Assert.NotEmpty(pdfBytes);

            string pdfOutputPath = Path.Combine(OutputPath, $"{model.NNFSe}-danfse-cancelada-homologacao.pdf");
            SafeWriteAllBytes(pdfOutputPath, pdfBytes);

            Assert.True(File.Exists(pdfOutputPath));
            Console.WriteLine($"PDF de cancelamento e homologação gerado em: {pdfOutputPath}");
        }

        [Fact]
        public void TestGenerateDanfseSubstituted()
        {
            string xmlPath = Path.Combine(SamplesPath, "danfse-substituida.xml");
            Assert.True(File.Exists(xmlPath));

            string xmlContent = File.ReadAllText(xmlPath);
            DanfseModel model = DanfseXmlParser.Parse(xmlContent);

            // Força a flag de substituição para testar marca d'água substituição
            model.IsSubstituted = true;

            byte[]? logoBytes = null;

            byte[] pdfBytes = DanfsePdfRenderer.GeneratePdf(model, logoBytes);
            Assert.NotEmpty(pdfBytes);

            string pdfOutputPath = Path.Combine(OutputPath, $"{model.NNFSe}-danfse-substituida.pdf");
            SafeWriteAllBytes(pdfOutputPath, pdfBytes);

            Assert.True(File.Exists(pdfOutputPath));
            Console.WriteLine($"PDF de nota substituída gerado em: {pdfOutputPath}");
        }

        [Fact]
        public void TestGenerateDanfseTerceiros()
        {
            string xmlPath = Path.Combine(SamplesPath, "danfse-terceiros.xml");
            Assert.True(File.Exists(xmlPath));

            string xmlContent = File.ReadAllText(xmlPath);
            DanfseModel model = DanfseXmlParser.Parse(xmlContent);

            byte[]? logoBytes = null;

            byte[] pdfBytes = DanfsePdfRenderer.GeneratePdf(model, logoBytes);
            Assert.NotEmpty(pdfBytes);

            string pdfOutputPath = Path.Combine(OutputPath, $"{model.NNFSe}-danfse-terceiros.pdf");
            SafeWriteAllBytes(pdfOutputPath, pdfBytes);

            Assert.True(File.Exists(pdfOutputPath));
            Console.WriteLine($"PDF de nota terceiros gerado em: {pdfOutputPath}");
        }
    }
}
