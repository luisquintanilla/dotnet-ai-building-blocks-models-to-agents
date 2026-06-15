# Building-block samples

Nine tiny, runnable .NET programs. One per building block, in the order the talk
introduces them. Each is a single file you run with `dotnet run`. Read them top to
bottom: the first comment states the problem, then the code shows the smallest
thing that solves it.

| # | File | Block | Shows |
| --- | --- | --- | --- |
| 1 | `01-chat.cs` | Models | `IChatClient`, one call, swap the model |
| 2 | `02-embeddings.cs` | Data & memory | `IEmbeddingGenerator`, similarity |
| 3 | `03-rag.cs` | Data & memory | `VectorData` retrieval, then augment |
| 4 | `04-tools.cs` | Tools | function calling with `AIFunctionFactory` |
| 5 | `05-mcp.cs` | Tools | an MCP tool from the Microsoft Learn server |
| 6 | `06-middleware.cs` | Middleware | caching + OpenTelemetry in the pipeline |
| 7 | `07-eval.cs` | Evaluations | score relevance, coherence, groundedness |
| 8 | `08-agent.cs` | Agents | wrap the `IChatClient` as a `ChatClientAgent` |
| 9 | `09-multi-agent.cs` | Agents | a concurrent multi-agent workflow |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or newer. These are
  file-based apps, so there is no project file. The `#:package` lines at the top
  of each file declare their NuGet dependencies.
- A [GitHub Models](https://github.com/marketplace/models) token. Any GitHub
  personal access token with the `models:read` permission works, and the models
  are free to try. The samples read it from the `GITHUB_TOKEN` environment
  variable.

Set the token for your shell:

```powershell
# PowerShell
$env:GITHUB_TOKEN = "ghp_your_token_here"
```

```bash
# bash / zsh
export GITHUB_TOKEN="ghp_your_token_here"
```

## Run

```bash
dotnet run 01-chat.cs
```

Work through them in order. The first run restores packages and takes a moment;
later runs are quick.

## Notes

- Every sample talks to GitHub Models through the OpenAI-compatible endpoint
  `https://models.inference.ai.azure.com`. To use Azure OpenAI, OpenAI, Ollama,
  or Foundry Local instead, change the few provider lines at the top. The rest of
  each sample only ever sees `IChatClient` or `IEmbeddingGenerator`, so it keeps
  working.
- `03-rag.cs` uses the in-memory vector store. The store ships in a connector
  package; your code targets the `Microsoft.Extensions.VectorData` abstraction,
  so swapping to SQLite, Qdrant, Azure AI Search, or Redis is a connector change,
  not a code change.
- `07-eval.cs` uses the same model as both the assistant and the judge to keep
  things simple. In real use, pick a strong judge model and run the evaluators in
  your test project and CI.
- Package versions are pinned in each file's `#:package` lines. If a version no
  longer resolves, bump it to the current release on
  [nuget.org](https://www.nuget.org).
