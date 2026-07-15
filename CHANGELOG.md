# Changelog - NFSe.DANFSe.v2

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