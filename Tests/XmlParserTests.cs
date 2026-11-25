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
    
        /// <summary>
        /// Os "Totais Aproximados dos Tributos" (Lei 12.741/2012) exigidos pela NT 008 vêm do grupo totTrib —
        /// vTotTribFed/Est/Mun — e não das retenções nem do ISS. A amostra usa valores DISTINTOS de propósito:
        /// em danfse-terceiros.xml o vTotTribMun coincide com o vISSQN (34,75), o que torna a diferença
        /// invisível.
        /// </summary>
        [Fact]
        public void TestParseTotaisAproximadosTributos()
        {
            string xmlPath = Path.Combine(SamplesPath, "danfse-tottrib-distinto.xml");
            Assert.True(File.Exists(xmlPath), $"O arquivo XML de teste não foi encontrado em: {xmlPath}");

            DanfseModel model = DanfseXmlParser.Parse(File.ReadAllText(xmlPath));

            Assert.Equal("100.88", model.Valores.VTotTribFed);
            Assert.Equal("0.00", model.Valores.VTotTribEst);
            Assert.Equal("22.73", model.Valores.VTotTribMun);
            Assert.Equal("15.00", model.Valores.VIssqn); // o ISS é outra grandeza
        }

        [Fact]
        public void TestParseTotaisAproximadosTributosPercentuais()
        {
            string xmlPath = Path.Combine(SamplesPath, "danfse-normal.xml");
            Assert.True(File.Exists(xmlPath));

            DanfseModel model = DanfseXmlParser.Parse(File.ReadAllText(xmlPath));

            Assert.Equal("0.00", model.Valores.PTotTribFed);
            Assert.Equal("0.00", model.Valores.PTotTribEst);
            Assert.Equal("5.00", model.Valores.PTotTribMun);
        }

        [Fact]
        public void TestParseLeiauteSefinNacional1_6()
        {
            const string xmlSefin16 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<NFSe versao=""1.01"" xmlns=""http://www.sped.fazenda.gov.br/nfse"">
  <infNFSe Id=""NFS32053092212ABC34501DE35000000001842426111234567890"">
    <xLocEmi>Vitória</xLocEmi>
    <xLocPrestacao>Vitória</xLocPrestacao>
    <nNFSe>18424</nNFSe>
    <cLocIncid>3205309</cLocIncid>
    <xLocIncid>Vitória</xLocIncid>
    <xTribNac>Serviços de registros públicos, cartorários e notariais.</xTribNac>
    <verAplic>SefinNacional_1.6.0</verAplic>
    <ambGer>2</ambGer>
    <tpEmis>1</tpEmis>
    <cStat>100</cStat>
    <dhProc>2026-11-25T10:31:46-03:00</dhProc>
    <nDFSe>18424</nDFSe>
    <emit>
      <CNPJ>12ABC34501DE35</CNPJ>
      <xNome>CARTORIO DUMMY LTDA</xNome>
      <enderNac>
        <xLgr>RUA TESTE</xLgr>
        <nro>100</nro>
        <xBairro>CENTRO</xBairro>
        <cMun>3205309</cMun>
        <UF>ES</UF>
        <CEP>29010000</CEP>
      </enderNac>
    </emit>
    <valores>
      <vCalcAjusteBCISSQN>15.50</vCalcAjusteBCISSQN>
      <vBC>84.50</vBC>
      <pAliqAplic>5.00</pAliqAplic>
      <vISSQN>4.23</vISSQN>
      <vLiq>100.00</vLiq>
    </valores>
    <IBSCBS>
      <cLocalidadeIncid>3205309</cLocalidadeIncid>
      <xLocalidadeIncid>Vitória</xLocalidadeIncid>
      <valores>
        <vBC>80.27</vBC>
        <vCalcAjusteBCLocImoveis>5.00</vCalcAjusteBCLocImoveis>
        <vCalcAjusteBCIBSCBS>10.50</vCalcAjusteBCIBSCBS>
        <uf>
          <pIBSUF>0.10</pIBSUF>
          <pAliqEfetUF>0.10</pAliqEfetUF>
        </uf>
        <mun>
          <pIBSMun>0.00</pIBSMun>
          <pAliqEfetMun>0.00</pAliqEfetMun>
        </mun>
        <fed>
          <pCBS>0.90</pCBS>
          <pAliqEfetCBS>0.90</pAliqEfetCBS>
        </fed>
      </valores>
      <totCIBS>
        <vTotNF>100.00</vTotNF>
        <gIBS>
          <vIBSTot>0.08</vIBSTot>
          <gIBSUFTot><vIBSUF>0.08</vIBSUF></gIBSUFTot>
          <gIBSMunTot><vIBSMun>0.00</vIBSMun></gIBSMunTot>
        </gIBS>
        <gCBS>
          <vCBS>0.72</vCBS>
        </gCBS>
      </totCIBS>
    </IBSCBS>
    <DPS versao=""1.01"">
      <infDPS Id=""DPS3205309212ABC34501DE3500001000000000018275"">
        <tpAmb>2</tpAmb>
        <dhEmi>2026-11-25T10:30:00-03:00</dhEmi>
        <verAplic>Qualidata_1.0</verAplic>
        <serie>00001</serie>
        <nDPS>18275</nDPS>
        <dCompet>2026-11-25</dCompet>
        <tpEmit>1</tpEmit>
        <finNFSe>0</finNFSe>
        <cLocEmi>3205309</cLocEmi>
        <prest>
          <CNPJ>12ABC34501DE35</CNPJ>
          <regTrib>
            <opSimpNac>1</opSimpNac>
            <regEspTrib>4</regEspTrib>
          </regTrib>
        </prest>
        <serv>
          <locPrest><cLocPrestacao>3205309</cLocPrestacao></locPrest>
          <cServ>
            <cTribNac>210101</cTribNac>
            <cNBS>123012100</cNBS>
            <xDescServ>Reconhecimento de firma</xDescServ>
          </cServ>
        </serv>
        <valores>
          <vServPrest><vServ>100.00</vServ></vServPrest>
          <trib>
            <tribMun><tribISSQN>1</tribISSQN><tpRetISSQN>1</tpRetISSQN></tribMun>
            <totTrib><indTotTrib>0</indTotTrib></totTrib>
          </trib>
        </valores>
        <IBSCBS>
          <indFinal>0</indFinal>
          <cIndOp>010101</cIndOp>
          <indDest>1</indDest>
          <dest>
            <CNPJ>98XYZ7650001AA</CNPJ>
            <xNome>DESTINATARIO DUMMY S/A</xNome>
          </dest>
          <valores>
            <trib>
              <CST>000</CST>
              <cClassTrib>000001</cClassTrib>
            </trib>
          </valores>
        </IBSCBS>
      </infDPS>
    </DPS>
  </infNFSe>
</NFSe>";

            DanfseModel model = DanfseXmlParser.Parse(xmlSefin16);

            Assert.Equal("SefinNacional_1.6.0", model.VerAplic);
            Assert.Equal("12ABC34501DE35", model.Emitente.Documento);
            Assert.Equal("15.50", model.Valores.VCalcDR);
            Assert.Equal("0", model.IbsCbs.FinNFSe);
            Assert.Equal("000", model.IbsCbs.Cst);
            Assert.Equal("000001", model.IbsCbs.CClassTrib);
            Assert.Equal("5.00", model.IbsCbs.VCalcAjusteBCLocImoveis);
            Assert.Equal("10.50", model.IbsCbs.VCalcAjusteBCIBSCBS);
            Assert.Equal("98XYZ7650001AA", model.Destinatario.Documento);
            Assert.Equal("DESTINATARIO DUMMY S/A", model.Destinatario.Nome);
        }
    }
}
