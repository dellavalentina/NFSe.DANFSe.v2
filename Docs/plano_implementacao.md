# Plano de Implementação Histórico e Evolutivo - NFSe.DANFSe.v2

Este documento registra a evolução e o detalhamento técnico de todas as fases do plano de implementação executadas desde o início do projeto até a entrega da versão refatorada **NFSe.DANFSe.v2**, organizadas por versões de release.

---

## Índice de Releases e Fases

### [Release v0.1.2] - 2026-07-09 (Ajuste de Nomenclatura e Versão)
- [Fase X: Ajuste de Terminologia para Biblioteca/Pacote](#fase-x-ajuste-de-terminologia-para-bibliotecapacote-release-v012)

### [Release v0.1.1] - 2026-07-09 (Ajuste de Formatação e Patch Release)
- [Fase IX: Ajuste de Formatação de Data/Hora para PT-BR](#fase-ix-ajuste-de-formatacao-de-datahora-para-pt-br-release-v011)

### [Release v0.1.0] - 2026-07-08 (Implementação Inicial da v2)
- [Fase I: Inicialização do Repositório e Ambiente de CI](#fase-i-inicializacao-do-repositorio-e-ambiente-de-ci)
- [Fase II: Motor Visual e Conformidade com a Nota Técnica 008](#fase-ii-motor-visual-e-conformidade-com-a-nota-tecnica-008)
- [Fase III: XML Parser Resiliente e Regras de Negócio de Campos](#fase-iii-xml-parser-resiliente-e-regras-de-negocio-de-campos)
- [Fase IV: Algoritmos de Truncamento Visual e Estouro de Texto](#fase-iv-algoritmos-de-truncamento-visual-e-estouro-de-texto)
- [Fase V: Refatoração do Motor de Layout Orientado a Metadados](#fase-v-refatoracao-do-motor-de-layout-orientado-a-metadados)
- [Fase VI: Higienização de Dados e Portabilidade de Testes](#fase-vi-higienizacao-de-dados-e-portabilidade-de-testes)
- [Fase VII: Parâmetros de Customização e Marca d'Água de Teste](#fase-vii-parametros-de-customizacao-e-marca-dagua-de-teste)
- [Fase VIII: Reorganização de Pastas, Namespaces e Nomenclatura (Refactoring Principal)](#fase-viii-reorganizacao-de-pastas-namespaces-e-nomenclatura-refactoring-principal)

---

## Release v0.1.2 - 2026-07-09

### Fase X: Ajuste de Terminologia para Biblioteca/Pacote (Release v0.1.2)
* **Objetivo**: Padronizar as referências a "Componente" para "Biblioteca" ou "Pacote" nas configurações do projeto, documentação e logs internos, e atualizar a versão para `0.1.2`.
* **Ações Executadas**:
  * **Configurações do Projeto**: Atualização da descrição do pacote NuGet e alteração da tag `<Version>` para `0.1.2` em [NFSe.DANFSe.v2.csproj](file:///d:/__Projetos_Qualidata_SVN/sistemas-fontes/NotaFiscalEletronica/NFSe.DANFSe.v2/src/NFSe.DANFSe.v2.csproj).
  * **Documentação**: Atualização do badge de versão no [README.md](file:///d:/__Projetos_Qualidata_SVN/sistemas-fontes/NotaFiscalEletronica/NFSe.DANFSe.v2/README.md) e divisão clara das alterações entre `0.1.1` e `0.1.2` no [CHANGELOG.md](file:///d:/__Projetos_Qualidata_SVN/sistemas-fontes/NotaFiscalEletronica/NFSe.DANFSe.v2/CHANGELOG.md).
  * **Rastreamento de Logs**: Atualização do prefixo do logger interno para `[NFSe.DANFSe.v2]` em [DanfsePdfRenderer.cs](file:///d:/__Projetos_Qualidata_SVN/sistemas-fontes/NotaFiscalEletronica/NFSe.DANFSe.v2/src/Rendering/DanfsePdfRenderer.cs) para coincidir com a nomenclatura final do assembly.
  * **Validação**: Execução bem-sucedida de `dotnet clean`, `dotnet build` e de todos os 18 testes automatizados via `dotnet test`.

---

## Release v0.1.1 - 2026-07-09

### Fase IX: Ajuste de Formatação de Data/Hora para PT-BR (Release v0.1.1)
* **Objetivo**: Formatar elementos de data/hora no padrão brasileiro (PT-BR) na geração do PDF do DANFSe e documentar o release v0.1.1.
* **Ações Executadas**:
  * **Formatadores**: Criação dos métodos `FormatDate` e `FormatDateTime` em [Formatters.cs](file:///d:/__Projetos_Qualidata_SVN/sistemas-fontes/NotaFiscalEletronica/NFSe.DANFSe.v2/src/Helpers/Formatters.cs) para conversão segura de datas ISO 8601.
  * **Renderização**: Atualização em [DanfsePdfRenderer.cs](file:///d:/__Projetos_Qualidata_SVN/sistemas-fontes/NotaFiscalEletronica/NFSe.DANFSe.v2/src/Rendering/DanfsePdfRenderer.cs) para usar os novos formatadores ao renderizar a Competência da NFS-e (`DCompet`), Data/Hora de Emissão da NFS-e (`DhProc`) e Data/Hora de Emissão da DPS (`DhEmi`).
  * **Controle de Versão**: Ajuste da tag `<Version>` para `0.1.1` em [NFSe.DANFSe.v2.csproj](file:///d:/__Projetos_Qualidata_SVN/sistemas-fontes/NotaFiscalEletronica/NFSe.DANFSe.v2/src/NFSe.DANFSe.v2.csproj).
  * **Changelog**: Registro do novo patch release no topo do arquivo [CHANGELOG.md](file:///d:/__Projetos_Qualidata_SVN/sistemas-fontes/NotaFiscalEletronica/NFSe.DANFSe.v2/CHANGELOG.md).
  * **Documentação**: Atualização do badge de versão no [README.md](file:///d:/__Projetos_Qualidata_SVN/sistemas-fontes/NotaFiscalEletronica/NFSe.DANFSe.v2/README.md).
  * **Testes**: Criação e execução com sucesso de novos testes unitários validando as formatações.
  * **Status de Sincronização do Release**:
    * **GitHub**: A versão `0.1.1` foi comitada e enviada para o GitHub (`origin/main`).
    * **NuGet.org**: A versão pública ativa no NuGet.org era `0.1.0`. A versão `0.1.1` foi gerada localmente em `src/bin/Release/NFSe.DANFSe.v2.0.1.1.nupkg` e necessita ser enviada pelo usuário no terminal via comando `dotnet nuget push` usando sua chave de API para concluir a indexação do pacote público.

---

## Release v0.1.0 - 2026-07-08

### Fase I: Inicialização do Repositório e Ambiente de CI
* **Objetivo**: Estruturar a base do projeto em .NET 6 de forma robusta e integrada com boas práticas de versionamento.
* **Ações Executadas**:
  * Definição da versão inicial como `0.1.0`, de autoria de `Adriano Della` e sem empresa associada.
  * Criação dos arquivos `.gitignore` (excluindo pastas `bin/`, `obj/`, `.vs/`, builds e PDFs de output dos testes) e `.gitattributes` (garantindo saneamento e normalização de quebras de linha `LF` nos arquivos do projeto).
  * Criação do pipeline de integração contínua do GitHub Actions ([`dotnet.yml`](file:///D:/__Projetos_Qualidata_SVN/sistemas-fontes/NotaFiscalEletronica/NFSe.DANFSe.v2/.github/workflows/dotnet.yml)) configurado para restaurar, compilar em modo Release e testar a solução automaticamente a cada push/pull request.
  * Criação do arquivo de template de Pull Request (`pull_request_template.md`).

### Fase II: Motor Visual e Conformidade com a Nota Técnica 008
* **Objetivo**: Implementar o desenho visual do DANFSe nacional do zero baseado na biblioteca PDFsharp 6.x, em conformidade com as fontes, tamanhos e tabelas exigidos.
* **Ações Executadas**:
  * Desenvolvimento do `EmbeddedFontResolver.cs` que lê as fontes LiberationSans (Regular, Bold, Italic) embutidas como recursos do assembly, garantindo a renderização consistente do PDF independente de fontes instaladas no sistema operacional do host.
  * Desenho das bordas das tabelas (grid com espessura padrão de 0.5pt e cantos arredondados).
  * Renderização de todos os campos textuais de cabeçalho, canhoto de recebimento, emitente, tomador, intermediário, discriminação de serviços, tributos federais/IBS/CBS e dados adicionais.
  * Integração com a biblioteca QRCoder para desenhar o QR Code oficial de consulta nacional diretamente como vetor gráfico no PDF (evitando pixelização).
  * Adição das marcas d'água diagonais "CANCELADA" e "SUBSTITUÍDA" em cor cinza K35 com opacidade de 30%, centralizadas na página (página 23 da NT-008).

### Fase III: XML Parser Resiliente e Regras de Negócio de Campos
* **Objetivo**: Corrigir falhas de leitura no parser causadas por variações de namespaces e prefixos do XML da NFS-e.
* **Ações Executadas**:
  * Refatoração do `DanfseXmlParser.cs` para buscar elementos ignorando a declaração estática de namespaces (resiliência dinâmica de schemas).
  * Correção de leitura de campos cruciais como o grupo de elementos `<trib>` dentro de `<valores>`.
  * Implementação da lógica do Benefício Municipal: concatenação do código `nBM` com a descrição de tipo `tpBM` diferenciando o enumerado de acordo com a versão do schema da nota (comportamento distinto nas versões `v1.00` e `v1.01`).

### Fase IV: Algoritmos de Truncamento Visual e Estouro de Texto
* **Objetivo**: Resolver problemas de sobreposição de textos em células de dimensões fixadas no DANFSe.
* **Ações Executadas**:
  * Criação do método helper `TruncateTextToWidth` que calcula o comprimento gráfico físico que uma string ocupará no papel em pontos (Pt) baseado na fonte e tamanho especificados.
  * Substituição de truncamentos estáticos baseados em comprimento de caracteres (`Substring`) por truncamento visual dinâmico com reticências (`...`), aplicado para textos grandes como o Benefício Municipal e Código da Obra (`_model.Servico.CObra`).

### Fase V: Refatoração do Motor de Layout Orientado a Metadados
* **Objetivo**: Remover a lógica rígida de posicionamento de coordenadas absolutas do arquivo de renderização principal para facilitar futuras edições visuais da Nota Técnica.
* **Ações Executadas**:
  * Criação de [`DanfseLayoutRegistry.cs`](file:///D:/__Projetos_Qualidata_SVN/sistemas-fontes/NotaFiscalEletronica/NFSe.DANFSe.v2/src/Rendering/DanfseLayoutRegistry.cs) que mapeia todas as chaves de campos aos seus rótulos, posições de coordenadas X e Y em centímetros, larguras, alturas e alinhamentos de texto.
  * Criação da rotina unificada `DrawMetadataField` em `DanfsePdfRenderer.cs` que encapsula o comportamento de desenho dos metadados, aplicando sombreado cinza nos cantos apropriados, desenhando as fontes pequenas do rótulo e as fontes normais dos valores de forma automática.
  * Criação de regras de coordenadas híbridas: posições fixas absolutas para cabeçalho e posições dinâmicas baseadas em *Offsets* relativos para os blocos secundários (Tomador, Intermediário, Serviço e Impostos), cujas posições variam se algum bloco for ocultado.

### Fase VI: Higienização de Dados e Portabilidade de Testes
* **Objetivo**: Sanear a pasta de amostras para garantir que o repositório não trafegue nenhuma informação oficial ou sensível em conformidade com as regras da LGPD, e tornar os testes autônomos.
* **Ações Executadas**:
  * Criação da pasta `Samples/` dentro do projeto de testes contendo amostras limpas de XML (`danfse-normal.xml`, `danfse-cancelamento.xml`, `danfse-substituida.xml`).
  * Higienização de todos os nomes, CPFs/CNPJs, emails, certificados e assinaturas das notas.
  * Randomização completa das chaves de acesso de 50 dígitos para garantir a aleatoriedade de dados internos.
  * Refatoração da carga de arquivos nos testes para utilizar caminhos relativos ao executável (`AppDomain.CurrentDomain.BaseDirectory`), assegurando que a suíte execute com sucesso em qualquer máquina local ou de CI sem intervenção manual.

### Fase VII: Parâmetros de Customização e Marca d'Água de Teste
* **Objetivo**: Flexibilizar a geração para integradores sem violar as restrições normativas de schemas XSD.
* **Ações Executadas**:
  * Implementação da lógica de logotipo alternativo: se o parâmetro opcional `logoBytes` for preenchido, a logomarca do emitente substitui completamente a logo horizontal nacional padrão no cabeçalho.
  * Adição do parâmetro `forceTestWatermark`. Quando habilitado, o motor desenha uma marca d'água translúcida com a palavra `"TESTE"` na diagonal do documento, permitindo homologações visuais rápidas em ambiente de produção sem invalidar o campo XML de ambiente (`tpAmb`).

### Fase VIII: Reorganização de Pastas, Namespaces e Nomenclatura (Refactoring Principal)
* **Objetivo**: Corrigir a estrutura redundante de diretórios e renomear os namespaces para a padronização definitiva.
* **Ações Executadas**:
  * **Físico**: Movimentação dos arquivos de código de `src/NFSe.Componente.v2/` para a raiz `src/` e dos testes de `tests/NFSe.Componente.v2.Tests/` para `Tests/`.
  * **Projetos**: Renomeação dos arquivos `.csproj` para `NFSe.DANFSe.v2.csproj` e `NFSe.DANFSe.v2.Tests.csproj`.
  * **Solução**: Renomeação de `NFSe.Componente.v2.slnx` para `NFSe.DANFSe.v2.slnx` com atualização das referências internas de projeto.
  * **Cógido**: Atualização em lote de todos os namespaces de `NFSe.Componente.v2` para `NFSe.DANFSe.v2` (incluindo diretivas de imports).
  * **Recursos**: Atualização dos caminhos literais de manifesto do Assembly para carregar as fontes Liberation e logo PNG sob a nova estrutura lógica do projeto.
  * **Verificação**: Compilação e verificação final de testes de regressão executados via console com sucesso total.
