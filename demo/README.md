# ChatApp demo

This is the baseline app for the talk. It is the production .NET AI Chat Web App template,
scaffolded with Aspire into `demo/ChatApp`. The "everything together" slide points at this app.

An enhanced version lives on the `advanced-demo` branch. Same app, two blocks swapped: layout-aware
ingestion and better retrieval. The `git diff main..advanced-demo` is the point of that demo.

## Template

- Package: `Microsoft.Extensions.AI.Templates` (`10.7.0-preview.3.26309.5`)
- Short name: `aichatweb`

## How it was scaffolded

From the repo root:

```powershell
dotnet new install Microsoft.Extensions.AI.Templates
dotnet new aichatweb -o demo\ChatApp --provider githubmodels --vector-store local --aspire
```

Provider: GitHub Models. Vector store: local JSON file on disk. `--aspire` makes it a distributed
app, so you get the Aspire dashboard for traces and logs, which is where the telemetry from the
middleware block shows up.

## What's in it

| Project | What it is |
| --- | --- |
| `ChatApp.AppHost` | The Aspire app host. Wires up the web app, the model connection, and a `markitdown` container for document conversion. |
| `ChatApp.Web` | The Blazor chat UI, the `IChatClient` and embeddings wiring, ingestion, and the vector store. |
| `ChatApp.ServiceDefaults` | Shared Aspire defaults: OpenTelemetry, health checks, service discovery. |

The two blocks the `advanced-demo` branch swaps live in `ChatApp.Web/Services`:

- `Services/Ingestion/` (`DataIngestor.cs`, `DocumentReader.cs`) — the ingestion pipeline.
- `Services/SemanticSearch.cs` — retrieval over the vector store.

## Prerequisites

- The [.NET 10 SDK](https://dotnet.microsoft.com/download).
- [Docker](https://www.docker.com/products/docker-desktop/) running. The app host starts a
  `markitdown` container to convert documents during ingestion.
- A [GitHub Models](https://github.com/marketplace/models) token (a GitHub PAT with `models:read`).

## Configure the model

Set the connection string as a user secret on the **app host** project:

```powershell
cd demo\ChatApp\ChatApp.AppHost
dotnet user-secrets set ConnectionStrings:openai "Endpoint=https://models.inference.ai.azure.com;Key=YOUR-GITHUB-TOKEN"
```

The web app reads that connection through `AddAzureOpenAIClient("openai")` (the GitHub Models
endpoint speaks the OpenAI protocol). The chat model is `gpt-4o-mini`; embeddings are
`text-embedding-3-small`.

## Build

```powershell
dotnet build demo\ChatApp\ChatApp.sln
```

## Run

```powershell
dotnet run --project demo\ChatApp\ChatApp.AppHost
```

The Aspire dashboard opens. From there, launch the web app, drop a PDF into the chat, and ask a
grounded question. The sample documents in `ChatApp.Web/wwwroot/Data` are ingested on first run.
