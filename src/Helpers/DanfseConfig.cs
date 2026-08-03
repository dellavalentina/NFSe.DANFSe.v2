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

                try
                {
                    string? appSetting = ConfigurationManager.AppSettings["NFSe.DANFSe.AutoFixInconsistentTotal"];
                    if (bool.TryParse(appSetting, out bool result))
                    {
                        return result;
                    }
                }
                catch
                {
                    // Ignora exceções de acesso a configurações
                }

                return false;
            }
            set => _autoFixInconsistentTotal = value;
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
