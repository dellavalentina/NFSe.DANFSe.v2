# Guia de Publicação no NuGet.org

Este guia fornece orientações passo a passo para configurar, empacotar e publicar a biblioteca **NFSe.DANFSe.v2** no repositório público e oficial do ecossistema .NET, o **NuGet.org**.

---

## Passo 1: Criar uma Conta no NuGet.org
1. Acesse o site oficial: [https://www.nuget.org](https://www.nuget.org).
2. Clique em **Sign in** no canto superior direito.
3. Faça login utilizando uma conta da Microsoft (pessoal ou corporativa).
4. Configure o seu nome de usuário (ex: `Qualidata` ou `AdrianoDella`).

---

## Passo 2: Gerar a Chave de API (API Key)
A API Key é um token de autenticação que permite que o seu terminal envie o pacote para o servidor do NuGet de forma segura.

1. No NuGet.org, clique no seu nome de perfil no canto superior direito e selecione **API Keys**.
2. Clique em **+ Create**.
3. Preencha os campos conforme as instruções abaixo:
   * **Key Name**: `NFSe.DANFSe.v2 Publish Key` (ou um nome de sua preferência).
   * **Expires in**: Escolha o período de validade (ex: `365 days` para automações duradouras ou `90 days` por segurança).
   * **Scopes**: Marcar a opção **Push** -> *Push new packages and package versions*.
   * **Glob Pattern**: Digite exatamente `NFSe.DANFSe.v2` (isso limita o uso desta chave apenas para este pacote).
4. Clique em **Create**.
5. Na lista de chaves geradas, localize a chave criada e clique em **Copy**.
6. **IMPORTANTE**: Salve essa chave imediatamente em um local seguro (como um gerenciador de senhas). Ela não será exibida novamente por motivos de segurança do NuGet.

---

## Passo 3: Configurar os Metadados do Projeto (`.csproj`)
Antes de empacotar, garanta que o arquivo [`src/NFSe.DANFSe.v2.csproj`](file:///D:/__Projetos_Qualidata_SVN/sistemas-fontes/NotaFiscalEletronica/NFSe.Componente.v2/src/NFSe.DANFSe.v2.csproj) contenha metadados profissionais mínimos que serão exibidos na página do pacote.

Recomendamos adicionar as seguintes tags no grupo principal `<PropertyGroup>`:
```xml
  <PropertyGroup>
    ...
    <Version>0.1.0</Version>
    <Authors>Adriano Della</Authors>
    <PackageId>NFSe.DANFSe.v2</PackageId>
    <Description>Componente .NET para geração de DANFSe em PDF (Nota Técnica 008)</Description>
    <PackageLicenseExpression>BSD-3-Clause</PackageLicenseExpression>
    <PackageTags>nfse;danfse;pdf;sped;nota-fiscal;danfe</PackageTags>
    <RepositoryType>git</RepositoryType>
    <!-- Substitua pela URL final do seu repositório quando publicar no GitHub -->
    <!-- <RepositoryUrl>https://github.com/usuario/NFSe.DANFSe.v2</RepositoryUrl> -->
  </PropertyGroup>
```

---

## Passo 4: Gerar o Arquivo de Distribuição (`.nupkg`)
O arquivo `.nupkg` (NuGet Package) é um pacote compactado contendo a DLL compilada da biblioteca e os arquivos de metadados.

1. Abra o seu console/terminal na raiz do projeto (`D:\__Projetos_Qualidata_SVN\sistemas-fontes\NotaFiscalEletronica\NFSe.DANFSe.v2`).
2. Execute o comando de compilação em modo de produção (Release):
   ```bash
   dotnet pack --configuration Release
   ```
3. O compilador gerará o arquivo compilado em:
   `src/bin/Release/NFSe.DANFSe.v2.0.1.0.nupkg`

---

## Passo 5: Fazer o Upload (Publish) do Pacote
1. No terminal, execute o seguinte comando, substituindo o texto `SUA_API_KEY_COPIADA` pela chave que você gerou no **Passo 2**:
   ```bash
   dotnet nuget push src/bin/Release/*.nupkg --api-key SUA_API_KEY_COPIADA --source https://api.nuget.org/v3/index.json
   ```
2. O terminal exibirá mensagens de progresso de upload. Se tudo estiver correto, você verá a mensagem `Your package was pushed successfully`.

---

## Passo 6: Validação e Indexação do Pacote
* Após o upload, o NuGet.org realiza uma validação automatizada de segurança (varredura de vírus e verificação de assinatura digital da DLL).
* Esse processo costuma demorar entre **5 a 15 minutos**.
* Você receberá um e-mail do NuGet confirmando que o pacote foi indexado e está público.
* A partir deste momento, o comando de instalação oficial estará disponível para uso em qualquer projeto .NET:
  ```bash
  dotnet add package NFSe.DANFSe.v2
  ```
