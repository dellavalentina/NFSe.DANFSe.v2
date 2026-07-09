# Guia de Publicação Confiável (Trusted Publishing) no NuGet.org

A **Publicação Confiável (Trusted Publishing)** é o método mais moderno e seguro recomendado pela Microsoft e pelo NuGet.org para publicar pacotes. Ele elimina a necessidade de criar, armazenar e expirar **API Keys** manuais.

Em vez disso, ele utiliza o protocolo **OpenID Connect (OIDC)** para criar uma relação de confiança federada entre o seu repositório do **GitHub** e o **NuGet.org**. Quando o seu fluxo do GitHub Actions executa, ele recebe um token temporário de vida curta (válido por apenas alguns minutos) diretamente do NuGet.org para fazer o push do pacote.

---

## Como Configurar a Publicação Confiável (Trusted Publishing)

### Passo 1: Configurar a Relação de Confiança no NuGet.org
Se este é o primeiro push do pacote, ou se você está registrando um pacote existente:

1. Acesse o [NuGet.org](https://www.nuget.org) e faça login.
2. No menu do seu perfil (canto superior direito), clique em **Portal do Editor (Publishers)** ou vá em **Manage Packages** -> **Trusted Publishing**.
3. Se você estiver registrando um novo pacote (nunca publicado antes):
   * Clique em **Register new package using Trusted Publishing**.
4. Preencha as informações da **Identidade do GitHub**:
   * **Organization/Owner**: O nome do seu usuário ou da organização no GitHub (ex: o seu usuário ou `Qualidata`).
   * **Repository**: O nome do repositório no GitHub (ex: `NFSe.DANFSe.v2`).
   * **Workflow File Name**: O nome do arquivo YAML que executará a publicação (ex: `publish.yml` ou `dotnet.yml`).
   * **Environment Name** (Opcional): Se você usa Ambientes de Deploy no GitHub (pode deixar em branco).
5. Clique em **Register**.

---

### Passo 2: Configurar as Permissões no GitHub Actions (YAML)
Para que o GitHub Actions consiga solicitar o token OIDC temporário ao NuGet.org, o job de deploy do seu arquivo `.yml` precisa de permissões de escrita de token (`id-token: write`).

Edite o seu arquivo de workflow do GitHub Actions (ex: [`.github/workflows/dotnet.yml`](file:///D:/__Projetos_Qualidata_SVN/sistemas-fontes/NotaFiscalEletronica/NFSe.DANFSe.v2/.github/workflows/dotnet.yml)) para incluir:

```yaml
name: Publicar no NuGet

on:
  release:
    types: [published] # Dispara a publicação quando um Release for publicado no GitHub

jobs:
  publish:
    runs-on: ubuntu-latest
    
    # IMPORTANTE: Permissões necessárias para OIDC (Trusted Publishing)
    permissions:
      id-token: write
      contents: read

    steps:
    - name: Checkout Repository
      uses: actions/checkout@v4

    - name: Setup .NET SDK
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: 6.0.x

    - name: Restore dependencies
      run: dotnet restore

    - name: Build project
      run: dotnet build --configuration Release --no-restore

    - name: Pack NuGet Package
      run: dotnet pack --configuration Release --no-restore -o out

    - name: Publish to NuGet.org
      # O comando dotnet nuget push detectará automaticamente as credenciais federadas do GitHub Actions
      run: dotnet nuget push out/*.nupkg --source https://api.nuget.org/v3/index.json
```

---

## Vantagens de Usar Trusted Publishing
1. **Segurança Máxima**: Nenhuma API Key ou senha secreta precisa ser gravada nos "Secrets" do GitHub. Se o seu repositório for hackeado ou vazado, não há chaves de publicação expostas.
2. **Zero Manutenção**: Chaves de API normais expiram no máximo em 365 dias, exigindo atualização manual anual. Com Trusted Publishing, a autenticação nunca expira e é renovada automaticamente.
3. **Escopo Restrito**: O token temporário é emitido apenas para aquele repositório específico e para aquela ação específica, impedindo publicações maliciosas em outros pacotes.
