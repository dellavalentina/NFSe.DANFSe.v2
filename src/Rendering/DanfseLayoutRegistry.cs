using System;
using System.Collections.Generic;

namespace NFSe.DANFSe.v2.Rendering
{
    public enum LayoutAlignment
    {
        Left,
        Center,
        Right
    }

    public class FieldMetadata
    {
        public string Key { get; set; } = string.Empty;
        public double XOffset { get; set; }
        public double YOffset { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Label { get; set; } = string.Empty;
        public LayoutAlignment Alignment { get; set; } = LayoutAlignment.Left;
        public bool HasShading { get; set; }
        public int MaxLength { get; set; }
    }

    public static class DanfseLayoutRegistry
    {
        public static readonly Dictionary<string, FieldMetadata> Fields = new Dictionary<string, FieldMetadata>
        {
            // --- 1. CABEÇALHO ---
            { "Cabecalho.Titulo", new FieldMetadata { XOffset = 5.41, YOffset = 0.35, Width = 10.19, Height = 0.35, Alignment = LayoutAlignment.Center } },
            { "Cabecalho.Subtitulo", new FieldMetadata { XOffset = 5.41, YOffset = 0.65, Width = 10.19, Height = 0.35, Alignment = LayoutAlignment.Center } },
            { "Cabecalho.Homologacao", new FieldMetadata { XOffset = 5.41, YOffset = 0.95, Width = 10.19, Height = 0.35, Alignment = LayoutAlignment.Center } },

            // --- 2. CHAVE DE ACESSO E DADOS GERAIS ---
            { "Chave.Label", new FieldMetadata { XOffset = 0.40, YOffset = 1.55, Width = 15.00, Height = 0.30, Alignment = LayoutAlignment.Left } },
            { "Chave.Valor", new FieldMetadata { XOffset = 0.40, YOffset = 1.90, Width = 15.00, Height = 0.30, Alignment = LayoutAlignment.Left } },
            { "Nfse.NumeroLabel", new FieldMetadata { XOffset = 0.40, YOffset = 2.35, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left } },
            { "Nfse.NumeroValor", new FieldMetadata { XOffset = 0.40, YOffset = 2.65, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left } },
            { "Nfse.CompetenciaLabel", new FieldMetadata { XOffset = 5.41, YOffset = 2.35, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left } },
            { "Nfse.CompetenciaValor", new FieldMetadata { XOffset = 5.41, YOffset = 2.65, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left } },
            { "Nfse.EmissaoLabel", new FieldMetadata { XOffset = 10.51, YOffset = 2.35, Width = 5.00, Height = 0.25, Alignment = LayoutAlignment.Left } },
            { "Nfse.EmissaoValor", new FieldMetadata { XOffset = 10.51, YOffset = 2.65, Width = 5.00, Height = 0.25, Alignment = LayoutAlignment.Left } },
            
            { "Dps.NumeroLabel", new FieldMetadata { XOffset = 0.40, YOffset = 3.05, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left } },
            { "Dps.NumeroValor", new FieldMetadata { XOffset = 0.40, YOffset = 3.35, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left } },
            { "Dps.SerieLabel", new FieldMetadata { XOffset = 5.41, YOffset = 3.05, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left } },
            { "Dps.SerieValor", new FieldMetadata { XOffset = 5.41, YOffset = 3.35, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left } },
            { "Dps.EmissaoLabel", new FieldMetadata { XOffset = 10.51, YOffset = 3.05, Width = 5.00, Height = 0.25, Alignment = LayoutAlignment.Left } },
            { "Dps.EmissaoValor", new FieldMetadata { XOffset = 10.51, YOffset = 3.35, Width = 5.00, Height = 0.25, Alignment = LayoutAlignment.Left } },

            { "Emitente.BlocoLabel", new FieldMetadata { XOffset = 0.40, YOffset = 3.72, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left } },
            { "Situacao.BlocoLabel", new FieldMetadata { XOffset = 5.41, YOffset = 3.72, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left } },
            { "Situacao.Valor", new FieldMetadata { XOffset = 5.41, YOffset = 4.02, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left } },
            { "Finalidade.BlocoLabel", new FieldMetadata { XOffset = 10.51, YOffset = 3.72, Width = 5.00, Height = 0.25, Alignment = LayoutAlignment.Left } },
            { "Finalidade.Valor", new FieldMetadata { XOffset = 10.51, YOffset = 4.02, Width = 5.00, Height = 0.25, Alignment = LayoutAlignment.Left } },

            // --- 3. PRESTADOR ---
            { "Prestador.BlocoLabel", new FieldMetadata { XOffset = 0.40, YOffset = 4.41, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left } },
            { "Prestador.Documento", new FieldMetadata { XOffset = 5.41, YOffset = 4.41, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "CNPJ / CPF / NIF" } },
            { "Prestador.IM", new FieldMetadata { XOffset = 10.51, YOffset = 4.41, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Indicador Municipal (Inscrição)" } },
            { "Prestador.Telefone", new FieldMetadata { XOffset = 15.62, YOffset = 4.41, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Telefone" } },
            { "Prestador.Nome", new FieldMetadata { XOffset = 0.40, YOffset = 5.05, Width = 9.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Nome / Nome Empresarial" } },
            { "Prestador.Municipio", new FieldMetadata { XOffset = 10.51, YOffset = 5.05, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Município / Sigla UF" } },
            { "Prestador.IbgeCep", new FieldMetadata { XOffset = 15.62, YOffset = 5.05, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Código IBGE / CEP" } },
            { "Prestador.Endereco", new FieldMetadata { XOffset = 0.40, YOffset = 5.69, Width = 9.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Endereço" } },
            { "Prestador.Email", new FieldMetadata { XOffset = 10.51, YOffset = 5.69, Width = 9.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "E-mail" } },
            { "Prestador.OpSimpNac", new FieldMetadata { XOffset = 0.40, YOffset = 6.35, Width = 9.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Simples Nacional na Data de Competência" } },
            { "Prestador.RegApTribSN", new FieldMetadata { XOffset = 10.51, YOffset = 6.35, Width = 9.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Regime de Apuração Tributária pelo SN" } },

            // --- 4. TOMADOR (Relativos ao currentY do bloco) ---
            { "Tomador.BlocoLabel", new FieldMetadata { XOffset = 0.40, YOffset = 0.07, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left } },
            { "Tomador.Documento", new FieldMetadata { XOffset = 5.41, YOffset = 0.07, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "CNPJ / CPF / NIF" } },
            { "Tomador.IM", new FieldMetadata { XOffset = 10.51, YOffset = 0.07, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Indicador Municipal (Inscrição)" } },
            { "Tomador.Telefone", new FieldMetadata { XOffset = 15.62, YOffset = 0.07, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Telefone" } },
            { "Tomador.Nome", new FieldMetadata { XOffset = 0.40, YOffset = 0.71, Width = 9.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Nome / Nome Empresarial" } },
            { "Tomador.Endereco", new FieldMetadata { XOffset = 0.40, YOffset = 1.37, Width = 9.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Endereço" } },
            { "Tomador.Municipio", new FieldMetadata { XOffset = 10.51, YOffset = 0.71, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Município / Sigla UF" } },
            { "Tomador.IbgeCep", new FieldMetadata { XOffset = 15.62, YOffset = 0.71, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Código IBGE / CEP" } },
            { "Tomador.Email", new FieldMetadata { XOffset = 10.51, YOffset = 1.37, Width = 9.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "E-mail" } },

            // --- Destinatário ---
            { "Destinatario.Documento", new FieldMetadata { XOffset = 5.41, YOffset = 0.07, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "CNPJ / CPF / NIF" } },
            { "Destinatario.Telefone", new FieldMetadata { XOffset = 15.62, YOffset = 0.07, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Telefone" } },
            { "Destinatario.Nome", new FieldMetadata { XOffset = 0.40, YOffset = 0.71, Width = 9.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Nome / Nome Empresarial" } },
            { "Destinatario.Municipio", new FieldMetadata { XOffset = 10.51, YOffset = 0.71, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Município / Sigla UF" } },
            { "Destinatario.IbgeCep", new FieldMetadata { XOffset = 15.62, YOffset = 0.71, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Código IBGE / CEP" } },
            { "Destinatario.Endereco", new FieldMetadata { XOffset = 0.40, YOffset = 1.37, Width = 9.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Endereço" } },
            { "Destinatario.Email", new FieldMetadata { XOffset = 10.51, YOffset = 1.37, Width = 9.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "E-mail" } },

            // --- 5. INTERMEDIÁRIO (Relativos) ---
            { "Interm.Documento", new FieldMetadata { XOffset = 5.41, YOffset = 0.07, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "CNPJ / CPF / NIF" } },
            { "Interm.IM", new FieldMetadata { XOffset = 10.51, YOffset = 0.07, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Indicador Municipal (Inscrição)" } },
            { "Interm.Telefone", new FieldMetadata { XOffset = 15.62, YOffset = 0.07, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Telefone" } },
            { "Interm.Nome", new FieldMetadata { XOffset = 0.40, YOffset = 0.71, Width = 9.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Nome / Nome Empresarial" } },
            { "Interm.Municipio", new FieldMetadata { XOffset = 10.51, YOffset = 0.71, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Município / Sigla UF" } },
            { "Interm.IbgeCep", new FieldMetadata { XOffset = 15.62, YOffset = 0.71, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Código IBGE / CEP" } },
            { "Interm.Endereco", new FieldMetadata { XOffset = 0.40, YOffset = 1.37, Width = 9.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Endereço" } },
            { "Interm.Email", new FieldMetadata { XOffset = 10.51, YOffset = 1.37, Width = 9.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "E-mail" } },

            // --- 6. SERVIÇO PRESTADO ---
            { "Servico.Tributacao", new FieldMetadata { XOffset = 5.41, YOffset = 0.07, Width = 5.00, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Código de Tributação Nacional / Municipal" } },
            { "Servico.NBS", new FieldMetadata { XOffset = 10.51, YOffset = 0.07, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Código da NBS" } },
            { "Servico.LocalPrestacao", new FieldMetadata { XOffset = 15.62, YOffset = 0.07, Width = 5.00, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Local da Prestação / Sigla UF / País" } },

            // --- 8. TRIBUTAÇÃO MUNICIPAL (ISSQN) ---
            { "Trib.TipoTributacao", new FieldMetadata { XOffset = 5.41, YOffset = 0.07, Width = 5.00, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Tipo de Tributação do ISSQN" } },
            { "Trib.Incidencia", new FieldMetadata { XOffset = 10.51, YOffset = 0.07, Width = 9.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Município / Sigla UF / País da Incidência do ISSQN" } },
            { "Trib.RegimeEspecial", new FieldMetadata { XOffset = 0.40, YOffset = 0.72, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Regime Especial de Tributação do ISSQN" } },
            { "Trib.TipoImunidade", new FieldMetadata { XOffset = 5.41, YOffset = 0.72, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Tipo de Imunidade do ISSQN" } },
            { "Trib.Suspensao", new FieldMetadata { XOffset = 10.51, YOffset = 0.72, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Suspensão da Exigibilidade do ISSQN" } },
            { "Trib.NumeroProcesso", new FieldMetadata { XOffset = 15.62, YOffset = 0.72, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Número Processo Suspensão" } },
            { "Trib.BeneficioMunicipal", new FieldMetadata { XOffset = 0.40, YOffset = 1.37, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Benefício Municipal", MaxLength = 40 } },
            { "Trib.CalculoBM", new FieldMetadata { XOffset = 5.41, YOffset = 1.37, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Cálculo do BM" } },
            { "Trib.Deducoes", new FieldMetadata { XOffset = 10.51, YOffset = 1.37, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Total Deduções/Reduções" } },
            { "Trib.DescontoIncondicionado", new FieldMetadata { XOffset = 15.62, YOffset = 1.37, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Desconto Incondicionado" } },
            { "Trib.BcIssqn", new FieldMetadata { XOffset = 0.40, YOffset = 2.01, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "BC ISSQN" } },
            { "Trib.AliquotaAplicada", new FieldMetadata { XOffset = 5.41, YOffset = 2.01, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Alíquota Aplicada" } },
            { "Trib.RetencaoIssqn", new FieldMetadata { XOffset = 10.51, YOffset = 2.01, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Retenção do ISSQN" } },
            { "Trib.IssqnApurado", new FieldMetadata { XOffset = 15.62, YOffset = 2.01, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "ISSQN Apurado" } },

            // --- 9. TRIBUTOS FEDERAIS (Relativos) ---
            // Linha 1: BC PIS/COFINS, Alíquota PIS, Alíquota COFINS
            { "Fed.VBCPisCofins", new FieldMetadata { XOffset = 5.41, YOffset = 0.07, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "BC PIS/COFINS" } },
            { "Fed.AliqPis", new FieldMetadata { XOffset = 10.51, YOffset = 0.07, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Alíquota PIS (%)" } },
            { "Fed.AliqCofins", new FieldMetadata { XOffset = 15.62, YOffset = 0.07, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Alíquota COFINS (%)" } },
            // Linha 2: IRRF, CP, CSLL
            { "Fed.IRRF", new FieldMetadata { XOffset = 5.41, YOffset = 0.72, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "IRRF" } },
            { "Fed.CP", new FieldMetadata { XOffset = 10.51, YOffset = 0.72, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Contribuição Previdenciária - Retida" } },
            { "Fed.CSLL", new FieldMetadata { XOffset = 15.62, YOffset = 0.72, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Contribuições Sociais - Retidas" } },
            // Linha 3: PIS, COFINS, RetPisCofins
            { "Fed.PIS", new FieldMetadata { XOffset = 0.40, YOffset = 1.37, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "PIS - Débito Apuração Própria" } },
            { "Fed.COFINS", new FieldMetadata { XOffset = 5.41, YOffset = 1.37, Width = 4.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "COFINS - Débito Apuração Própria" } },
            { "Fed.RetPisCofins", new FieldMetadata { XOffset = 10.51, YOffset = 1.37, Width = 9.50, Height = 0.25, Alignment = LayoutAlignment.Left, Label = "Descrição Contrib. Sociais - Retidas" } }
        };
    }
}
