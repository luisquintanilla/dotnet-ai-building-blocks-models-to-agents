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
| 2 | This is home: where blocks fit | 3 | 0:05 |
| 3 | The map: six blocks, one foundation | 1 | 0:06 |
| 4 | Block 1 — Models | 5 | 0:11 |
| 5 | Block 2 — Data, ingestion & memory | 6 | 0:17 |
| 6 | Block 3 — Tools & MCP | 5 | 0:22 |
| 7 | Block 4 — Middleware & observability | 3 | 0:25 |
| 8 | Block 5 — Evaluations | 4 | 0:29 |
| 9 | Block 6 — Agents (Microsoft Agent Framework) | 7 | 0:36 |
| 10 | Everything together: the AI Chat Template | 5 | 0:41 |
| 11 | What's next | 2 | 0:43 |
| 12 | Recap + call to action | 2 | 0:45 |
| — | Q&A | 15 | 1:00 |

Buffer plan: if running long, cut the second half of Block 2 (RAG walkthrough) and the multi-agent demo to talking-only. Protect the agent graduation moment (Block 6) and the template reveal.

---

## 1. Hook (2 min)
- Open with the problem, not a story. Everybody's first AI demo is one model call. Looks easy.
- Then the real work: swap models, ground in your data, tools, caching, telemetry, knowing it's good, agents.
- Today that means stitching SDKs from different ecosystems. The .NET team shipped these as composable building blocks so you don't have to.
- Promise: we build the whole stack bottom-up with code you can run, and land on agents.

## 2. This is home (3 min) — *diagram: dotnet-stack-fit*
- The blocks are `Microsoft.Extensions.*`. They sit next to DI, configuration, logging, hosting, HTTP.
- Same DI registration, same builder, same middleware idea from ASP.NET Core, same IConfiguration/user-secrets, same ILogger, same OpenTelemetry. Aspire orchestrates.
- Message: if you know .NET, you already know how to build AI apps.

## 3. The map (1 min) — *diagram: building-blocks-stack*
- Six blocks: Models → Data → Tools/MCP → Middleware → Evaluations → Agents. One IChatClient foundation.
- We add a block only when the problem asks. Each block gets a runnable sample.

## 4. Block 1 — Models (5 min) — *sample: 01-chat.cs · diagram: stack-1*
- `IChatClient` is the foundation. One interface, any provider (GitHub Models, Azure OpenAI, OpenAI, Ollama, Foundry Local).
- Only the two client-construction lines are provider-specific. Everything after sees IChatClient.
- Swap model = change the string. Swap provider = change two lines. That's the interop promise.
- **Demo:** run 01, show same code answering with gpt-4o-mini then gpt-4o.

## 5. Block 2 — Data, ingestion & memory (6 min) — *samples: 02-embeddings.cs, 03-rag.cs · diagram: stack-2*
- Embeddings: models match on meaning, not keywords. `IEmbeddingGenerator` is the same pattern as `IChatClient`. Cosine similarity ships in .NET.
- RAG: retrieve, augment, generate. `Microsoft.Extensions.VectorData` is the storage abstraction.
- Swap the store (in-memory → SQLite → Qdrant → Azure AI Search → Redis) like swapping an EF Core provider. The rest of the code stays put.
- Mention `Microsoft.Extensions.DataIngestion` (reader → chunker → enricher → writer) as the pipeline that fills the store; deep dive in "what's next."
- **Demo:** run 02 (near vs far score), then 03 (retrieve 2 facts, grounded answer).

## 6. Block 3 — Tools & MCP (5 min) — *samples: 04-tools.cs, 05-mcp.cs · diagram: stack-3*
- Tools: a model can't run your code. `AIFunctionFactory` wraps a C# method; `UseFunctionInvocation` runs it. Method/param names + `[Description]` become the schema.
- Builder pattern again: AsBuilder → UseFunctionInvocation → Build.
- MCP: open standard, "HTTP for tools." A server exposes tools once; any client uses them. An MCP tool is just an `AIFunction`, drops into the same Tools list.
- .NET can consume MCP tools and serve them.
- **Demo:** run 04 (model calls GetNow + DaysUntil), then 05 (list Microsoft Learn tools, grounded answer).

## 7. Block 4 — Middleware & observability (3 min) — *sample: 06-middleware.cs · diagram: stack-4*
- `IChatClient` is a pipeline, same shape as the ASP.NET Core request pipeline.
- Stack function invocation + caching + `UseOpenTelemetry`. Read outermost-first.
- Cache: repeated prompt skips the model. Telemetry: OpenTelemetry is vendor-neutral → Aspire dashboard.
- Add a line to add a behavior. **Demo:** run 06, show second identical prompt served from cache.

## 8. Block 5 — Evaluations (4 min) — *sample: 07-eval.cs · diagram: stack-5*
- The quality gate everybody skips. Score the response: relevance, coherence, groundedness, and more.
- Evaluators use an `IChatClient` as the judge. Same foundation, one more block.
- **Key framing:** evaluations is NOT in the template. You wrap it around the app, in your test project / CI (offline MSTest/xUnit with caching + reporting) or online to telemetry.
- **Demo:** run 07, show the three metric scores + the judge's rating.

## 9. Block 6 — Agents (7 min) — *samples: 08-agent.cs, 09-multi-agent.cs · diagram: stack-6*
- The graduated moment. You already have an IChatClient and tools. Wrap them: `new ChatClientAgent(...)`. That's Microsoft Agent Framework. One line, no rewrite.
- A session carries the conversation, so the agent remembers across turns.
- Multi-agent: compose agents like middleware. Concurrent workflow = fan out to specialists, fan in results. `workflow.AsAIAgent(...)` runs through the same interface.
- **Say out loud:** one converged framework. Semantic Kernel and AutoGen work lives on here. Local-first to build, deploy to Foundry.
- **Demo:** run 08 (agent remembers name), then 09 (three reviewers, one summary).

## 10. Everything together: the AI Chat Template (5 min) — *diagram: template-anatomy*
- Show the production .NET AI Chat Template. One command → Blazor chat UI, ingestion, vector store, grounded RAG, middleware, Aspire.
- Point at every piece and name the block it came from. "You've seen every one of these."
- Evaluations plugs in at the test/CI layer.
- **Demo:** `dotnet new install Microsoft.Extensions.AI.Templates` → `dotnet new aichat -o MyChatApp` → `dotnet run`. Provider picker on the way out.

## 11. What's next (2 min) — *diagram: whats-next*
- Advanced ingestion (PDFs with layout/tables/images, ONNX layout detection, composable MEDI pipeline).
- Advanced RAG (better retrieval than default top-k).
- Both shown as upgrades that replace a template default, not rewrites.

## 12. Recap + CTA (2 min) — *recap card*
- Six blocks, all `Microsoft.Extensions.*`, all the same patterns, all one foundation. Adding blocks, never starting over.
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
