# NFSe.DANFSe.v2

[![Build & Test](https://img.shields.io/badge/.NET-6.0-blue.svg)](https://dotnet.microsoft.com/download)
[![Library](https://img.shields.io/badge/PDFsharp-6.1-orange.svg)](https://www.pdfsharp.com)
[![License](https://img.shields.io/badge/License-BSD_3--Clause-blue.svg)](https://opensource.org/licenses/BSD-3-Clause)
[![Version](https://img.shields.io/badge/Version-0.1.1-green.svg)](#)

Componente em **.NET 6 / C#** para geração do **DANFSe (Documento Auxiliar da Nota Fiscal de Serviço Eletrônica)** em formato PDF. Este componente foi desenvolvido em conformidade estrita com as especificações visuais e regras de negócio da **Nota Técnica SE/CGNFS-e nº 008**.

---

## ⚠️ Status do Projeto

* **Os PDF's dos DANFSe's resultantes ainda não foram comparados com documentos reais gerados pela RFB, pois o formato ainda não está disponível para download/consulta.**

## 💡 Motivação

A partir de **15 de julho de 2026**, a RFB deixará de disponibilizar o DANFSe via API, tornando o desenho e geração do DANFSe de responsabilidade do emissor.

Este pacote efetua a montagem visual da nota sem depender de motores baseados em HTML, gerando o arquivo com coordenadas usando o PDFsharp.

## ✨ Características

* **Sem dependência de binários externos:** Geração baseada diretamente no PDFsharp 6.x.
* **Conformidade Estrita com a Nota Técnica 008:** Respeito rigoroso às fontes, margens, espessuras de bordas e coordenadas exigidas no padrão nacional.
* **Logotipo Customizado:** Permite enviar a logomarca personalizada do prestador que substitui a marca oficial da NFS-e no cabeçalho respeitando a área delimitada de `4,00 cm x 1,16 cm`.
* **Marca d'água "TESTE":** Parâmetro para forçar marca d'água diagonal de teste para homologações visuais internas.
* **Stateless:** A entrada (XML) e a saída (PDF) são tratadas inteiramente em memória (`byte[]`).

---

## 📁 Estrutura do Projeto

```text
NFSe.DANFSe.v2/
│
├── .github/workflows/                     # Fluxos de Integração Contínua (GitHub Actions)
│   └── dotnet.yml                         # Pipeline de build e testes automáticos do CI
│
├── src/                                   # Projeto Principal (Class Library net6.0)
│   ├── Helpers/                           # Formatadores, Resolvers e Utils
│   │   ├── Formatters.cs                  # Máscaras de CPF, CNPJ, Telefone, CEP e Enums da NT
│   │   ├── IbgeResolver.cs                # Resolução de Municípios e UFs baseado em código IBGE
│   │   └── EmbeddedFontResolver.cs        # Registro de fontes customizadas no PDFsharp 6.x
│   ├── Models/                            # Modelo de dados tipado correspondente ao XML
│   │   ├── DanfseModel.cs                 # Estrutura de dados populada a partir da NFS-e
│   │   └── NfseSchemaValues.cs            # Valores constantes do Schema Nacional da NFS-e
│   ├── Rendering/                         # Motor de Renderização PDF
│   │   ├── DanfsePdfRenderer.cs           # Geração, desenho do grid e aplicação de lógicas de layout
│   │   ├── DanfseLayoutRegistry.cs        # Registro centralizado de coordenadas e metadados dos campos do DANFE
│   │   └── QrCodeVectorDrawer.cs          # Desenho vetorial de QR Code
│   └── Resources/                         # Recursos embutidos
│       ├── Fonts/                         # Fontes LiberationSans (Regular, Bold, Italic)
│       └── Images/                        # Logomarca NFS-e nacional horizontal
│
├── Tests/                                 # Projeto de Testes Unitários (XUnit net6.0)
│   ├── Samples/                           # Arquivos XML e logos fictícios de exemplo (higienizados)
│   │   ├── danfse-normal.xml              # XML higienizado com dados de teste
│   │   ├── danfse-cancelamento.xml        # XML higienizado com evento de cancelamento
│   │   ├── danfse-substituida.xml         # XML higienizado com evento de substituição
│   │   └── logo-alternative.png           # Logotipo abstrato gerado para testes locais
│   ├── XmlParserTests.cs                  # Testes do leitor e processador do XML da NFS-e
│   └── DanfseGenerationTests.cs           # Testes de geração do PDF a partir dos Samples relativos
│
└── NFSe.DANFSe.v2.slnx                    # Arquivo de Solução Visual Studio (formato moderno)
```

---

## 🛠️ Como Instalar e Configurar

### Instalação via NuGet
Para adicionar o componente ao seu projeto, execute o comando abaixo no terminal do seu projeto:
```bash
dotnet add package NFSe.DANFSe.v2
```

### Compilação a partir do Código-Fonte (Opcional)
Se preferir compilar manualmente a partir do código-fonte, execute:
```bash
dotnet build
```

---

## ⚡ Exemplo de Utilização

O componente recebe como entrada o modelo populado `DanfseModel`, o array de bytes da logomarca do prestador (opcional) e uma flag para forçar marca d'água de teste (opcional):

```csharp
using System.IO;
using NFSe.DANFSe.v2.Models;
using NFSe.DANFSe.v2.Rendering;

// 1. Instanciar ou desserializar os dados do XML no DanfseModel
DanfseModel model = CarregarDadosDoXml("danfse-normal.xml");

// 2. Opcional: Ler logotipo customizado do prestador (se informado, substitui a logo padrão)
byte[] customLogoBytes = File.ReadAllBytes("logo-prestador.png");

// 3. Gerar o DANFSe em PDF
byte[] pdfBytes = DanfsePdfRenderer.GeneratePdf(model, customLogoBytes, forceTestWatermark: false);

// 4. Salvar ou transmitir o array de bytes do PDF
File.WriteAllBytes("DANFSe_Gerado.pdf", pdfBytes);
```

---

## 🧪 Rodando os Testes Unitários

O projeto de testes unitários lê os arquivos XML e de imagens fictícios de amostra fornecidos no diretório `Tests/Samples` por meio de caminhos relativos ao executável e gera os respectivos PDFs de verificação dentro do diretório `Output/` (na raiz do projeto) para análise visual.

Execute o comando abaixo:
```bash
dotnet test
```

Os seguintes cenários de DANFSe são validados:
1. **DANFSe Normal** com todos os blocos preenchidos. 🚩
2. **DANFSe Normal com Logomarca Personalizada** (validação uso de logomarca personalizada). 🚩
3. **DANFSe sem Tomador/Destinatário/Intermediário** (validação das linhas mínimas de `0.40 cm` e textos centralizados).
4. **DANFSe Cancelado** (validação da marca d'água `CANCELADA`).
5. **DANFSe Substituído** (validação da marca d'água `SUBSTITUÍDA`).
6. **DANFSe em Homologação** (validação do cabeçalho em vermelho `NFS-e SEM VALIDADE JURÍDICA`).

🚩 Utilizando o parâmetro `forceTestWatermark: true` para imprimir a palavra `TESTE`.

---

## 📝 Especificações Técnicas Importantes

* **Layout Orientado a Metadados (`DanfseLayoutRegistry`)**: Todas as coordenadas (X, Y), dimensões (largura, altura), alinhamentos de texto e rótulos dos campos do DANFE são definidos declarativamente na classe `DanfseLayoutRegistry`. O renderizador consome esses metadados via função auxiliar `DrawMetadataField`, o que desacopla a lógica visual do posicionamento físico e facilita ajustes finos futuros.
* **Desenho por Coordenadas Dinâmicas**: Diferente de soluções rígidas por grid estático, a classe `DanfsePdfRenderer` calcula as posições `Y` dinamicamente conforme os blocos anteriores aumentam, diminuem ou são omitidos (como Tomador ou Tributação Municipal).
* **Parser de XML com Namespaces Dinâmicos**: A leitura de tags em `DanfseXmlParser.cs` é realizada por meio de namespaces resolvidos dinamicamente em tempo de execução, tornando o parser altamente resiliente a variações de versão de schemas da NFS-e (v1.00, v1.01, etc.).
* **Corretude de Buffer no PDFsharp**: Para carregar imagens a partir de streams usando o PDFsharp 6, o stream de bytes deve ser instanciado de forma pública:
  ```csharp
  new MemoryStream(logoBytes, 0, logoBytes.Length, false, true); // publiclyVisible = true
  ```
  Isso evita problemas de `UnauthorizedAccessException` nas APIs internas do PDFsharp ao acessar buffers de imagem em nuvem ou ambiente web.

## 📄 Licença

Este projeto é distribuído sob a licença BSD 3-Clause. Veja o arquivo `LICENSE.txt` para mais detalhes.