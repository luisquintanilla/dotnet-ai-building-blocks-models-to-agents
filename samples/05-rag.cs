#:package OpenAI@2.11.0
#:package Microsoft.Extensions.AI.OpenAI@10.7.0
#:package Microsoft.Extensions.VectorData.Abstractions@10.1.0
#:package Microsoft.SemanticKernel.Connectors.InMemory@1.74.0-preview

// Block 2 - Data and memory, part two: RAG.
// The problem: the model doesn't know your data. So you store your text as
// vectors, retrieve the closest matches to a question, and hand them to the
// model as context. Microsoft.Extensions.VectorData is the storage abstraction.
// You code against it once; swap the store (in-memory, SQLite, Qdrant, Azure AI
// Search, Redis) without touching the rest. Here we use the in-memory store.

using System.ClientModel;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using OpenAI;

string token = Environment.GetEnvironmentVariable("GITHUB_TOKEN")
    ?? throw new InvalidOperationException("Set GITHUB_TOKEN to a GitHub PAT with models:read. See README.");

OpenAIClient provider = new(
    new ApiKeyCredential(token),
    new OpenAIClientOptions { Endpoint = new Uri("https://models.inference.ai.azure.com") });

IChatClient chat = provider.GetChatClient("gpt-4o-mini").AsIChatClient();
IEmbeddingGenerator<string, Embedding<float>> embedder =
    provider.GetEmbeddingClient("text-embedding-3-small").AsIEmbeddingGenerator();

// A tiny "knowledge base" about the .NET AI building blocks.
string[] facts =
[
    "Microsoft.Extensions.AI defines IChatClient, the provider-agnostic chat interface.",
    "Microsoft.Extensions.VectorData is the storage abstraction for embeddings and search.",
    "Model Context Protocol, MCP, is an open standard for giving models access to tools.",
    "Microsoft.Extensions.AI.Evaluation scores response quality using the same IChatClient.",
    "Microsoft Agent Framework wraps an IChatClient as an agent with ChatClientAgent."
];

// Set up the store. The collection holds our records and their vectors.
InMemoryVectorStore store = new();
VectorStoreCollection<int, Doc> docs = store.GetCollection<int, Doc>("facts");
await docs.EnsureCollectionExistsAsync();

for (int i = 0; i < facts.Length; i++)
{
    Embedding<float> v = await embedder.GenerateAsync(facts[i]);
    await docs.UpsertAsync(new Doc { Id = i, Text = facts[i], Vector = v.Vector });
}

// Retrieve: embed the question, pull the closest facts.
string question = "How do I score whether my model's answers are any good?";
Embedding<float> queryVector = await embedder.GenerateAsync(question);

List<string> retrieved = [];
await foreach (VectorSearchResult<Doc> hit in docs.SearchAsync(queryVector.Vector, top: 2))
{
    retrieved.Add(hit.Record.Text);
    Console.WriteLine($"[{hit.Score:F3}] {hit.Record.Text}");
}

// Augment: give the retrieved facts to the model, then ask.
string context = string.Join("\n", retrieved);
ChatResponse answer = await chat.GetResponseAsync(
    $"""
     Answer using only these facts:
     {context}

     Question: {question}
     """);

Console.WriteLine();
Console.WriteLine(answer.Text);
Console.WriteLine();
Console.WriteLine("Retrieve, augment, generate. Swap the store later and this code stays put.");

// The record. Attributes tell any vector store how to map the fields.
class Doc
{
    [VectorStoreKey]
    public int Id { get; set; }

    [VectorStoreData]
    public required string Text { get; set; }

    [VectorStoreVector(1536, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Vector { get; set; }
}
