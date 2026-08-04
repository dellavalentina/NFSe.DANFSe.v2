using System;
#if NETFRAMEWORK
using System.Configuration;
#endif
using System.Linq;
using System.Reflection;

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

                _autoFixInconsistentTotal = TryGetConfigurationValue();
                return _autoFixInconsistentTotal.Value;
            }
            set => _autoFixInconsistentTotal = value;
        }

        /// <summary>
        /// Tenta ler a configuração do App.config/Web.config com múltiplas estratégias de fallback.
        /// </summary>
        private static bool TryGetConfigurationValue()
        {
            System.Diagnostics.Debug.WriteLine("=== Iniciando leitura de configuração NFSe.DANFSe.AutoFixInconsistentTotal ===");

#if NETFRAMEWORK
            // No .NET Framework, o acesso direto é garantido, nativo e rápido
            return TryGetConfigurationValueDirect();
#else
            // No .NET Standard 2.0 / .NET 6.0, evitamos referenciar ConfigurationManager diretamente
            // para não disparar exceções de JIT em runtimes que não o possuam. Usamos reflexão segura.
            return TryGetConfigurationValueReflection();
#endif
        }

#if NETFRAMEWORK
        /// <summary>
        /// Tenta ler configuração usando ConfigurationManager diretamente.
        /// Funciona quando System.Configuration está disponível (principalmente .NET Framework)
        /// </summary>
        private static bool TryGetConfigurationValueDirect()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[Estratégia 1] Tentando acesso direto a ConfigurationManager...");

                // Verificar se ConfigurationManager está disponível
                if (ConfigurationManager.AppSettings == null)
                {
                    System.Diagnostics.Debug.WriteLine("[Estratégia 1] ConfigurationManager.AppSettings é null");
                    return false;
                }

                string? value = ConfigurationManager.AppSettings["NFSe.DANFSe.AutoFixInconsistentTotal"];
                System.Diagnostics.Debug.WriteLine($"[Estratégia 1] Valor bruto obtido: '{value}'");

                if (value != null && bool.TryParse(value, out bool result))
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[Estratégia 1] ✓ Sucesso! Configuração lida: {result}");
                    return result;
                }

                if (value != null)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[Estratégia 1] Valor obtido mas não é bool válido: '{value}'");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[Estratégia 1] ✗ Erro: {ex.GetType().Name} - {ex.Message}");
            }

            return false;
        }
#endif

        /// <summary>
        /// Tenta ler a configuração do App.config por reflexão como fallback.
        /// Necessário para .NET Standard 2.0 quando System.Configuration não está diretamente disponível.
        /// </summary>
        private static bool TryGetConfigurationValueReflection()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[Estratégia 2] Tentando acesso via reflexão...");

                Type? configType = null;

                // Etapa 1: Tentar carregar o assembly e tipo
                System.Diagnostics.Debug.WriteLine("[Estratégia 2] Etapa 1 - Procurando Type System.Configuration.ConfigurationManager...");

                // Tentativa 1: Type.GetType direto (mais provável em .NET Framework)
                configType = Type.GetType("System.Configuration.ConfigurationManager, System.Configuration");
                if (configType != null)
                {
                    System.Diagnostics.Debug.WriteLine("[Estratégia 2] ✓ Encontrado via Type.GetType (System.Configuration)");
                }
                else
                {
                    // Tentativa 2: Carregar explicitamente o assembly
                    System.Diagnostics.Debug.WriteLine("[Estratégia 2] Tentando carregar assembly System.Configuration.ConfigurationManager...");
                    try
                    {
                        var asm = System.Reflection.Assembly.Load("System.Configuration.ConfigurationManager");
                        configType = asm?.GetType("System.Configuration.ConfigurationManager");
                        if (configType != null)
                        {
                            System.Diagnostics.Debug.WriteLine("[Estratégia 2] ✓ Encontrado via Assembly.Load");
                        }
                    }
                    catch (Exception asmEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Estratégia 2] Falha ao carregar assembly: {asmEx.Message}");
                    }
                }

                if (configType == null)
                {
                    System.Diagnostics.Debug.WriteLine("[Estratégia 2] ✗ Type não encontrado");
                    return false;
                }

                // Etapa 2: Obter AppSettings
                System.Diagnostics.Debug.WriteLine("[Estratégia 2] Etapa 2 - Obtendo propriedade AppSettings...");
                PropertyInfo? appSettingsProperty = configType.GetProperty(
                    "AppSettings",
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);

                if (appSettingsProperty == null)
                {
                    System.Diagnostics.Debug.WriteLine("[Estratégia 2] ✗ Propriedade AppSettings não encontrada");
                    return false;
                }

                object? appSettings = appSettingsProperty.GetValue(null);
                if (appSettings == null)
                {
                    System.Diagnostics.Debug.WriteLine("[Estratégia 2] ✗ AppSettings retornou null");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"[Estratégia 2] ✓ AppSettings obtido (tipo: {appSettings.GetType().Name})");

                // Etapa 3: Usar a NameValueCollection para obter o valor
                System.Diagnostics.Debug.WriteLine("[Estratégia 2] Etapa 3 - Acessando chave 'NFSe.DANFSe.AutoFixInconsistentTotal'...");

                string? appSetting = null;

                // Forma 1: Via indexer (mais direto)
                try
                {
                    var indexerMethod = appSettings.GetType().GetMethod("get_Item",
                        BindingFlags.Public | BindingFlags.Instance,
                        null,
                        new[] { typeof(string) },
                        null);

                    if (indexerMethod != null)
                    {
                        appSetting = indexerMethod.Invoke(appSettings, new[] { (object)"NFSe.DANFSe.AutoFixInconsistentTotal" }) as string;
                        System.Diagnostics.Debug.WriteLine($"[Estratégia 2] Valor obtido via indexer: '{appSetting}'");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Estratégia 2] Indexer falhou: {ex.Message}");
                }

                // Forma 2: Via método Get se a forma 1 falhar
                if (string.IsNullOrEmpty(appSetting))
                {
                    try
                    {
                        var getMethod = appSettings.GetType().GetMethod("Get",
                            BindingFlags.Public | BindingFlags.Instance);

                        if (getMethod != null)
                        {
                            appSetting = getMethod.Invoke(appSettings, new[] { (object)"NFSe.DANFSe.AutoFixInconsistentTotal" }) as string;
                            System.Diagnostics.Debug.WriteLine($"[Estratégia 2] Valor obtido via Get: '{appSetting}'");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Estratégia 2] Get falhou: {ex.Message}");
                    }
                }

                if (!string.IsNullOrEmpty(appSetting))
                {
                    if (bool.TryParse(appSetting, out bool result))
                    {
                        System.Diagnostics.Debug.WriteLine($"[Estratégia 2] ✓ Sucesso! Configuração lida via reflexão: {result}");
                        return result;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[Estratégia 2] Valor obtido mas não é bool válido: '{appSetting}'");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[Estratégia 2] Chave não encontrada em AppSettings");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[Estratégia 2] ✗ Erro geral: {ex.GetType().Name} - {ex.Message}");
            }

            System.Diagnostics.Debug.WriteLine("[Estratégia 2] Retornando false (padrão)");
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

