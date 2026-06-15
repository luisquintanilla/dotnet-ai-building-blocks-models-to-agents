<!-- .element overrides and per-slide classes are applied via <div> wrappers.
     This file is the single source of truth for the deck and doubles as a
     readable Markdown walkthrough of the talk. -->

<div class="title-slide">

<span class="kicker">.NET + AI</span>

# From Models to Agents

<p class="subtitle">The Essential Building Blocks of AI Apps</p>

<div class="title-rule"></div>

<p class="byline">Luis Quintanilla &nbsp;·&nbsp; .NET team, Microsoft</p>

</div>

Note:
Hey everyone, thanks for joining. Today I want to take the mystery out of building AI apps in .NET.
The short version: you already know how to do this. The AI features you keep hearing about are just building blocks, and they live right where the rest of your .NET app lives.
We'll start small, with code you can run, and stack the blocks one at a time until we land on agents. Then I'll show you the production template that ties it all together.

---

<span class="kicker">The problem</span>

## "Just call a model," they said

<div class="cols">
<div class="col-left">

Real AI features need more than one call:

- a model, and the freedom to change it
- your data, retrieved and grounded
- tools and actions
- caching, retries, telemetry
- a way to know it's any good
- and sometimes, agents

</div>
<div class="col-left">

Today that means stitching SDKs from different ecosystems, each with its own shape.

<p class="lead">The .NET team shipped these as composable building blocks so you don't have to.</p>

</div>
</div>

Note:
Here's the trap. Everybody's first AI demo is one call to a model. Looks easy.
Then the real work shows up. You want to swap the model when a cheaper one lands. You want answers grounded in your data. You want tools, caching, telemetry, and some way to prove the thing actually works.
The usual path is gluing together half a dozen libraries from different worlds. The .NET team's answer is to ship these as building blocks that snap together. That's what the next 40 minutes is about.

---

<span class="kicker">This is home</span>

## The blocks are `Microsoft.Extensions.*`

<div class="diagram">
<img src="assets/diagrams/dotnet-stack-fit.svg" alt="The Microsoft.Extensions family, including the AI building blocks, feeding into every .NET app type, orchestrated by Aspire." />
</div>

Note:
Look at where these live. Microsoft.Extensions.AI, VectorData, DataIngestion, AI.Evaluation. They sit right next to the extensions you already use every day: dependency injection, configuration, logging, hosting, HTTP.
Same DI registration. Same builder pattern. Same middleware idea from ASP.NET Core. Same IConfiguration and user-secrets. Same ILogger, same OpenTelemetry. Aspire orchestrates all of it.
So the message for the whole talk is this: if you know .NET, you already know how to build AI apps. This is home.

---

<span class="kicker">The map</span>

## Six blocks, one foundation

<div class="diagram">
<img src="assets/diagrams/building-blocks-stack.svg" alt="The building-blocks stack: Models, Data and memory, Tools and MCP, Middleware, Evaluations, and Agents, all on one IChatClient foundation." />
</div>

<p class="muted small">We'll light up one block at a time, and prove each with a tiny app you can run.</p>

Note:
This is our map. Six blocks. Models at the bottom, agents at the top, and everything resting on one shared foundation, IChatClient.
The plan is simple: we add a block only when the problem asks for it. Start with the simplest thing that works, then graduate.
And every block gets a tiny runnable sample. Single C# file, dotnet run, real output. No magic, no big project to set up.

---

<span class="kicker">Block 1 · Models</span>

## One interface for every provider

<div class="cols narrow-left">
<div class="col-left">

`IChatClient` is the foundation. Talk to GitHub Models, Azure OpenAI, OpenAI, Ollama, or Foundry Local through the same interface.

<div class="badges">
<span class="badge ext">Microsoft.Extensions.AI</span>
<span class="badge">provider-agnostic</span>
</div>

<span class="run">dotnet run 01-chat.cs</span>

</div>
<div class="col-left">

```csharp
OpenAIClient provider = new(
    new ApiKeyCredential(token),
    new() { Endpoint = new Uri(githubModels) });

IChatClient chat = provider
    .GetChatClient("gpt-4o-mini")
    .AsIChatClient();

ChatResponse response =
    await chat.GetResponseAsync("What is .NET?");
```

</div>
</div>

<p class="repeat-beat">Familiar .NET pattern · simplest thing that works · interoperable · same foundation</p>

Note:
Block one, models. The only provider-specific lines are the two that build the client. Everything after sees IChatClient and nothing else.
Want a different model? Change the string. Different provider? Change those two lines. The rest of your app doesn't move. That's the interop promise, and it's the foundation the other five blocks sit on.
Run the sample and you'll see the same code answer with gpt-4o-mini and then gpt-4o, no other changes.

--

<div class="diagram">
<img src="assets/diagrams/building-blocks-stack-1.svg" alt="The building-blocks stack with the Models block highlighted." />
</div>

Note:
One block lit. Models. Hold this picture in your head, because we're going to add to it the rest of the talk.

---

<span class="kicker">Block 2 · Data &amp; memory</span>

## Embeddings: match on meaning

<div class="cols">
<div class="col-left">

`IEmbeddingGenerator` is the same idea as `IChatClient`: one interface, any provider. Turn text into vectors, compare by cosine similarity.

<span class="run">dotnet run 02-embeddings.cs</span>

</div>
<div class="col-left">

```csharp
IEmbeddingGenerator<string, Embedding<float>> embedder =
    provider.GetEmbeddingClient("text-embedding-3-small")
            .AsIEmbeddingGenerator();

Embedding<float> a = await embedder.GenerateAsync(
    "I build apps with .NET.");
Embedding<float> b = await embedder.GenerateAsync(
    ".NET is a developer platform.");

float score = TensorPrimitives.CosineSimilarity(
    a.Vector.Span, b.Vector.Span);
```

</div>
</div>

Note:
Block two is data and memory, and it starts with embeddings. Models match on meaning, not keywords. So you turn text into vectors and compare them.
Notice the interface. IEmbeddingGenerator is the exact same pattern as IChatClient. One interface, swap the provider underneath. The cosine similarity helper ships in the box with .NET.
Closer meaning, higher score. That one idea is what powers search and RAG, which is the next slide.

--

<span class="kicker">Block 2 · RAG</span>

## Retrieve, augment, generate

<div class="cols">
<div class="col-left">

`Microsoft.Extensions.VectorData` is the storage abstraction. Code against it once, swap the store like you swap an EF Core provider.

<div class="badges">
<span class="badge ext">Microsoft.Extensions.VectorData</span>
<span class="badge">in-memory → Qdrant → Azure AI Search</span>
</div>

<span class="run">dotnet run 03-rag.cs</span>

</div>
<div class="col-left">

```csharp
await foreach (VectorSearchResult<Doc> hit in
    docs.SearchAsync(queryVector, top: 2))
{
    retrieved.Add(hit.Record.Text);
}

ChatResponse answer = await chat.GetResponseAsync(
    $"Answer using only these facts:\n{context}\n\n" +
    $"Question: {question}");
```

</div>
</div>

<p class="repeat-beat">Familiar .NET pattern · simplest thing that works · interoperable · same foundation</p>

Note:
Now use those vectors. RAG is three steps: retrieve the closest text, augment the prompt with it, generate the answer.
The storage sits behind Microsoft.Extensions.VectorData. In the sample it's the in-memory store. To move to Qdrant, Azure AI Search, Redis, or SQLite, you change the store and the rest of this code stays put. Same story as switching an EF Core provider.
This is exactly how the chat template grounds its answers, and we'll see that later.

--

<div class="diagram">
<img src="assets/diagrams/building-blocks-stack-2.svg" alt="The building-blocks stack with the Data and memory block highlighted." />
</div>

Note:
Two blocks lit. Models, plus data and memory. The picture is filling in.

---

<span class="kicker">Block 3 · Tools</span>

## Let the model run your code

<div class="cols">
<div class="col-left">

`AIFunctionFactory` wraps a normal C# method as a tool. `UseFunctionInvocation` runs it when the model asks, then feeds the result back.

<span class="run">dotnet run 04-tools.cs</span>

</div>
<div class="col-left">

```csharp
IChatClient chat = inner.AsBuilder()
    .UseFunctionInvocation()
    .Build();

[Description("Days until a month/day this year.")]
int DaysUntil(int month, int day) => /* ... */;

ChatOptions options = new()
{
    Tools = [AIFunctionFactory.Create(DaysUntil)]
};

await chat.GetResponseAsync(prompt, options);
```

</div>
</div>

Note:
Block three, tools. A model reasons about text but it can't read a clock or call your API. So you hand it tools.
AIFunctionFactory turns a plain C# method into a tool. The method name, the parameter names, and the Description attribute become the schema the model sees. Name things clearly.
And look how tools turn on: AsBuilder, UseFunctionInvocation, Build. That's the ASP.NET Core builder pattern. You wrote plain methods, the model called them. We never called them ourselves.

--

<span class="kicker">Block 3 · MCP</span>

## One open standard for every tool

<div class="cols">
<div class="col-left">

Model Context Protocol is "HTTP for tools." A server exposes tools once, any client uses them. An MCP tool is just an `AIFunction`.

<div class="badges">
<span class="badge open">open standard</span>
<span class="badge">consume &amp; serve</span>
</div>

<span class="run">dotnet run 05-mcp.cs</span>

</div>
<div class="col-left">

```csharp
await using McpClient learn =
    await McpClient.CreateAsync(new HttpClientTransport(new()
    {
        Endpoint = new Uri("https://learn.microsoft.com/api/mcp"),
        Name = "Microsoft Learn"
    }));

IList<McpClientTool> tools = await learn.ListToolsAsync();

await chat.GetResponseAsync(question,
    new ChatOptions { Tools = [.. tools] });
```

</div>
</div>

<p class="repeat-beat">Familiar .NET pattern · simplest thing that works · interoperable · same foundation</p>

Note:
Writing a tool for every system doesn't scale. MCP is the open standard that fixes that. Think of it as HTTP for tools. A server publishes its tools once, and any MCP client can use them.
Here we connect to the public Microsoft Learn server, list its tools, and the model searches the docs for us.
And the payoff is the interop. An MCP tool is just an AIFunction. It drops straight into the same Tools list from the previous slide. No new concept. .NET can consume MCP tools and serve them too.

--

<div class="diagram">
<img src="assets/diagrams/building-blocks-stack-3.svg" alt="The building-blocks stack with the Tools and MCP block highlighted." />
</div>

Note:
Three blocks. Models, data, tools. We can already build something real. But real means production, and that's the next block.

---

<span class="kicker">Block 4 · Middleware</span>

## `IChatClient` is a pipeline

<div class="cols">
<div class="col-left">

Exactly like the ASP.NET Core request pipeline. Wrap the client and each piece does one job: function invocation, caching, telemetry.

<div class="badges">
<span class="badge open">OpenTelemetry</span>
<span class="badge">Aspire dashboard</span>
</div>

<span class="run">dotnet run 06-middleware.cs</span>

</div>
<div class="col-left">

```csharp
IChatClient chat = inner.AsBuilder()
    .UseFunctionInvocation()
    .UseDistributedCache(cache)
    .UseOpenTelemetry(sourceName: name)
    .Build();
```

<p class="muted small">Add a line to add a behavior. Remove a line to remove it.</p>

</div>
</div>

Note:
Block four, middleware and observability. Production needs more than a raw call. Caching, retries, telemetry, function invocation.
IChatClient is a pipeline, the same shape as the ASP.NET Core request pipeline. Read it top to bottom: invoke tools, then cache, then trace, then the real client.
Caching means a repeated prompt skips the model the second time. Telemetry is OpenTelemetry, vendor-neutral, so those traces show up in the Aspire dashboard or any backend you like. Add a line to add a behavior. That's the whole model.

--

<div class="diagram">
<img src="assets/diagrams/building-blocks-stack-4.svg" alt="The building-blocks stack with the Middleware and observability block highlighted." />
</div>

Note:
Four blocks lit. Now we can ship it. But how do we know it's good, and how do we keep it good? That's block five.

---

<span class="kicker">Block 5 · Evaluations</span>

## Know it's good. Stay good.

<div class="cols">
<div class="col-left">

`Microsoft.Extensions.AI.Evaluation` scores responses with evaluators that use an `IChatClient` as the judge. Same foundation.

This block isn't in the template. You add it in your **test project and CI**.

<span class="run">dotnet run 07-eval.cs</span>

</div>
<div class="col-left">

```csharp
IEvaluator evaluators = new CompositeEvaluator(
    new RelevanceEvaluator(),
    new CoherenceEvaluator(),
    new GroundednessEvaluator());

EvaluationResult result = await evaluators.EvaluateAsync(
    conversation, modelResponse, judge,
    additionalContext: [grounding]);

NumericMetric m = result.Get<NumericMetric>(name);
// m.Value: 4.0/5  (m.Interpretation?.Rating)
```

</div>
</div>

<p class="repeat-beat">Familiar .NET pattern · simplest thing that works · interoperable · same foundation</p>

Note:
Block five, evaluations. This is the quality gate, and it's the one people skip until it bites them.
You score the response. Relevance, coherence, groundedness, and more. The evaluators use an IChatClient as the judge, so it's the same foundation again, one more block.
Here's the important framing: evaluations is not in the chat template. It's the block you wrap around the app. Run it online, scoring to telemetry, or offline in MSTest or xUnit in CI, with caching and reporting. Score every change so quality never quietly regresses.

--

<div class="diagram">
<img src="assets/diagrams/building-blocks-stack-5.svg" alt="The building-blocks stack with the Evaluations block highlighted." />
</div>

Note:
Five blocks. We've got a grounded, observable, tested app. There's one block left, and it's the one everybody's asking about.

---

<span class="kicker">Block 6 · Agents</span>

## From blocks to agents, one line

<div class="cols">
<div class="col-left">

You already have an `IChatClient` and tools. Wrap them as a `ChatClientAgent`. That's Microsoft Agent Framework. No rewrite.

<div class="badges">
<span class="badge ext">Microsoft.Agents.AI</span>
<span class="badge">same IChatClient</span>
</div>

<span class="run">dotnet run 08-agent.cs</span>

</div>
<div class="col-left">

```csharp
AIAgent agent = new ChatClientAgent(
    chat,
    instructions: "You are a .NET event planner.",
    name: "Planner",
    tools: [AIFunctionFactory.Create(DaysUntil)]);

AgentResponse first =
    await agent.RunAsync("Days until Dec 25th?");

AgentSession session = await agent.CreateSessionAsync();
// the session remembers across turns
```

</div>
</div>

Note:
Block six. Agents. This is the graduated moment the whole talk has been building toward.
At some point you want the model to hold its own instructions, keep a conversation, and use tools on its own. That's an agent. And here's the punchline: you already have an IChatClient and tools, so you just wrap them. One line. new ChatClientAgent. That's Microsoft Agent Framework.
No new mental model, no rewrite. A session carries the conversation so it remembers across turns. This is why we spent the whole talk on the foundation. Moving to agents is a wrap, not a rebuild.

--

<span class="kicker">Block 6 · Multi-agent</span>

## Compose agents like middleware

<div class="cols">
<div class="col-left">

One agent is enough until the work has parts. Then you compose them. A concurrent workflow fans out to specialists and fans the results back in.

<span class="run">dotnet run 09-multi-agent.cs</span>

</div>
<div class="col-left">

```csharp
Workflow workflow = AgentWorkflowBuilder.BuildConcurrent(
    [technical, product, risk],
    aggregator: outputs => Summarize(outputs));

// a workflow runs through the same agent interface
AIAgent panel = workflow.AsAIAgent(name: "ReviewPanel");

AgentResponse review = await panel.RunAsync(proposal);
```

</div>
</div>

<p class="repeat-beat">One converged framework · local-first · deploy to Foundry</p>

Note:
And when one agent isn't enough, you compose them, the same way you composed middleware.
Here three reviewers, technical, product, and risk, look at one proposal at the same time, and we gather their notes. That's a concurrent workflow: fan out, fan in.
The nice part: the workflow becomes an AIAgent through the same interface. Compose, don't rewrite. And one more thing to say out loud: this is one converged framework. If you used Semantic Kernel or AutoGen, that work lives on here, in Microsoft Agent Framework. Local-first to build, deploy to Foundry when you're ready.

--

<div class="diagram">
<img src="assets/diagrams/building-blocks-stack-6.svg" alt="The full building-blocks stack with every block, including Agents, highlighted." />
</div>

Note:
All six blocks lit. That's the whole stack, and you watched it get built one runnable sample at a time. Now let me show you what it looks like when somebody already assembled it for you.

---

<span class="kicker">Everything together</span>

## The .NET AI Chat Template

<div class="diagram">
<img src="assets/diagrams/template-anatomy.svg" alt="The AI Chat Template's components mapped back to the building blocks, with evaluations plugging in at the test and CI layer." />
</div>

Note:
This is the production .NET AI Chat Template. One command in the CLI or Visual Studio and you get a real app: a Blazor chat UI, ingestion, a vector store, grounded RAG, the middleware pipeline, all wired up with Aspire.
And here's the thing I want to land. Point at any piece of this app. The IChatClient? That's block one. The embeddings and vector store? Block two. Tools, middleware, telemetry? Blocks three and four. You've already seen every one of these.
The template is just the blocks, assembled. And evaluations plugs in right here, at the test and CI layer, keeping the whole thing honest.

--

<span class="kicker">Try it</span>

## Scaffold and run

<span class="run">dotnet new install Microsoft.Extensions.AI.Templates</span>

<span class="run">dotnet new aichat -o MyChatApp</span>

<span class="run">dotnet run</span>

<p class="muted small">Pick your provider: GitHub Models, Azure OpenAI, OpenAI, or Ollama. Same building blocks underneath.</p>

Note:
You don't have to take my word for it. Install the templates, scaffold an aichat app, run it. Two minutes.
On the way out it asks which provider you want. GitHub Models to start for free, Azure OpenAI for production, OpenAI, or Ollama to run local. Doesn't matter, because underneath it's the same building blocks we just walked through, and the same IChatClient seam lets you switch later.

---

<span class="kicker">What's next</span>

## Upgrade the defaults

<div class="diagram">
<img src="assets/diagrams/whats-next.svg" alt="Advanced ingestion and advanced RAG plugging into the template, replacing the default building blocks." />
</div>

Note:
Two things we're exploring, both shown as upgrades to the template's defaults.
First, advanced ingestion. The template handles simple documents. For real PDFs with layout, tables, and images, there's a composable ingestion pipeline, reader to chunker to enricher to writer, with ONNX layout detection. Every step swappable.
Second, advanced RAG. Better retrieval than the default top-k. Because the blocks are swappable, these aren't rewrites. You replace one default and keep everything else.

---

<span class="kicker">Recap</span>

## You already knew how to do this

<div class="blocks">
<div class="block-card active"><div class="num">01</div><div class="name">Models</div><div class="pkg">Microsoft.Extensions.AI</div></div>
<div class="block-card active"><div class="num">02</div><div class="name">Data &amp; memory</div><div class="pkg">VectorData · DataIngestion</div></div>
<div class="block-card active"><div class="num">03</div><div class="name">Tools &amp; MCP</div><div class="pkg">AIFunction · MCP</div></div>
<div class="block-card active"><div class="num">04</div><div class="name">Middleware</div><div class="pkg">UseFunctionInvocation · OTel</div></div>
<div class="block-card active"><div class="num">05</div><div class="name">Evaluations</div><div class="pkg">AI.Evaluation</div></div>
<div class="block-card active"><div class="num">06</div><div class="name">Agents</div><div class="pkg">Microsoft.Agents.AI</div></div>
</div>

<p class="lead">Building blocks for the AI age, and they're already part of .NET.</p>

Note:
Let's bring it home. Six blocks. Models, data and memory, tools and MCP, middleware, evaluations, agents.
Every one is a Microsoft.Extensions library. Every one uses the DI, builder, and middleware patterns you already know. Every one sits on the same IChatClient foundation and on open standards. So moving from a single model call all the way to a multi-agent workflow was adding blocks, never starting over.
Building blocks for the AI age, and they're already part of .NET. You learn it once and use it everywhere.

---

<span class="kicker">Resources</span>

## Try → Learn → Tell us

<div class="cols">
<div class="col-left">

**Try**
<span class="run">dotnet new aichat -o MyChatApp</span>

**Learn**
- The .NET + AI hub: `learn.microsoft.com/dotnet/ai`
- All nine samples in this repo

</div>
<div class="col-left">

**This talk + samples**
`github.com/luisquintanilla/dotnet-ai-building-blocks-models-to-agents`

**Tell us**
Open an issue with what you build, or what's missing.

<div class="badges">
<span class="badge ext">Microsoft.Extensions.*</span>
<span class="badge open">open standards</span>
</div>

</div>
</div>

Note:
Three ways to take this further. Try: scaffold the template today. Learn: the dotnet slash ai hub is the map, and every sample from this talk is in the repo so you can run them yourself. Tell us: open an issue with what you build or what's missing, because that's how these blocks keep getting better.
Grab the repo link, and let's open it up for questions.

---

<div class="title-slide">

<div class="title-rule"></div>

# Questions?

<p class="subtitle">From Models to Agents: The Essential Building Blocks of AI Apps</p>

<p class="byline">Luis Quintanilla &nbsp;·&nbsp; github.com/luisquintanilla</p>

</div>

Note:
Thank you. I've got about fifteen minutes, so let's get into your questions. If you want to follow along, the repo link is on the previous slide and everything we ran today is in there.
