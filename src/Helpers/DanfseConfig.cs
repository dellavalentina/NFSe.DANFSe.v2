using System;
using System.Configuration;

namespace NFSe.DANFSe.v2.Helpers
{
    /// <summary>
    /// Fornece opções de configuração globais para a biblioteca NFSe.DANFSe.v2 com suporte automático a App.config / Web.config.
    /// </summary>
    public static class DanfseConfig
    {
        private static bool? _autoFixInconsistentTotal;

        /// <summary>
        /// Obtém ou define a configuração global para recálculo automático de totais inconsistentes da SEFIN Nacional.
        /// Se não for definido explicitamente via código, lê a chave "NFSe.DANFSe.AutoFixInconsistentTotal" do App.config da aplicação cliente.
        /// </summary>
        public static bool AutoFixInconsistentTotal
        {
            get
            {
                if (_autoFixInconsistentTotal.HasValue)
                {
                    return _autoFixInconsistentTotal.Value;
                }

                // Tenta ler do arquivo de configuração com tratamento robusto
                bool configValue = TryGetConfigurationValue();
                return configValue;
            }
            set => _autoFixInconsistentTotal = value;
        }

        /// <summary>
        /// Tenta obter o valor de configuração do App.config com tratamento completo de exceções.
        /// </summary>
        private static bool TryGetConfigurationValue()
        {
            try
            {
                string? appSetting = ConfigurationManager.AppSettings["NFSe.DANFSe.AutoFixInconsistentTotal"];

                if (bool.TryParse(appSetting, out bool result))
                {
                    return result;
                }
            }
            catch (System.IO.FileNotFoundException ex)
            {
                // Arquivo de configuração não encontrado - logging opcional
                System.Diagnostics.Debug.WriteLine(
                    $"Aviso: Arquivo de configuração não encontrado ao tentar ler 'NFSe.DANFSe.AutoFixInconsistentTotal'. " +
                    $"Usando valor padrão (false). Detalhes: {ex.Message}");
            }
            catch (ConfigurationErrorsException ex)
            {
                // Erro ao parsear o arquivo de configuração
                System.Diagnostics.Debug.WriteLine(
                    $"Aviso: Erro ao processar arquivo de configuração para 'NFSe.DANFSe.AutoFixInconsistentTotal'. " +
                    $"Usando valor padrão (false). Detalhes: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Qualquer outra exceção
                System.Diagnostics.Debug.WriteLine(
                    $"Aviso: Erro inesperado ao ler configuração 'NFSe.DANFSe.AutoFixInconsistentTotal'. " +
                    $"Usando valor padrão (false). Tipo: {ex.GetType().Name}, Detalhes: {ex.Message}");
            }

            // Valor padrão seguro
            return false;
        }

        /// <summary>
        /// Reseta os valores em memória da configuração, forçando nova leitura do App.config quando aplicável.
        /// </summary>
        public static void Reset()
        {
            _autoFixInconsistentTotal = null;
        }
    }
}
