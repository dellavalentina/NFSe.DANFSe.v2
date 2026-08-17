# Changelog - NFSe.DANFSe.v2

## [0.2.3] - 2026-08-17
### Corrigido / Refatorado
- **Adequação ao DANFSe Oficial do Emissor Nacional (Anexo B)**:
  - **Exclusões e Reduções da Base de Cálculo (IBS/CBS)**: Atualizado o renderer para calcular e exibir o somatório real das exclusões (`vDescIncond + vCalcReeRepRes + vISSQN + vPIS + vCOFINS`), exibindo `R$ 109,88` quando houver abatimento do ISSQN.
  - **Formatação de Alíquotas e NBS**: Adicionados helpers `FormatPercent` (formatação com vírgula e `%`) e `FormatNbs` (máscara `x.xxxx.xx.xx`).
  - **Reduções de Alíquota IBS/CBS**: Exibição do padrão `- / - / -` quando sem reduções.
  - **Benefício Municipal**: Ajustada a descrição para `"Redução por valor monetário"` na versão 1.01.
  - **Lei nº 12.741/2012**: Ajustado o rodapé para exibição em porcentagem (`Federais: 0,00 %; Estaduais: 0,00 %; Municipais: 5,00 %;`).

## [0.2.2] - 2026-08-04
### Refatorado / Corrigido
- **Ajuste na assinatura de `GeneratePdf`**: O parâmetro `autoFixInconsistentTotal` foi alterado de volta para `bool?` para permitir maior flexibilidade caso o integrador passe um valor nulo da sua fonte de configuração (tratando `null` como `false` internamente).
- **Correção de Warnings de Nullability**: Corrigido o tipo do parâmetro `logoBytes` para `byte[]?` resolvendo warnings de compilador quando valores nulos eram passados nos testes.

## [0.2.1] - 2026-08-04
### Removido / Refatorado
- **Remoção de `DanfseConfig`**: Excluída a classe de configuração global `DanfseConfig` e a lógica de leitura dinâmica de arquivos `App.config` / `Web.config`.
- **Refatoração do Parâmetro de Totais**: O parâmetro `autoFixInconsistentTotal` em `DanfsePdfRenderer.GeneratePdf` foi simplificado para um tipo `bool` com padrão `false`. A decisão de como ler e repassar a configuração agora cabe inteiramente ao desenvolvedor que consome a biblioteca, mantendo o motor de renderização PDF totalmente stateless.

## [0.2.0] - 2026-08-04
### Adicionado
- **Suporte ao .NET Framework 4.8**: Adicionado target framework `net48` em conjunto com `net6.0` e `netstandard2.0` no projeto para facilitar a integração nativa em projetos legados (.NET Framework).
- **Compilação Condicional em `DanfseConfig`**: Isolado o namespace `System.Configuration` e o método de leitura direta do `ConfigurationManager` usando a diretiva `#if NETFRAMEWORK`, prevenindo exceções de JIT em ambientes modernos (.NET Core/.NET 6+) que não possuem a referência em tempo de execução.

## [0.1.5] - 2026-08-03
### Adicionado
- **Configuração Global `DanfseConfig`**: Criada classe estática de configuração com suporte automático a `App.config` / `Web.config` (`System.Configuration.ConfigurationManager`) para a propriedade `AutoFixInconsistentTotal`.
- **Parsing de `CST` / `cClassTrib` e `finNFSe`**: Mapeamento completo dos nós `<trib>/<gIBSCBS>` e `<finNFSe>` nos elementos `<IBSCBS>` de `infNFSe` e `infDPS`.

### Corrigido
- **Mapeamento Estrito dos Schemas XSD (v1.00 e v1.01)**:
  - **`SITUAÇÃO DA NFS-E`**: Mapeamento exclusivo a partir de `<cStat>` (`TStat` do XSD: `100 - NFS-e Gerada`, `101`, `102`, `103`, `107`).
  - **`FINALIDADE`**: Mapeamento exclusivo a partir de `<finNFSe>` (`TSRTCFinNFSe` do XSD: `0 - NFS-e regular`; vazio no schema v1.00).
- **Formatadores**: Criados `FormatCstCClassTrib`, `FormatFinalidadeNfse` e atualizado `FormatSituacaoNfse` sem interferência indevida de parâmetros inferidos.
- **Parsing de IBSCBS (Mesclagem infNFSe + infDPS)**: Reestruturada a extração do nó `<IBSCBS>` para combinar de forma resiliente os dados calculados da nota com os dados de declaração da DPS.

## [0.1.4] - 2026-08-03
### Adicionado
- **Reforma Tributária / Resiliência de Totais (`autoFixInconsistentTotal`)**: Adicionada opção configurável e algoritmo de verificação inteligente anti-duplicidade em `DanfsePdfRenderer.GeneratePdf`. Quando ativada, se o XML da SEFIN Nacional omitir o imposto cobrado por fora na tag `<vTotNF>` (igualando-o ao valor líquido `vLiq`), o DANFSe exibe o valor total corrigido (`vLiq + vIBS + vCBS`). Se o XML já estiver correto, o valor original é mantido sem nenhuma duplicidade de cálculo.
- **Model / Parser — `tpEmit`**: Adicionada propriedade `TpEmit` em `DpsData` e suporte no `DanfseXmlParser` para leitura da tag `<tpEmit>` da DPS.
- **Formatters — `FormatTpEmit` & `FormatIbsCbsIndOp`**: Adicionados formatadores para conversão por extenso de emitente (`"1 - Prestador"`, `"2 - Tomador"`, `"3 - Intermediário"`) e concatenação completa de `cIndOp` / `cLocalidadeIncid` no bloco IBS/CBS.

### Corrigido
- **Renderização — Bloco Emitente da NFS-e**: Corrigida omissão visual do valor da célula `EMITENTE DA NFS-E` no cabeçalho do documento auxiliar.
- **Renderização — Indicador de País (ISO BR)**: Atualizada a formatação dos locais de prestação e incidência do ISSQN para incluir o código de país ISO (`BR`) nas operações em território nacional, conforme os exemplos da NT-008.
- **Testes Unitários**: Adicionados testes unitários e de integração cobrindo os novos formatadores e as 3 variações de cálculo da flag `autoFixInconsistentTotal`.

## [0.1.3] - 2026-07-15
### Corrigido
- **Parser — Endereço do Tomador/Destinatário/Intermediário**: `ParseParty()` agora reconhece corretamente a estrutura `<end>/<endNac>` utilizada pelo DPS (além da estrutura `<enderNac>` direta do `infNFSe`). Os campos de logradouro (`xLgr`, `nro`, `xCpl`, `xBairro`) estão em `<end>`, enquanto `cMun` e `CEP` estão em `<end>/<endNac>`. Anteriormente, nenhum dado de endereço do Tomador era lido.
- **Parser — Conformidade com XML Schema (case-sensitive)**: Auditoria completa dos nomes de elementos lidos pelo parser contra os XSDs oficiais da NFS-e v1.01 (`tiposComplexos_v1.01.xsd`, `tiposEventos_v1.01.xsd`). Corrigidas as seguintes inconsistências:
  - `tribFed.Element("pisCofins")` → `tribFed.Element("piscofins")` (conforme `TCTribOutrosPisCofins` no schema).
  - Removida leitura de `xMun` em `<endNac>` (`TCEnderNac` define apenas `cMun` e `CEP`).
  - Removida leitura de `xMun` em `<enderNac>` (`TCEnderecoEmitente` não possui campo `xMun`).
  - Removida leitura de `xMun` e `CEP` em `<locPrest>` (`TCLocPrest` define apenas `cLocPrestacao` ou `cPaisPrestacao` em `xs:choice`).
  - Removida leitura de `art` em `<obra>` (`TCInfoObra` não possui campo `art` em nenhuma versão do schema).
- **Parser — Tributação Federal (PIS/COFINS)**: Adicionada leitura dos campos `vBCPisCofins`, `pAliqPis` e `pAliqCofins` do elemento `<piscofins>` dentro de `<tribFed>`.
- **Modelo — `ServicoData`**: Adicionadas propriedades `VBCPisCofins`, `PAliqPis` e `PAliqCofins`.
- **Formatters — `FormatRetPisCofins`**: Adicionado mapeamento do código `"0"` (Não Retido) e revisados os mapeamentos dos códigos `"1"` e `"2"` conforme NT NFS-e v1.01.

### Adicionado
- **Renderização — Seção Tributação Federal**: Nova linha exibindo Base de Cálculo PIS/COFINS, Alíquota PIS e Alíquota COFINS. O bloco federal passa a ter 3 linhas de dados (altura ajustada de 1,30 cm para 1,95 cm).
- **Renderização — Local da Prestação**: Melhoria na exibição de códigos de localidade não resolvíveis como IBGE (ex.: código de localidade estrangeira): exibe `Cód. XXXXXXXX` com UF derivada do prefixo quando aplicável, e País quando não for Brasil.
- **Layout — `DanfseLayoutRegistry`**: Adicionados campos `Fed.VBCPisCofins`, `Fed.AliqPis` e `Fed.AliqCofins` na seção de Tributação Federal. Campos `Fed.IRRF`, `Fed.CP`, `Fed.CSLL`, `Fed.PIS`, `Fed.COFINS` e `Fed.RetPisCofins` reposicionados para acomodar a nova linha.

## [0.1.2] - 2026-07-09
### Alterado
- Ajuste na terminologia do projeto em arquivos de documentação (README.md), configurações de projeto (.csproj) e logs internos.

## [0.1.1] - 2026-07-09
### Alterado
- Ajuste na formatação de elementos do tipo data/hora (dhProc e dhEmi) e data (dCompet) do XML Schema para o padrão brasileiro (PT-BR) na renderização do DANFSe.

## [0.1.0] - 2026-07-08
### Adicionado
- Implementação completa do DANFSe nacional em conformidade estrita com a Nota Técnica SE/CGNFS-e nº 008.