using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace NFSe.DANFSe.v2.Helpers
{
    public static class Formatters
    {
        private static readonly System.Collections.Generic.Dictionary<string, string> UfByIbgePrefix = 
            new System.Collections.Generic.Dictionary<string, string>
            {
                { "11", "RO" }, { "12", "AC" }, { "13", "AM" }, { "14", "RR" },
                { "15", "PA" }, { "16", "AP" }, { "17", "TO" }, { "21", "MA" },
                { "22", "PI" }, { "23", "CE" }, { "24", "RN" }, { "25", "PB" },
                { "26", "PE" }, { "27", "AL" }, { "28", "SE" }, { "29", "BA" },
                { "31", "MG" }, { "32", "ES" }, { "33", "RJ" }, { "35", "SP" },
                { "41", "PR" }, { "42", "SC" }, { "43", "RS" }, { "50", "MS" },
                { "51", "MT" }, { "52", "GO" }, { "53", "DF" }
            };

        public static string FormatDocumento(string doc)
        {
            if (string.IsNullOrEmpty(doc)) return doc;

            // Remove caracteres não numéricos
            doc = Regex.Replace(doc, @"[^\d]", "");

            if (doc.Length == 14)
            {
                return Convert.ToUInt64(doc).ToString(@"00\.000\.000\/0000\-00");
            }
            else if (doc.Length == 11)
            {
                return Convert.ToUInt64(doc).ToString(@"000\.000\.000\-00");
            }

            return doc;
        }

        public static string FormatCep(string cep)
        {
            if (string.IsNullOrEmpty(cep)) return cep;

            cep = Regex.Replace(cep, @"[^\d]", "");

            if (cep.Length == 8)
            {
                return Convert.ToUInt64(cep).ToString(@"00\.000\-000");
            }

            return cep;
        }

        public static string FormatPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone)) return phone;

            phone = Regex.Replace(phone, @"[^\d]", "");

            if (phone.Length == 10)
            {
                return Convert.ToUInt64(phone).ToString(@"(00) 0000\-0000");
            }
            else if (phone.Length == 11)
            {
                return Convert.ToUInt64(phone).ToString(@"(00) 00000\-0000");
            }

            return phone;
        }

        public static string FormatCurrency(string val)
        {
            if (string.IsNullOrEmpty(val) || val == "-") return "-";

            if (double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed))
            {
                return parsed.ToString("C2", new CultureInfo("pt-BR"));
            }

            return val;
        }

        public static string GetUfFromIbge(string cMun)
        {
            if (string.IsNullOrEmpty(cMun) || cMun.Length < 2) return string.Empty;
            var prefix = cMun.Substring(0, 2);
            return UfByIbgePrefix.TryGetValue(prefix, out var uf) ? uf : string.Empty;
        }

        public static string FormatMunUf(string xMun, string cMun, string uf)
        {
            var mun = xMun;
            if (string.IsNullOrEmpty(mun))
            {
                mun = IbgeResolver.GetCityName(cMun);
            }

            if (string.IsNullOrEmpty(uf))
            {
                uf = GetUfFromIbge(cMun);
            }

            if (string.IsNullOrEmpty(mun))
            {
                return !string.IsNullOrEmpty(uf) ? uf : "-";
            }

            return !string.IsNullOrEmpty(uf) ? $"{mun} / {uf}" : mun;
        }

        public static string FormatIbgeCep(string cMun, string cep)
        {
            if (string.IsNullOrEmpty(cep)) return "-";
            
            var formattedCep = FormatCep(cep);
            if (string.IsNullOrEmpty(cMun)) return formattedCep;

            return $"{cMun} / {formattedCep}";
        }

        public static string FormatTipoTributacao(string code)
        {
            if (code == Models.NfseSchemaValues.TribIssqnTributavel) return "Operação Tributável";
            if (code == Models.NfseSchemaValues.TribIssqnExportacao) return "Exportação de Serviço";
            if (code == Models.NfseSchemaValues.TribIssqnNaoIncidencia) return "Não Incidência";
            if (code == Models.NfseSchemaValues.TribIssqnImunidade) return "Imunidade";
            return string.IsNullOrEmpty(code) ? "-" : code;
        }

        public static string FormatRegimeEspecial(string code)
        {
            return code switch
            {
                "0" => "Nenhum",
                "1" => "Microempresa Municipal",
                "2" => "Estimativa",
                "3" => "Sociedade de Profissionais",
                "4" => "Cooperativa",
                "5" => "MEI",
                "6" => "ME/EPP",
                "9" => "Outros",
                _ => string.IsNullOrEmpty(code) ? "-" : code
            };
        }

        public static string FormatTipoBM(string code, string versao)
        {
            if (string.IsNullOrEmpty(code)) return string.Empty;
            
            bool isV100 = versao == "1.00";
            if (isV100)
            {
                return code switch
                {
                    "1" => "Alíquota Diferenciada",
                    "2" => "Redução da BC",
                    "3" => "Isenção",
                    _ => code
                };
            }
            else // Default to v1.01
            {
                return code switch
                {
                    "1" => "Isenção",
                    "2" => "Redução da BC",
                    "3" => "Redução da BC em R$",
                    "4" => "Alíquota Diferenciada",
                    _ => code
                };
            }
        }

        public static string FormatRetIss(string code)
        {
            if (code == Models.NfseSchemaValues.RetIssNaoRetido) return "Não Retido";
            if (code == Models.NfseSchemaValues.RetIssRetidoTomador) return "Retido pelo Tomador";
            if (code == Models.NfseSchemaValues.RetIssRetidoIntermediario) return "Retido pelo Intermediário";
            return string.IsNullOrEmpty(code) ? "-" : code;
        }

        public static string FormatExigibilidadeSuspensa(string code)
        {
            if (code == Models.NfseSchemaValues.ExigSuspNao) return "Não";
            if (code == Models.NfseSchemaValues.ExigSuspJudicial) return "Exigibilidade Suspensa por Decisão Judicial";
            if (code == Models.NfseSchemaValues.ExigSuspAdministrativo) return "Exigibilidade Suspensa por Processo Administrativo";
            return string.IsNullOrEmpty(code) ? "-" : code;
        }

        public static string FormatOpSimplesNacional(string code)
        {
            if (code == Models.NfseSchemaValues.OpSimplesNaoOptante) return "1 - Não Optante";
            if (code == Models.NfseSchemaValues.OpSimplesOptanteMei) return "2 - Optante - MEI";
            if (code == Models.NfseSchemaValues.OpSimplesOptanteMeEpp) return "3 - Optante - ME/EPP";
            return string.IsNullOrEmpty(code) ? "-" : code;
        }

        public static string FormatRegApTribSN(string code)
        {
            if (code == Models.NfseSchemaValues.RegApTribSnMeEpp) return "1 - ME/EPP";
            if (code == Models.NfseSchemaValues.RegApTribSnMei) return "2 - MEI";
            return string.IsNullOrEmpty(code) ? "-" : code;
        }

        public static string FormatRetPisCofins(string code)
        {
            return code switch
            {
                "1" => "PIS/COFINS/CSLL Não Retido",
                "2" => "Retido pelo Tomador",
                _ => string.IsNullOrEmpty(code) ? "-" : code
            };
        }

        public static string FormatTribNac(string code)
        {
            if (string.IsNullOrEmpty(code)) return "-";
            if (code.Length == 6)
            {
                return $"{code.Substring(0, 2)}.{code.Substring(2, 2)}.{code.Substring(4, 2)}";
            }
            return code;
        }

        /// <summary>
        /// Formata uma data/hora no padrão do XML Schema para o formato PT-BR (dd/MM/yyyy HH:mm:ss).
        /// </summary>
        public static string FormatDateTime(string val)
        {
            if (string.IsNullOrEmpty(val)) return "-";

            if (DateTimeOffset.TryParse(val, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset dto))
            {
                return dto.ToString("dd/MM/yyyy HH:mm:ss");
            }

            if (DateTime.TryParse(val, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
            {
                return dt.ToString("dd/MM/yyyy HH:mm:ss");
            }

            return val;
        }

        /// <summary>
        /// Formata uma data no padrão do XML Schema para o formato PT-BR (dd/MM/yyyy).
        /// </summary>
        public static string FormatDate(string val)
        {
            if (string.IsNullOrEmpty(val)) return "-";

            if (DateTimeOffset.TryParse(val, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset dto))
            {
                return dto.ToString("dd/MM/yyyy");
            }

            if (DateTime.TryParse(val, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
            {
                return dt.ToString("dd/MM/yyyy");
            }

            return val;
        }
    }
}
