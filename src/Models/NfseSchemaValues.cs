using System;

namespace NFSe.DANFSe.v2.Models
{
    public static class NfseSchemaValues
    {
        // 1. Tipo de Ambiente (tpAmb)
        public const string TpAmbProducao = "1";
        public const string TpAmbHomologacao = "2";

        // 2. Ambiente Gerador (ambGer)
        public const string AmbGerMunicipio = "1";
        public const string AmbGerSefinNacional = "2";

        // 3. Tipo de Tributação do ISSQN (tribISSQN)
        public const string TribIssqnTributavel = "1";
        public const string TribIssqnExportacao = "2";
        public const string TribIssqnNaoIncidencia = "3";
        public const string TribIssqnImunidade = "4";

        // 4. Retenção do ISSQN (tpRetISSQN)
        public const string RetIssNaoRetido = "1";
        public const string RetIssRetidoTomador = "2";
        public const string RetIssRetidoIntermediario = "3";

        // 5. Suspensão da Exigibilidade (tpSusp)
        public const string ExigSuspNao = "0";
        public const string ExigSuspJudicial = "1";
        public const string ExigSuspAdministrativo = "2";

        // 6. Opção pelo Simples Nacional (opSimpNac)
        public const string OpSimplesNaoOptante = "1";
        public const string OpSimplesOptanteMei = "2";
        public const string OpSimplesOptanteMeEpp = "3";

        // 7. Regime de Apuração no Simples Nacional (regApTribSN)
        public const string RegApTribSnMeEpp = "1";
        public const string RegApTribSnMei = "2";

        // 8. Identificadores de Eventos
        public const string EventoCancelamento = "e101101";
        public const string EventoSubstituicao1 = "e101102";
        public const string EventoSubstituicao2 = "e101103";
    }
}
