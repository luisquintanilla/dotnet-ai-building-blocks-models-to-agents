# Abstract

**From Models to Agents: The Essential Building Blocks of AI Apps**

"Just call a model" is where every AI demo starts and where every real feature gets stuck. Production AI needs more: the freedom to swap models, your own data retrieved and grounded, tools and actions, caching and telemetry, a way to prove the output is any good, and sometimes agents. The usual path is gluing together SDKs from different ecosystems.

It doesn't have to be that way in .NET. The AI building blocks ship as `Microsoft.Extensions.*` libraries that sit right next to the dependency injection, configuration, logging, and HTTP extensions you already use. Same builder pattern, same middleware pipeline, same DI registration. If you know .NET, you already know how to build AI apps.

We'll build the stack from the bottom up, one runnable sample at a time: a single `IChatClient` call, embeddings and RAG over a swappable vector store, tools and the open Model Context Protocol, the middleware pipeline with caching and OpenTelemetry, and evaluations as the quality gate in CI. Each block rests on the same foundation and on open standards, so moving up the stack is adding a block, never starting over. We finish by wrapping that same client as an agent with Microsoft Agent Framework, composing a small multi-agent workflow, and then revealing the production .NET AI Chat Template, where every piece you just saw is already assembled.

You'll leave able to build, ground, observe, evaluate, and graduate an AI app to agents using the .NET you already know.

**Audience:** .NET developers, all levels. No prior AI experience required.

**Length:** 60 minutes (45 minutes content, 15 minutes Q&A).
