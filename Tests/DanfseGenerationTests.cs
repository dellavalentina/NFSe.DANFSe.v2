using System;
using System.IO;
using Xunit;
using NFSe.DANFSe.v2.Parser;
using NFSe.DANFSe.v2.Models;
using NFSe.DANFSe.v2.Rendering;
using NFSe.DANFSe.v2.Helpers;

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

        [Fact]
        public void TestFormattersTpEmitAndIbsCbsIndOp()
        {
            Assert.Equal("1 - Prestador", Formatters.FormatTpEmit("1"));
            Assert.Equal("2 - Tomador", Formatters.FormatTpEmit("2"));
            Assert.Equal("3 - Intermediário", Formatters.FormatTpEmit("3"));
            Assert.Equal("1 - Prestador", Formatters.FormatTpEmit(""));

            string formattedIndOp = Formatters.FormatIbsCbsIndOp("01", "3205309", "Vitória", "ES");
            Assert.Equal("01 / 3205309 / Vitória / ES", formattedIndOp);

            Assert.Equal("000 / 000001", Formatters.FormatCstCClassTrib("000", "000001"));
            Assert.Equal("-", Formatters.FormatCstCClassTrib("", ""));

            Assert.Equal("100 - NFS-e Gerada", Formatters.FormatSituacaoNfse("100"));
            Assert.Equal("101 - NFS-e de Substituição Gerada", Formatters.FormatSituacaoNfse("101"));
            Assert.Equal("102 - NFS-e de Decisão Judicial", Formatters.FormatSituacaoNfse("102"));
            Assert.Equal("103 - NFS-e Avulsa", Formatters.FormatSituacaoNfse("103"));
            Assert.Equal("107 - NFS-e MEI", Formatters.FormatSituacaoNfse("107"));

            Assert.Equal("0 - NFS-e regular", Formatters.FormatFinalidadeNfse("0"));
            Assert.Equal("", Formatters.FormatFinalidadeNfse(""));
        }

        [Fact]
        public void TestParseIbsCbsFromInfNfseAndInfDps()
        {
            string xmlPath = Path.Combine(SamplesPath, "danfse-normal.xml");
            string xmlContent = File.ReadAllText(xmlPath);
            DanfseModel model = DanfseXmlParser.Parse(xmlContent);

            // Verifica se a mesclagem de infNFSe/IBSCBS e infDPS/IBSCBS foi realizada corretamente
            Assert.Equal("3205309", model.IbsCbs.CLocalidadeIncid);
            Assert.Equal("Vitória", model.IbsCbs.XLocalidadeIncid);
            Assert.Equal("030101", model.IbsCbs.CIndOp);
            Assert.Equal("000", model.IbsCbs.Cst);
            Assert.Equal("000001", model.IbsCbs.CClassTrib);
        }

        [Fact]
        public void TestAutoFixInconsistentTotalFlag()
        {
            string xmlPath = Path.Combine(SamplesPath, "danfse-normal.xml");
            Assert.True(File.Exists(xmlPath));

            string xmlContent = File.ReadAllText(xmlPath);
            DanfseModel model = DanfseXmlParser.Parse(xmlContent);

            // Adiciona dados hipotéticos de IBS/CBS onde vTotNF no XML veio omitindo o imposto por fora (inconsistente)
            model.Valores.VLiq = "100.00";
            model.IbsCbs.VIbsTot = "0.10";
            model.IbsCbs.VCbs = "0.90";
            model.IbsCbs.VTotNF = "100.00"; // XML omitiu o IBS/CBS por fora (igual ao líquido)

            // Teste 1: Default (autoFix = false) -> Gera PDF sem alterar o XML
            byte[] pdfOriginal = DanfsePdfRenderer.GeneratePdf(model, autoFixInconsistentTotal: false);
            Assert.NotEmpty(pdfOriginal);
            string pathOriginal = Path.Combine(OutputPath, $"{model.NNFSe}-autofix-false-xml-original.pdf");
            SafeWriteAllBytes(pathOriginal, pdfOriginal);

            // Teste 2: autoFix = true -> Detecta omissão no XML e recalcula para 101.00 sem duplicidade
            byte[] pdfFixed = DanfsePdfRenderer.GeneratePdf(model, autoFixInconsistentTotal: true);
            Assert.NotEmpty(pdfFixed);
            string pathFixed = Path.Combine(OutputPath, $"{model.NNFSe}-autofix-true-recalculado.pdf");
            SafeWriteAllBytes(pathFixed, pdfFixed);

            // Teste 3: XML já correto (vTotNF = 101.00) com autoFix = true -> Mantém 101.00 (Zero Duplicidade!)
            model.IbsCbs.VTotNF = "101.00";
            byte[] pdfAlreadyCorrect = DanfsePdfRenderer.GeneratePdf(model, autoFixInconsistentTotal: true);
            Assert.NotEmpty(pdfAlreadyCorrect);
            string pathAlreadyCorrect = Path.Combine(OutputPath, $"{model.NNFSe}-autofix-true-xml-ja-correto.pdf");
            SafeWriteAllBytes(pathAlreadyCorrect, pdfAlreadyCorrect);

            Console.WriteLine($"PDFs de teste de autoFix gerados em:\n - {pathOriginal}\n - {pathFixed}\n - {pathAlreadyCorrect}");
        }

    }
}
