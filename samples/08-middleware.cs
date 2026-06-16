#:package OpenAI@2.11.0
#:package Microsoft.Extensions.AI.OpenAI@10.7.0
#:package Microsoft.Extensions.AI@10.7.0
#:package Microsoft.Extensions.Caching.Memory@10.0.9
#:package OpenTelemetry@1.16.0
#:package OpenTelemetry.Exporter.Console@1.16.0

// Block 4 - Middleware and observability.
// The problem: production needs more than a raw call. You want caching, retries,
// telemetry, function invocation. IChatClient is a pipeline, exactly like the
// ASP.NET Core request pipeline. You wrap the client in middleware and each piece
// does one job. Here we stack function invocation, caching, and OpenTelemetry.

using System.ClientModel;
using System.ComponentModel;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenTelemetry.Trace;

string token = Environment.GetEnvironmentVariable("GITHUB_TOKEN")
    ?? throw new InvalidOperationException("Set GITHUB_TOKEN to a GitHub PAT with models:read. See README.");

// Telemetry: emit traces to the console. In a real app these flow to the Aspire
// dashboard or any OpenTelemetry backend, because the standard is vendor-neutral.
string sourceName = "Sample.Block4";
using TracerProvider tracer = OpenTelemetry.Sdk.CreateTracerProviderBuilder()
    .AddSource(sourceName)
    .AddConsoleExporter()
    .Build();

// An in-process cache so a repeated prompt skips the model the second time.
IDistributedCache cache = new MemoryDistributedCache(
    Options.Create(new MemoryDistributedCacheOptions()));

IChatClient inner = new OpenAIClient(
        new ApiKeyCredential(token),
        new OpenAIClientOptions { Endpoint = new Uri("https://models.inference.ai.azure.com") })
    .GetChatClient("gpt-4.1-mini")
    .AsIChatClient();

// Outermost first. Read it top to bottom: invoke tools, then cache, then trace,
// then the real client. Add or remove a line to add or remove a behavior.
IChatClient chat = inner
    .AsBuilder()
    .UseFunctionInvocation()
    .UseDistributedCache(cache)
    .UseOpenTelemetry(sourceName: sourceName, configure: o => o.EnableSensitiveData = true)
    .Build();

[Description("Gets the current local time.")]
string GetTime() => DateTime.Now.ToString("t");

ChatOptions options = new() { Tools = [AIFunctionFactory.Create(GetTime)] };

Console.WriteLine("First call (hits the model, traces, fills the cache):");
ChatResponse first = await chat.GetResponseAsync("What time is it?", options);
Console.WriteLine(first.Text);

Console.WriteLine();
Console.WriteLine("Same prompt again (served from cache, no model call):");
ChatResponse second = await chat.GetResponseAsync("What time is it?", options);
Console.WriteLine(second.Text);

Console.WriteLine();
Console.WriteLine("Same builder pattern you already know. Each block does one thing.");
