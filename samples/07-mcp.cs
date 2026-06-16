#:package OpenAI@2.11.0
#:package Microsoft.Extensions.AI.OpenAI@10.7.0
#:package Microsoft.Extensions.AI@10.7.0
#:package ModelContextProtocol@1.4.0

// Block 3 - Tools, part two: MCP.
// The problem: writing a tool for every system you want to reach doesn't scale.
// Model Context Protocol is an open standard, think "HTTP for tools." A server
// exposes tools once; any MCP client can use them. Here we connect to the public
// Microsoft Learn MCP server and let the model search the docs.
//
// The payoff: an MCP tool is just an AIFunction. It drops straight into the same
// ChatOptions.Tools list you used in 06-tools.cs. No new concept to learn.

using System.ClientModel;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OpenAI;

string token = Environment.GetEnvironmentVariable("GITHUB_TOKEN")
    ?? throw new InvalidOperationException("Set GITHUB_TOKEN to a GitHub PAT with models:read. See README.");

// Connect to the Microsoft Learn MCP server over HTTP and discover its tools.
await using McpClient learn = await McpClient.CreateAsync(new HttpClientTransport(new()
{
    Endpoint = new Uri("https://learn.microsoft.com/api/mcp"),
    Name = "Microsoft Learn"
}));

IList<McpClientTool> tools = await learn.ListToolsAsync();
Console.WriteLine($"Discovered tools: {string.Join(", ", tools.Select(t => t.Name))}");
Console.WriteLine();

IChatClient chat = new OpenAIClient(
                new ApiKeyCredential(token),
                new OpenAIClientOptions { Endpoint = new Uri("https://models.inference.ai.azure.com") })
            .GetChatClient("gpt-4.1-mini")
            .AsIChatClient()
    .AsBuilder()
    .UseFunctionInvocation()
    // Free GitHub Models caps the request at 8000 tokens. Live doc results can be
    // larger, so we trim each tool result before it goes back to the model. In
    // production you'd summarize or page instead. This is just middleware.
    .Use(inner => new TrimToolResults(inner, maxChars: 6000))
    .Build();

// The MCP tools spread directly into the Tools list, exactly like the plain C#
// tools in 06-tools.cs. The model searches Learn, reads the results, and answers
// grounded in the docs.
ChatResponse response = await chat.GetResponseAsync(
    "Using the Microsoft Learn docs, what is Microsoft.Extensions.AI? Answer in two sentences.",
    new ChatOptions { Tools = [.. tools] });

Console.WriteLine(response.Text);
Console.WriteLine();
Console.WriteLine("One open standard, every tool. That's MCP.");

// A few lines of middleware that keep large tool results inside the model's
// context window. DelegatingChatClient is the same pattern as 08-middleware.cs.
sealed class TrimToolResults(IChatClient inner, int maxChars) : DelegatingChatClient(inner)
{
    public override Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        foreach (ChatMessage message in messages)
        {
            foreach (AIContent content in message.Contents)
            {
                if (content is FunctionResultContent result)
                {
                    string text = result.Result as string ?? result.Result?.ToString() ?? string.Empty;
                    if (text.Length > maxChars)
                        result.Result = text[..maxChars];
                }
            }
        }

        return base.GetResponseAsync(messages, options, cancellationToken);
    }
}

