# Building-block samples

Eleven tiny, runnable .NET programs. One per building block, in the order the talk
introduces them. Each is a single file you run with `dotnet run`. Read them top to
bottom: the first comment states the problem, then the code shows the smallest
thing that solves it.

| # | File | Block | Shows |
| --- | --- | --- | --- |
| 1 | `01-chat.cs` | Models | `IChatClient`, one call, swap the model |
| 1b | `01b-ollama.cs` | Models | same code, swap the provider to local Ollama |
| 2 | `02-vision.cs` | Models | image input to `IChatClient` (multimodal) |
| 3 | `03-image-generation.cs` | Models | `IImageGenerator`, generate an image |
| 4 | `04-embeddings.cs` | Data & memory | `IEmbeddingGenerator`, similarity |
| 5 | `05-rag.cs` | Data & memory | `VectorData` retrieval, then augment |
| 6 | `06-tools.cs` | Tools | function calling with `AIFunctionFactory` |
| 7 | `07-mcp.cs` | Tools | an MCP tool from the Microsoft Learn server |
| 8 | `08-middleware.cs` | Middleware | caching + OpenTelemetry in the pipeline |
| 9 | `09-eval.cs` | Evaluations | score relevance, coherence, groundedness |
| 10 | `10-agent.cs` | Agents | wrap the `IChatClient` as a `ChatClientAgent` |
| 11 | `11-multi-agent.cs` | Agents | a concurrent multi-agent workflow |

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
  each sample only ever sees an abstraction like `IChatClient`, `IImageGenerator`,
  or `IEmbeddingGenerator`, so it keeps working.
- `02-vision.cs` runs on GitHub Models because `gpt-4.1-mini` is vision-capable.
  It downloads the image and passes the bytes inline, so the model service never
  has to fetch a URL.
- `01b-ollama.cs` proves the provider swap end to end against a model running on
  your machine. The only change from `01-chat.cs` is the client you construct: an
  `OllamaApiClient` (from `OllamaSharp`) that hands back the same `IChatClient`.
  Install [Ollama](https://ollama.com), then:

  ```powershell
  ollama pull llama3.2:1b
  dotnet run 01b-ollama.cs
  ```

  It runs fully offline with no key. The answer quality from a 1B local model
  isn't the point; the swap is. Want a sharper answer? Pull a larger model and
  change the one string.
- `03-image-generation.cs` is the one sample GitHub Models can't run, because the
  catalog has no image model. Point it at your own image-capable endpoint with two
  environment variables, and sign in keyless (no API key to copy):

  ```powershell
  # PowerShell - your own Azure OpenAI resource
  $env:AZURE_OPENAI_ENDPOINT = "https://YOUR-RESOURCE.openai.azure.com/"
  $env:AZURE_OPENAI_IMAGE_DEPLOYMENT = "gpt-image-1-mini"  # optional; this is the default
  az login   # DefaultAzureCredential uses this; nothing is hard-coded in the file
  dotnet run 03-image-generation.cs
  ```

  Deploy an image model first (for example `gpt-image-1-mini` on `GlobalStandard`);
  see the [Azure OpenAI image generation docs](https://learn.microsoft.com/azure/ai-foundry/openai/how-to/dall-e).
  Note that the `gpt-image-1` family always returns image bytes and rejects a
  `response_format`, so the sample doesn't set one. Prefer OpenAI? Swap the
  provider line for an `OpenAIClient` with `OPENAI_API_KEY`; the `IImageGenerator`
  code stays the same.
- `07-mcp.cs` connects to the live Microsoft Learn MCP server. Its doc results can
  be larger than the free GitHub Models input limit (8000 tokens), so the sample
  adds a small `DelegatingChatClient` that trims each tool result before it goes
  back to the model. In production you would summarize or page instead.
- `05-rag.cs` uses the in-memory vector store. The store ships in a connector
  package; your code targets the `Microsoft.Extensions.VectorData` abstraction,
  so swapping to SQLite, Qdrant, Azure AI Search, or Redis is a connector change,
  not a code change.
- `09-eval.cs` uses the same model as both the assistant and the judge to keep
  things simple. In real use, pick a strong judge model and run the evaluators in
  your test project and CI.
- Package versions are pinned in each file's `#:package` lines. If a version no
  longer resolves, bump it to the current release on
  [nuget.org](https://www.nuget.org).


