#:package OpenAI@2.11.0
#:package Microsoft.Extensions.AI.OpenAI@10.7.0

// Block 1 - Models are multimodal.
// The problem: models are not text-only anymore. You still don't want one SDK
// path for text and another for images. Microsoft.Extensions.AI keeps the same
// IChatClient interface; the message just carries richer content.

using System.ClientModel;
using Microsoft.Extensions.AI;
using OpenAI;

string token = Environment.GetEnvironmentVariable("GITHUB_TOKEN")
    ?? throw new InvalidOperationException("Set GITHUB_TOKEN to a GitHub PAT with models:read. See README.");

OpenAIClient provider = new(
    new ApiKeyCredential(token),
    new OpenAIClientOptions { Endpoint = new Uri("https://models.inference.ai.azure.com") });

IChatClient chat = provider.GetChatClient("gpt-4o-mini").AsIChatClient();

Uri imageUri = new("https://upload.wikimedia.org/wikipedia/commons/3/3f/Fronalpstock_big.jpg");
ChatMessage message = new(ChatRole.User,
[
    new TextContent("What is in this image? Answer in one sentence."),
    new UriContent(imageUri, "image/jpeg")
]);

ChatResponse response = await chat.GetResponseAsync([message]);
Console.WriteLine(response.Text);

// That's it! It is still one interface. The same one-interface pattern works
// for audio input too: pass an audio content part to a model that accepts audio.
