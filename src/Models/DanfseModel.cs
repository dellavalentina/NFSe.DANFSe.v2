using System;

namespace NFSe.DANFSe.v2.Models
{
    public class DanfseModel
    {
        // Status & Metadata
        public string Id { get; set; } = string.Empty; // Chave de acesso (e.g. "NFS3205309...")
        public string NNFSe { get; set; } = string.Empty; // Número da NFS-e
        public string DhProc { get; set; } = string.Empty; // Data/Hora Processamento
        public string TpAmb { get; set; } = "1"; // 1 = Produção, 2 = Homologação
        public string AmbGer { get; set; } = string.Empty;
        public string Versao { get; set; } = "1.01"; // Versão da NFS-e (1.00 ou 1.01)
        public string CLocIncid { get; set; } = string.Empty;
        public string XLocIncid { get; set; } = string.Empty;
        public string XTribNac { get; set; } = string.Empty;
        public string XNBS { get; set; } = string.Empty;

        // Status Event Flags
        public bool IsCancelled { get; set; }
        public bool IsSubstituted { get; set; }

        // Emitente
        public Party Emitente { get; set; } = new Party();

        // Tomador
        public Party Tomador { get; set; } = new Party();

        // Intermediário
        public Party Intermediario { get; set; } = new Party();

        // Destinatário (específico para IBS/CBS se houver)
        public Party Destinatario { get; set; } = new Party();

        // Prestador (específico dos dados da DPS)
        public PrestadorData Prestador { get; set; } = new PrestadorData();

        // Valores Gerais
        public ValoresData Valores { get; set; } = new ValoresData();

        // IBS / CBS (Reforma Tributária)
        public IbsCbsData IbsCbs { get; set; } = new IbsCbsData();

        // DPS (Declaração de Prestação de Serviços)
        public DpsData Dps { get; set; } = new DpsData();

        // Serviço Prestado
        public ServicoData Servico { get; set; } = new ServicoData();
    }

    public class Party
    {
        public string Documento { get; set; } = string.Empty; // CNPJ ou CPF
        public string IM { get; set; } = string.Empty; // Inscrição Municipal
        public string Nome { get; set; } = string.Empty; // Nome / Razão Social
        public string Fone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Endereco Endereco { get; set; } = new Endereco();
        public bool Identificado { get; set; } = true;
    }

    public class Endereco
    {
        public string Logradouro { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Complemento { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string CMun { get; set; } = string.Empty; // Código IBGE
        public string XMun { get; set; } = string.Empty; // Nome do Município
        public string UF { get; set; } = string.Empty;
        public string CEP { get; set; } = string.Empty;
    }

    public class PrestadorData
    {
        public string IM { get; set; } = string.Empty;
        public string Fone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string OpSimpNac { get; set; } = string.Empty; // 1=Não Optante, 2=Optante-MEI, 3=Optante-ME/EPP
        public string RegApTribSN { get; set; } = string.Empty; // 1=ME/EPP, 2=MEI
        public string RegEspTrib { get; set; } = string.Empty; // Regime Especial de Tributação
    }

    public class ValoresData
    {
        public string VLiq { get; set; } = string.Empty;
        public string VTotalRet { get; set; } = string.Empty;
        public string TpBM { get; set; } = string.Empty; // Benefício Municipal
        public string VCalcBM { get; set; } = string.Empty;
        public string VCalcDR { get; set; } = string.Empty; // Deduções/Reduções
        public string VBC { get; set; } = string.Empty; // Base de cálculo ISSQN
        public string PAliqAplic { get; set; } = string.Empty; // Alíquota ISSQN
        public string VIssqn { get; set; } = string.Empty; // Valor do ISSQN
    }

    public class IbsCbsData
    {
        public string CLocalidadeIncid { get; set; } = string.Empty;
        public string XLocalidadeIncid { get; set; } = string.Empty;
        public string CIndOp { get; set; } = string.Empty; // Indicador de operação

        // Valores IBS/CBS
        public string VBC { get; set; } = string.Empty;
        public string VCalcReeRepRes { get; set; } = string.Empty;

        // Alíquotas e Valores
        public UfTax Uf { get; set; } = new UfTax();
        public MunTax Mun { get; set; } = new MunTax();
        public FedTax Fed { get; set; } = new FedTax();

        // Totais IBS/CBS
        public string VTotNF { get; set; } = string.Empty;
        public string VIbsTot { get; set; } = string.Empty;
        public string VIbsUf { get; set; } = string.Empty;
        public string VIbsMun { get; set; } = string.Empty;
        public string VCbs { get; set; } = string.Empty;
    }

    public class UfTax
    {
        public string PRedAliqUF { get; set; } = string.Empty;
        public string PIbsUF { get; set; } = string.Empty;
        public string PAliqEfetUF { get; set; } = string.Empty;
    }

    public class MunTax
    {
        public string PRedAliqMun { get; set; } = string.Empty;
        public string PIbsMun { get; set; } = string.Empty;
        public string PAliqEfetMun { get; set; } = string.Empty;
    }

    public class FedTax
    {
        public string PRedAliqCBS { get; set; } = string.Empty;
        public string PCbs { get; set; } = string.Empty;
        public string PAliqEfetCBS { get; set; } = string.Empty;
    }

    public class DpsData
    {
        public string Id { get; set; } = string.Empty;
        public string DhEmi { get; set; } = string.Empty;
        public string Serie { get; set; } = string.Empty;
        public string NDPS { get; set; } = string.Empty;
        public string DCompet { get; set; } = string.Empty;
    }

    public class ServicoData
    {
        public string CLocPrestacao { get; set; } = string.Empty;
        public string XMunPrestacao { get; set; } = string.Empty;
        public string CPaisPrestacao { get; set; } = string.Empty;
        public string CepPrestacao { get; set; } = string.Empty;

        public string CObra { get; set; } = string.Empty;
        public string Art { get; set; } = string.Empty;

        public string CTribNac { get; set; } = string.Empty;
        public string CTribMun { get; set; } = string.Empty;
        public string CNbs { get; set; } = string.Empty;
        public string XDescServ { get; set; } = string.Empty;
        public string XOutInf { get; set; } = string.Empty; // Informações complementares

        // ISSQN específicos (dentro de Valores.Trib)
        public string TribIssqn { get; set; } = string.Empty; // Tipo de tributação ISSQN (1=Trib, 2=Export, 3=Não Incid, 4=Imune)
        public string TpRetIssqn { get; set; } = string.Empty; // 1=Não retido, 2=Retido tomador, 3=Retido intermediário
        public string TpSusp { get; set; } = string.Empty; // Exigibilidade suspensa
        public string NProcesso { get; set; } = string.Empty;
        public string NBM { get; set; } = string.Empty; // Número do Benefício Municipal

        // Tributos Federais (Valores.Trib.TribFed)
        public string VRetIrrf { get; set; } = string.Empty;
        public string VRetCp { get; set; } = string.Empty;
        public string VRetCsll { get; set; } = string.Empty;
        public string VPis { get; set; } = string.Empty;
        public string VCofins { get; set; } = string.Empty;
        public string TpRetPisCofins { get; set; } = string.Empty;
    }
}
