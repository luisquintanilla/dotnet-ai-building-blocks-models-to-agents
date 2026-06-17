# ChatApp demo

This is the `advanced-demo` branch of the ChatApp demo. It starts from the production .NET AI Chat
Web App template, scaffolded with Aspire into `demo/ChatApp`, then upgrades two blocks with the real
RAG building blocks: layout-aware ingestion (PdfPig vision OCR) and a real retrieval pipeline (query
expansion, reranking, and CRAG).

The default template app lives on `main`. `git diff main..advanced-demo` is the "upgrade, not rewrite"
point of the demo: the packages and the wiring change, the Blazor UI and the app shape don't.

## Template

- Package: `Microsoft.Extensions.AI.Templates` (`10.7.0-preview.3.26309.5`)
- Short name: `aichatweb`

## How it was scaffolded

From the repo root:

```powershell
dotnet new install Microsoft.Extensions.AI.Templates
dotnet new aichatweb -o demo\ChatApp --provider githubmodels --vector-store local --aspire
```

Scaffolded against GitHub Models, then re-pointed at **Azure OpenAI** (keyless). The template is
provider-agnostic: only the `openai` connection string and the deployment names changed. The default
template stores vectors in a local JSON file; this branch swaps that for **SqliteVec** (a local
SQLite database file, `vector-store.db`), so the ingested chunks persist between runs. `--aspire`
makes it a distributed app, so you get the Aspire dashboard for traces and logs, which is where the
telemetry from the middleware block shows up.

## What's in it

| Project | What it is |
| --- | --- |
| `ChatApp.AppHost` | The Aspire app host. Wires up the web app and the Azure OpenAI connection. The default template also starts a `markitdown` container for document conversion; this branch reads PDFs with PdfPig vision OCR instead, so no container runs here. |
| `ChatApp.Web` | The Blazor chat UI, the `IChatClient` and embeddings wiring, ingestion, and the vector store. |
| `ChatApp.ServiceDefaults` | Shared Aspire defaults: OpenTelemetry, health checks, service discovery. |

The two blocks the `advanced-demo` branch swaps live in `ChatApp.Web/Services`:

- `Services/Ingestion/` (`DataIngestor.cs`, `DocumentReader.cs`): the ingestion pipeline.
- `Services/SemanticSearch.cs`: retrieval over the vector store.

## Prerequisites

- The [.NET 10 SDK](https://dotnet.microsoft.com/download).
- The [Aspire CLI](https://aspire.dev): `dotnet tool install -g Aspire.Cli`.
- An [Azure OpenAI](https://learn.microsoft.com/azure/ai-services/openai/) resource with a
  **vision-capable** chat deployment named `gpt-5-mini` and an embeddings deployment named
  `embedding`. The chat model must accept image input, because ingestion renders each PDF page to
  an image and uses the chat model for OCR.
- The Azure CLI, signed in with `az login`. Your identity needs the **Cognitive Services OpenAI
  User** role on the resource (keyless, no API key).

## Configure the model (keyless)

Set the **endpoint only** as a user secret on the **app host** project. An endpoint with no key
tells the client to authenticate with `DefaultAzureCredential` (your `az login`), so no API key is
ever stored:

```powershell
cd demo\ChatApp\ChatApp.AppHost
dotnet user-secrets set ConnectionStrings:openai "https://<your-resource>.openai.azure.com/"
```

The web app reads that connection through `AddAzureOpenAIClient("openai")`. It uses the chat
deployment named `gpt-5-mini` and the embeddings deployment named `embedding`. Rename these in
`ChatApp.Web/Program.cs` if your deployments differ.

## Build

```powershell
dotnet build demo\ChatApp\ChatApp.sln
```

## Run

Sign in first so the keyless credential can get a token:

```powershell
az login
```

Then start the app with the Aspire CLI (it finds the app host automatically):

```powershell
cd demo\ChatApp
aspire run
```

The Aspire dashboard opens. From there, launch the web app, drop a PDF into the chat, and ask a
grounded question. The sample documents in `ChatApp.Web/wwwroot/Data` are ingested on first run.
That first ingest runs vision OCR over every PDF page through the chat model, so it is slow. The
SqliteVec store persists it to disk, so later runs just query. Delete `vector-store.db` to force a
re-ingest after you change the source documents.
