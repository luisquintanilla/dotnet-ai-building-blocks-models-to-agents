# Speaker outline + talking points

**From Models to Agents: The Essential Building Blocks of AI Apps**
Luis Quintanilla · 60 minutes (45 content + 15 Q&A)

The through-line: **building blocks for the AI age, and they're already part of .NET.**
Repeated beat at every block: *familiar .NET pattern · simplest thing that works · interoperable · same foundation.*

---

## Time budget (45 min content)

| # | Section | Time | Running |
|---|---------|------|---------|
| 1 | Hook: "just call a model" | 2 | 0:02 |
| 2 | AI primitives, native to .NET | 3 | 0:05 |
| 3 | The map: six blocks, one foundation | 1 | 0:06 |
| 4 | Block 1 — Models (incl. multimodal) | 7 | 0:13 |
| 5 | Block 2 — Data, ingestion & memory | 5 | 0:18 |
| 6 | Block 3 — Tools & MCP | 4 | 0:22 |
| 7 | Block 4 — Middleware & observability | 3 | 0:25 |
| 8 | Block 5 — Evaluations | 3 | 0:28 |
| 9 | Block 6 — Agents (Microsoft Agent Framework) | 6 | 0:34 |
| 10 | Everything together: the AI Chat Template | 4 | 0:38 |
| 11 | What's next | 2 | 0:40 |
| 12 | The final demo: enhanced template on a branch | 3 | 0:43 |
| 13 | Recap + call to action | 2 | 0:45 |
| — | Q&A | 15 | 1:00 |

Buffer plan: if running long, drop the image-generation walk in Block 1 to talking-only (it needs an image-capable endpoint anyway), cut the RAG code read in Block 2, and run the multi-agent demo as talk-only. Protect the agent graduation moment (Block 6), the template reveal (10), and the final demo (12).

---

## 1. Hook (2 min)
- Open with the problem, not a story. Everybody's first AI demo is one model call. Looks easy.
- Then the real work: swap models, ground in your data, tools, caching, telemetry, knowing it's good, agents.
- Today that means stitching SDKs from different ecosystems. The .NET team shipped these as composable building blocks so you don't have to.
- Promise: we build the whole stack bottom-up with code you can run, and land on agents.

## 2. AI primitives, native to .NET (3 min) — *diagram: dotnet-stack-fit*
- Every AI app needs the same primitives: talk to models, represent and search data, call tools, observe, evaluate, orchestrate agents. .NET ships them as building blocks, built the way you already work.
- Same DI registration, same builder, same middleware idea from ASP.NET Core, same IConfiguration/user-secrets, same ILogger, same OpenTelemetry. Aspire orchestrates.
- Message: your .NET skills carry over. If you know .NET, you already know how to build AI apps.

## 3. The map (1 min) — *diagram: building-blocks-stack*
- Six blocks: Models → Data → Tools/MCP → Middleware → Evaluations → Agents. One IChatClient foundation.
- We add a block only when the problem asks. Each block gets a runnable sample.

## 4. Block 1 — Models (7 min) — *samples: 01-chat.cs, 02-vision.cs, 03-image-generation.cs · diagram: stack-1*
- `IChatClient` is the foundation. One interface, any provider (GitHub Models, Azure OpenAI, OpenAI, Ollama, Foundry Local).
- Only the two client-construction lines are provider-specific. Everything after sees IChatClient.
- Swap model = change the string. Swap provider = change two lines. That's the interop promise.
- **Multimodal:** models aren't text-only. The same IChatClient takes images (TextContent + UriContent/DataContent). Image generation is IImageGenerator, the same one-interface pattern. Speech-to-text is ISpeechToTextClient. Real-time audio is provider-SDK today.
- **Demo:** run 01 (same code, gpt-4o-mini then gpt-4o), then 02-vision (describe an image). Image generation (03) is talk-or-run: it needs an image-capable endpoint (OpenAI/Azure OpenAI); the IImageGenerator code is the same, you swap the provider.

## 5. Block 2 — Data, ingestion & memory (5 min) — *samples: 04-embeddings.cs, 05-rag.cs · diagram: stack-2*
- Embeddings: models match on meaning, not keywords. `IEmbeddingGenerator` is the same pattern as `IChatClient`. Cosine similarity ships in .NET.
- RAG: retrieve, augment, generate. `Microsoft.Extensions.VectorData` is the storage abstraction.
- Swap the store (in-memory → SQLite → Qdrant → Azure AI Search → Redis) like swapping an EF Core provider. The rest of the code stays put.
- Mention `Microsoft.Extensions.DataIngestion` (reader → chunker → enricher → writer) as the pipeline that fills the store; deep dive in "what's next."
- **Demo:** run 04 (near vs far score), then 05 (retrieve 2 facts, grounded answer).

## 6. Block 3 — Tools & MCP (4 min) — *samples: 06-tools.cs, 07-mcp.cs · diagram: stack-3*
- Tools: a model can't run your code. `AIFunctionFactory` wraps a C# method; `UseFunctionInvocation` runs it. Method/param names + `[Description]` become the schema.
- Builder pattern again: AsBuilder → UseFunctionInvocation → Build.
- MCP: open standard, "HTTP for tools." A server exposes tools once; any client uses them. An MCP tool is just an `AIFunction`, drops into the same Tools list.
- .NET can consume MCP tools and serve them.
- **Demo:** run 06 (model calls GetNow + DaysUntil), then 07 (list Microsoft Learn tools, grounded answer).

## 7. Block 4 — Middleware & observability (3 min) — *sample: 08-middleware.cs · diagram: stack-4*
- `IChatClient` is a pipeline, same shape as the ASP.NET Core request pipeline.
- Stack function invocation + caching + `UseOpenTelemetry`. Read outermost-first.
- Cache: repeated prompt skips the model. Telemetry: OpenTelemetry is vendor-neutral → Aspire dashboard.
- Add a line to add a behavior. **Demo:** run 08, show second identical prompt served from cache.

## 8. Block 5 — Evaluations (3 min) — *sample: 09-eval.cs · diagram: stack-5*
- The quality gate everybody skips. Score the response: relevance, coherence, groundedness, and more.
- Evaluators use an `IChatClient` as the judge. Same foundation, one more block.
- **Key framing:** evaluations is NOT in the template. You wrap it around the app, in your test project / CI (offline MSTest/xUnit with caching + reporting) or online to telemetry.
- **Demo:** run 09, show the three metric scores + the judge's rating.

## 9. Block 6 — Agents (6 min) — *samples: 10-agent.cs, 11-multi-agent.cs · diagrams: agent-encapsulation, stack-6*
- **What is an agent (open here):** a chatbot answers and you orchestrate every step; an agent has autonomy. It reasons, picks a tool, calls it, checks the result, and decides what's next, on its own. An agent is not a seventh block. It is the blocks you already built (model, data and memory, tools, middleware) wrapped behind one boundary, with the loop on top, on the same .NET foundation. *(diagram: agent-encapsulation)*
- The graduated moment. You already have an IChatClient and tools. Wrap them: `new ChatClientAgent(...)`. That's Microsoft Agent Framework. One line, no rewrite.
- A session carries the conversation, so the agent remembers across turns.
- Multi-agent: compose agents like middleware. Concurrent workflow = fan out to specialists, fan in results. `workflow.AsAIAgent(...)` runs through the same interface.
- **Say out loud:** one converged framework. Semantic Kernel and AutoGen work lives on here. Local-first to build, deploy to Foundry.
- **Demo:** run 10 (agent remembers name), then 11 (three reviewers, one summary).

## 10. Everything together: the AI Chat Template (4 min) — *diagram: template-anatomy*
- Show the production .NET AI Chat Template. One command → Blazor chat UI, ingestion, vector store, grounded RAG, middleware, Aspire.
- Point at every piece and name the block it came from. "You've seen every one of these."
- Evaluations plugs in at the test/CI layer.
- **Demo:** `dotnet new install Microsoft.Extensions.AI.Templates` → `dotnet new aichatweb -o MyChatApp --aspire` → `dotnet run --project MyChatApp/MyChatApp.AppHost`. Aspire dashboard opens; provider (GitHub Models) was chosen at scaffold time.

## 11. What's next (2 min) — *diagram: whats-next*
- Frame as problems, not products. Advanced ingestion: real PDFs have layout, tables, images. A composable reader → chunker → enricher → writer pipeline with layout detection fixes it. (Go deeper: PdfAIngest.)
- Advanced retrieval: default top-k isn't enough. Reranking, filtering, query rewriting. (Go deeper: advanced-rag.)
- Both are upgrades that replace one template default, not rewrites. Sets up the final demo.

## 12. The final demo (3 min) — *deck: "Upgrade, not rewrite"*
- Same template, on a branch. `advanced-demo` swaps two blocks: layout-aware ingestion + advanced retrieval. Everything else untouched.
- Show `git diff main..advanced-demo`. It's small. That's the thesis on screen: upgrade an AI app by changing a block, not starting over.
- **Demo:** run the branch on a real PDF, ask the same questions, show better grounding. If short on time, show the diff and talk it through.

## 13. Recap + CTA (2 min) — *recap card*
- Six blocks, all built the .NET way, all the same patterns, all one foundation. Adding blocks, never starting over.
- Graduated CTA: **Try** (scaffold the template) → **Learn** (`learn.microsoft.com/dotnet/ai` + the repo samples) → **Tell us** (open an issue).
- Land the line: building blocks for the AI age, and they're already part of .NET.

---

## Q&A primers (likely questions)
- **Cost / which provider?** GitHub Models to start free, Azure OpenAI for production, Ollama/Foundry Local for local. The IChatClient seam means you switch later without touching app code.
- **Semantic Kernel / AutoGen?** Converged into Microsoft Agent Framework. Existing investments carry forward; this is the one path now.
- **Why not LangChain-style?** The blocks are the .NET-native, DI-and-middleware version. You stay in the platform you already run in production.
- **Production-ready?** MEAI/VectorData/Evaluation are stable (10.x); concrete vector stores and some connectors are still preview, called out in the repo.
- **Where do evaluations run?** Test project / CI offline with caching + reporting, or online scoring to telemetry. Not in the template by design.
- **Security / secrets?** Same as any .NET app: IConfiguration, user-secrets, Key Vault. Nothing new to learn.
