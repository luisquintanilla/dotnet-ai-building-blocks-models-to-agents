#:package OpenAI@2.11.0
#:package Microsoft.Extensions.AI.OpenAI@10.7.0
#:package System.Numerics.Tensors@10.0.9

// Block 2 - Data and memory, part one: embeddings.
// The problem: models match on meaning, not keywords. To find related text you
// turn each string into a vector and compare vectors. IEmbeddingGenerator is the
// same idea as IChatClient: one interface, any provider.

using System.ClientModel;
using System.Numerics.Tensors;
using Microsoft.Extensions.AI;
using OpenAI;

string token = Environment.GetEnvironmentVariable("GITHUB_TOKEN")
    ?? throw new InvalidOperationException("Set GITHUB_TOKEN to a GitHub PAT with models:read. See README.");

OpenAIClient provider = new(
    new ApiKeyCredential(token),
    new OpenAIClientOptions { Endpoint = new Uri("https://models.inference.ai.azure.com") });

IEmbeddingGenerator<string, Embedding<float>> embedder =
    provider.GetEmbeddingClient("text-embedding-3-small").AsIEmbeddingGenerator();

// Turn three sentences into vectors.
Embedding<float> dotnet = await embedder.GenerateAsync("I build apps with .NET.");
Embedding<float> platform = await embedder.GenerateAsync(".NET is a developer platform.");
Embedding<float> cats = await embedder.GenerateAsync("My cat sleeps all day.");

// Cosine similarity: higher means closer in meaning. The helper ships in .NET.
float near = TensorPrimitives.CosineSimilarity(dotnet.Vector.Span, platform.Vector.Span);
float far = TensorPrimitives.CosineSimilarity(dotnet.Vector.Span, cats.Vector.Span);

Console.WriteLine($"'.NET apps'  vs  '.NET platform' : {near:F3}");
Console.WriteLine($"'.NET apps'  vs  'cat sleeps'    : {far:F3}");
Console.WriteLine();
Console.WriteLine("Closer meaning, higher score. That's the whole idea behind search and RAG.");
