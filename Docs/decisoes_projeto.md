# Registro de Decisões de Arquitetura (ADRs) - NFSe.DANFSe.v2

Este documento consolida as principais decisões de design, regras de layout, configurações de repositório e padrões de segurança adotados no desenvolvimento e refatoração do componente **NFSe.DANFSe.v2** desde o início do projeto. Ele serve como referência para mantenedores humanos e modelos de IA em desenvolvimentos futuros.

---

## 1. Estrutura do Projeto, Metadados e Nomenclatura
* **Decisão**: A versão inicial do projeto foi fixada a partir de `0.1.0` (estabilização após início em `0.0.0`).
* **Metadados**: Os metadados do pacote NuGet foram configurados com autor definido como `Adriano Della` e Company mantida vazia.
* **Renomeação e Reorganização Física**:
  * O projeto foi renomeado de `NFSe.Componente.v2` para `NFSe.DANFSe.v2` para refletir de forma explícita o seu propósito técnico (geração do documento auxiliar).
  * O código-fonte foi movido da pasta interna redundante para a raiz do diretório `src/`.
  * A pasta de testes foi renomeada de `tests/` para `Tests/` na raiz do projeto.
  * A classe principal de testes de PDF foi renomeada para `DanfseGenerationTests.cs`.
* **Benefício**: Estrutura de pastas limpa, portável e fácil de navegar.

---

## 2. Padrão de Segurança e Higienização de Dados (LGPD)
* **Decisão**: Todos os dados sensíveis de pessoas físicas ou jurídicas reais presentes nos XMLs de amostras originais foram integralmente higienizados para a publicação do repositório no GitHub.
* **Ajustes de segurança**:
  * CPFs, CNPJs, nomes de emitentes, e-mails, telefones e certificados digitais reais foram substituídos por dados fictícios (ex: `12345678909`, `PRESTADOR DE SERVICOS DUMMY LTDA`).
  * As chaves de acesso de 50 dígitos foram geradas de forma aleatória para remover o CPF do emitente original que estava embutido na composição da chave.
  * **Obrigatoriedade**: Qualquer nova amostra de XML adicionada em `Tests/Samples/` para testes futuros deve passar pelo mesmo processo de higienização.

---

## 3. Testes Unitários Autônomos e Caminhos Relativos
* **Decisão**: A suíte de testes deve rodar de forma 100% isolada e portátil, sem depender de caminhos de arquivos absolutos na máquina de desenvolvimento.
* **Implementação**:
  * Configuração no `.csproj` de testes para copiar recursivamente todos os arquivos da pasta `Samples/` (XMLs e imagem de logo customizado) para o diretório binário de build (`PreserveNewest`).
  * Utilização de `AppDomain.CurrentDomain.BaseDirectory` para localizar as amostras, garantindo compatibilidade com pipelines de Integração Contínua (CI) como GitHub Actions.

---

## 4. Mascaramento, Formatação de Campos e Regras de Negócio (Helpers/Formatters.cs)
* **Decisão**: Garantir que as informações numéricas puras do XML sejam mascaradas de forma legível no PDF e mapeadas para descrições por extenso de acordo com a Nota Técnica.
* **Máscaras padrão**: CPF, CNPJ, CEP e Telefones recebem suas formatações visuais antes do desenho na tela.
* **Mapeamento de Enums**: Valores do XML como `regTrib` (Regime Especial de Tributação), `tpISS` (Retenção do ISS) e `exigSusp` (Exigibilidade Suspensa) são convertidos em descrições amigáveis por extenso.
* **Resolução de Código de Obra (`_model.Servico.CObra`)**: Impressão condicional do código da obra nos campos correspondentes ou na seção de Informações Complementares.

---

## 5. Mapeamento Condicional do Benefício Municipal (BM) por Versão
* **Decisão**: O campo de Benefício Municipal deve exibir o código do benefício (`nBM`) concatenado com a descrição correspondente do tipo de benefício (`tpBM`), cuja semântica varia conforme a versão do schema da NFS-e.
* **Implementação**:
  * **Schema v1.00**: `1` = "Alíquota Diferenciada", `2` = "Redução da BC", `3` = "Isenção".
  * **Schema v1.01**: `1` = "Isenção", `2` = "Redução da BC", `3` = "Redução da BC em R$", `4` = "Alíquota Diferenciada".
  * A descrição resultante no formato `nBM - [Descrição]` é truncada em 40 caracteres e preenchida com reticências (`...`) caso a descrição exceda 37 caracteres.

---

## 6. Algoritmo de Truncamento Visual (`TruncateTextToWidth`)
* **Decisão**: Evitar que textos longos (como Código de Obra ou descrições) estourem as margens físicas das colunas e células delimitadas no PDFsharp.
* **Implementação**:
  * Criamos o método helper `TruncateTextToWidth` (e seu respectivo overload limitador de comprimento físico). Ele mede o tamanho em pontos que o texto ocupará no papel (usando a fonte e o tamanho selecionado) em relação à largura disponível na célula, cortando-o com reticências se ultrapassar o limite, impedindo sobreposições ou quebras visuais de linha inesperadas.

---

## 7. Desenho de Marcas d'Água de Notas Canceladas e Substituídas
* **Decisão**: A exibição de marcas d'água deve seguir rigorosamente as diretrizes visuais da página 23 da Nota Técnica.
* **Implementação**:
  * Quando uma nota possui a flag `IsCancelled` ou `IsSubstituted`, o renderizador rotaciona o contexto gráfico em `-35` graus com ponto de origem no centro da página e desenha a marca correspondente ("CANCELADA" ou "SUBSTITUÍDA") em cinza K35 semi-transparente (RGB 166, 166, 166 com opacidade de 30%).

---

## 8. Layout Orientado a Metadados (`DanfseLayoutRegistry`)
* **Decisão**: Separar a especificação das coordenadas físicas das células de desenho da lógica de renderização em si.
* **Implementação**:
  * Criação do arquivo `src/Rendering/DanfseLayoutRegistry.cs` contendo as larguras, alturas, alinhamentos, posições X/Y e textos dos rótulos de todas as células do DANFSe nacional (Nota Técnica 008).
  * Implementação da função helper `DrawMetadataField` em `DanfsePdfRenderer.cs` que centraliza as regras de desenho de bordas, preenchimento cinza lateral, aplicação de fontes de rótulos e valores, e truncamento automático de textos extensos.
* **Benefício**: Alterações ou ajustes visuais de layout agora são feitos apenas alterando propriedades declarativas nos metadados, sem tocar na complexa lógica do renderizador de PDF.

---

## 9. Marca d'água "TESTE" (Sem Violação do Schema XML)
* **Decisão**: Evitar o uso de ambientes fictícios (`tpAmb = 0` ou similares) nos XMLs de teste para não quebrar a validação oficial do XSD da Receita Federal.
* **Implementação**:
  * Inclusão do parâmetro opcional `bool forceTestWatermark = false` no método principal `GeneratePdf`.
  * Quando habilitado, o renderizador desenha de forma forçada a palavra `"TESTE"` na diagonal do PDF, permitindo testes visuais internos de forma limpa, sem corromper ou inviabilizar a integridade estrutural do XML original.

---

## 10. Comportamento do Logotipo Customizado
* **Decisão**: Se o usuário fornecer uma logomarca personalizada do prestador via parâmetro `logoBytes`, esta imagem deve substituir completamente a marca nacional padrão da NFS-e no canto superior esquerdo do DANFSe, em vez de serem exibidas lado a lado.
* **Benefício**: Melhor aproveitamento do layout de cabeçalho (`4,00 cm x 1,16 cm`), garantindo um visual limpo e profissional, evitando poluição visual ou sobreposição de marcas em espaços exíguos.

---

## 11. Parser de XML Resiliente a Namespaces
* **Decisão**: A leitura dos campos tributários (como a tag `<trib>`) e dados da nota no parser é realizada resolvendo os namespaces dinamicamente em tempo de execução.
* **Benefício**: Garante que variações futuras de namespaces (schemas v1.00, v1.01, etc.) da Receita Federal não quebrem a desserialização do componente.

---

## 12. Compatibilidade de Buffer no PDFsharp 6
* **Decisão**: Para evitar exceções de permissão de acesso ao carregar logos e imagens a partir de memória, o buffer deve ser instanciado de forma pública:
  ```csharp
  new MemoryStream(logoBytes, 0, logoBytes.Length, false, true); // publiclyVisible = true
  ```
* **Benefício**: Garante compatibilidade e execução sem erros do componente em microsserviços na nuvem, ambientes de contêineres e aplicações web.

---

## 13. Detalhamento dos Planos de Implementação Executados

### A. Refatoração de Layout e Metadados (Abordagem Híbrida)
Durante o desenvolvimento do `DanfseLayoutRegistry`, definimos que o DANFSe se divide em blocos estruturais estáticos e blocos dinâmicos. A implementação seguiu a seguinte estratégia técnica de coordenadas:
1. **Coordenadas Absolutas**: Campos de blocos estáticos (como o Cabeçalho principal da nota e o Canhoto do recibo de entrega) possuem coordenadas `Y` fixadas de forma absoluta no registro de metadados em centímetros.
2. **Coordenadas Relativas (Offset)**: Campos de blocos dinâmicos (como Tomador de Serviços, Intermediário, Serviço Prestado e Tributação Municipal) possuem posições `Y` tratadas como *Offsets* (deslocamento vertical). No renderizador, o valor real de desenho é computado somando o `YOffset` do metadado com a variável dinâmica `currentBlockY` calculada em tempo de execução.
3. **Padrão de Desenho Vetorial**: Células com fundos cinzas sombreados e bordas internas do grid de tabelas são desenhadas via `DrawMetadataField`, o qual calcula automaticamente o recuo do texto do valor para não colidir com o texto pequeno do rótulo da célula (`ValueYOffset` de `0.30 cm`).

### B. Procedimento de Migração e Reestruturação de Pastas
Para executar a renomeação e reestruturação física do projeto mantendo a integridade do compilador, o seguinte fluxo técnico passo a passo foi executado:
1. **Limpeza Prévia**: Remoção física de diretórios temporários e de compilação local (`bin/` e `obj/`) para evitar bloqueio de handles de arquivos pelo sistema operacional e reduzir arquivos redundantes.
2. **Reorganização de Raiz**:
   * Movimentação de todos os recursos lógicos do projeto C# de `src/NFSe.Componente.v2/` para a raiz `src/`.
   * Reorganização dos testes da pasta de origem unificada de subníveis para `Tests/` na raiz do diretório do projeto.
3. **Renomeação e Correção de Referências**:
   * Renomeação do arquivo XML de solução moderno do Visual Studio para `NFSe.DANFSe.v2.slnx` e substituição das referências de caminhos internos dos projetos correspondentes.
   * Renomeação de arquivos de projetos (`.csproj`) e workspaces (`.code-workspace`).
   * Substituição de todas as ocorrências de `NFSe.Componente.v2` por `NFSe.DANFSe.v2` em arquivos de código C#, incluindo namespaces e declarações de `using`.
   * Atualização das strings literais de manifesto do Assembly em `FontResolver.cs` e `DanfsePdfRenderer.cs` para carregar as fontes TTF e a logo PNG oficiais sob o novo namespace raiz.
4. **Verificação de Compilação**: Restauração dos pacotes NuGet e verificação do fluxo de testes via CLI com o utilitário do SDK `dotnet test`.
