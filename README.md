# Projeto Crawler de Mídias Sociais (.NET 8)

Este projeto consiste em uma arquitetura de back-end em .NET 8 projetada para coletar e armazenar dados de perfis de mídias sociais, utilizando a plataforma Apify para o crawling.

A arquitetura é dividida em dois processos principais:
-   Uma **API Web** para interações sob demanda (ex: cadastrar um novo perfil, disparar um crawl manualmente).
-   Um **Worker Service** para tarefas agendadas em segundo plano (ex: atualizar os dados de todos os perfis cadastrados a cada X minutos).

## Arquitetura

O sistema segue os princípios de Arquitetura Limpa (Clean Architecture), separando as responsabilidades em diferentes camadas e projetos.

```
                                                                        ┌───────────────────┐
[ Front-End (futuro) ] ---> [ Crawler.Api ] -----------------------+    |                   |
                                                                  |--> [ Crawler.Application ] --> [ Crawler.Infrastructure ] --> [ MongoDB / Apify ]
[ Agendador (Timer)  ] ---> [ Crawler.Worker ] --------------------+    |      (Core)       |
                                                                        └───────────────────┘
```

-   **Crawler.Api / Crawler.Worker**: Pontos de entrada da aplicação.
-   **Crawler.Application**: Orquestra os casos de uso e a lógica de negócio.
-   **Crawler.Core**: Contém as entidades e interfaces, o coração da aplicação.
-   **Crawler.Infrastructure**: Contém as implementações concretas de acesso a dados e serviços externos.

## Pré-requisitos

Para rodar este projeto, você precisará de:

-   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
-   [Visual Studio 2022](https://visualstudio.microsoft.com/vs/community/) (ou VS Code com a extensão C# Dev Kit)
-   Uma conta na [Apify](https://apify.com/) para obter um API Token.
-   Uma conta no [MongoDB Atlas](https://www.mongodb.com/cloud/atlas) para criar um cluster gratuito e obter uma Connection String.

## Configuração

O projeto utiliza o sistema de configuração do .NET, com `appsettings.json` para configurações não-sensíveis e o **User Secrets** para dados sensíveis.

**⚠️ Importante:** Nunca coloque tokens, senhas ou connection strings diretamente nos arquivos `appsettings.json` que são enviados para o repositório Git.

### Configurando os Segredos (User Secrets)

Você precisa configurar os segredos para os projetos `Crawler.Api` e `Crawler.Worker`.

1.  No Gerenciador de Soluções do Visual Studio, clique com o botão direito sobre o projeto (ex: `Crawler.Worker`).
2.  Selecione **"Gerenciar Segredos do Usuário"** (*Manage User Secrets*).
3.  Um arquivo `secrets.json` será aberto. Cole o conteúdo abaixo e preencha com seus dados.

**Para o projeto `Crawler.Worker` (secrets.json):**
```json
{
  "ApifySettings": {
    "ApiToken": "SEU_TOKEN_REAL_DA_APIFY"
  },
  "MongoDbSettings": {
    "ConnectionString": "SUA_CONNECTION_STRING_REAL_DO_MONGODB_ATLAS"
  }
}
```

**Para o projeto `Crawler.Api` (secrets.json):**
*Faça o mesmo processo. O conteúdo é idêntico.*
```json
{
  "ApifySettings": {
    "ApiToken": "SEU_TOKEN_REAL_DA_APIFY"
  },
  "MongoDbSettings": {
    "ConnectionString": "SUA_CONNECTION_STRING_REAL_DO_MONGODB_ATLAS"
  }
}
```

### Arquivos `appsettings.json`

Verifique se os arquivos `appsettings.json` de cada projeto contêm a estrutura de configuração necessária.

**`Crawler.Worker/appsettings.json`:**
```json
{
  "Logging": { ... },
  "ApifySettings": {
    "BaseUrl": "[https://api.apify.com/v2](https://api.apify.com/v2)",
    "ApiToken": "",
    "ActorId": "shu8hvrXbJbY3Eb9W"
  },
  "MongoDbSettings": {
    "DatabaseName": "CrawlerDb",
    "PostsCollectionName": "InstagramPosts",
    "TargetsCollectionName": "Targets",
    "ConnectionString": ""
  },
  "InstagramJob": {
    "IsEnabled": true,
    "CronExpression": "0 */5 * * * *"
  }
}
```

**`Crawler.Api/appsettings.json`:**
```json
{
  "Logging": { ... },
  "AllowedHosts": "*",
  "ApifySettings": {
    "BaseUrl": "[https://api.apify.com/v2](https://api.apify.com/v2)",
    "ApiToken": "",
    "ActorId": "shu8hvrXbJbY3Eb9W"
  },
  "MongoDbSettings": {
    "DatabaseName": "CrawlerDb",
    "PostsCollectionName": "InstagramPosts",
    "TargetsCollectionName": "Targets",
    "ConnectionString": ""
  },
  "ApplicationSettings": {
    "PublicBaseUrl": "" // Preenchido em appsettings.Development.json
  }
}
```

## Como Rodar Localmente

Para testar a aplicação completa, é recomendado rodar a API e o Worker ao mesmo tempo.

1.  No Gerenciador de Soluções, clique com o botão direito na **Solução**.
2.  Selecione **"Definir Projetos de Inicialização..."**.
3.  Marque a opção **"Vários projetos de inicialização"**.
4.  Defina a "Ação" como **"Iniciar"** para os projetos `Crawler.Api` e `Crawler.Worker`.
5.  Clique em "Aplicar" e "OK".
6.  Para testar a API (que usa webhooks), selecione o perfil de inicialização **Dev Tunnels** na barra de ferramentas do Visual Studio.
7.  Clique no botão de play (▶️) para iniciar.

## Endpoints Principais da API

-   `POST /api/targets/instagram`: Cadastra um novo perfil de Instagram para ser monitorado. Dispara o crawl inicial e retorna `200 OK` com os dados existentes se o perfil já existir.
-   `GET /api/posts/{username}`: Retorna os posts já salvos no banco de dados para um determinado usuário.
-   `POST /api/crawling/webhook-receiver`: Endpoint interno para receber as notificações da Apify quando um crawl é concluído.

---
Este projeto é uma Prova de Conceito (PoC) para um sistema de crawling robusto e escalável.
