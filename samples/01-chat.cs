#:package OpenAI@2.11.0
#:package Microsoft.Extensions.AI.OpenAI@10.7.0

// Block 1 - Models.
// The problem: every provider has its own SDK. You don't want to rewrite your
// app when you change models. Microsoft.Extensions.AI gives you one interface,
// IChatClient, and you talk to any provider through it.

using System.ClientModel;
using Microsoft.Extensions.AI;
using OpenAI;

string token = Environment.GetEnvironmentVariable("GITHUB_TOKEN")
    ?? throw new InvalidOperationException("Set GITHUB_TOKEN to a GitHub PAT with models:read. See README.");

// GitHub Models speaks the OpenAI protocol, so we point the OpenAI client at it.
// The only provider-specific lines are these two. Everything after is IChatClient.
OpenAIClient provider = new(
    new ApiKeyCredential(token),
    new OpenAIClientOptions { Endpoint = new Uri("https://models.inference.ai.azure.com") });

IChatClient chat = provider.GetChatClient("gpt-4.1-mini").AsIChatClient();

// One call. Same shape no matter who serves the model.
ChatResponse response = await chat.GetResponseAsync("In one sentence, what is .NET?");
Console.WriteLine(response.Text);

// That's it! To swap the model, change the string. To swap the provider, change
// the two lines above. Your code below the client keeps working, because it only
// ever sees IChatClient. That is the interop promise.
ChatResponse swapped = await provider.GetChatClient("gpt-4o").AsIChatClient()
    .GetResponseAsync("Name one thing you can build with .NET. Five words max.");

Console.WriteLine();
Console.WriteLine($"Same interface, different model: {swapped.Text}");
