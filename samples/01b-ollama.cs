#:package OllamaSharp@5.1.19
#:property JsonSerializerIsReflectionEnabledByDefault=true

// Block 1 - Models, provider swap.
// Same downstream code as 01-chat.cs. The ONLY thing that changes is the client
// you construct. Here it's a local Ollama model instead of GitHub Models, and the
// rest of the program never notices, because it only ever sees IChatClient.

using Microsoft.Extensions.AI;
using OllamaSharp;

// The only provider-specific line: build an Ollama-backed IChatClient.
// Needs a local Ollama (https://ollama.com) with: ollama pull llama3.2:1b
IChatClient chat = new OllamaApiClient(
    new Uri("http://localhost:11434"), "llama3.2:1b");

// Everything below is identical to 01-chat.cs. Same interface, different provider.
ChatResponse response = await chat.GetResponseAsync("In one sentence, what is .NET?");
Console.WriteLine(response.Text);
