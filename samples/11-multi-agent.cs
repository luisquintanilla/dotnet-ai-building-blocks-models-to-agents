#:package OpenAI@2.11.0
#:package Microsoft.Extensions.AI.OpenAI@10.7.0
#:package Microsoft.Agents.AI@1.10.0
#:package Microsoft.Agents.AI.Abstractions@1.10.0
#:package Microsoft.Agents.AI.Workflows@1.10.0

// Block 6, continued - multi-agent.
// The problem: one agent is enough until the work has parts that want different
// expertise. Then you compose agents the same way you composed middleware. Here
// three reviewers look at one proposal at the same time, and we gather their
// notes. This is a concurrent workflow: fan out to all agents, fan in the results.

using System.ClientModel;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using OpenAI;

string token = Environment.GetEnvironmentVariable("GITHUB_TOKEN")
    ?? throw new InvalidOperationException("Set GITHUB_TOKEN to a GitHub PAT with models:read. See README.");

IChatClient chat = new OpenAIClient(
        new ApiKeyCredential(token),
        new OpenAIClientOptions { Endpoint = new Uri("https://models.inference.ai.azure.com") })
    .GetChatClient("gpt-4.1-mini")
    .AsIChatClient();

// Three agents, three viewpoints. Each is a ChatClientAgent from Block 6.
AIAgent technical = new ChatClientAgent(chat,
    instructions: "You are a senior engineer. In two sentences, critique the technical plan.",
    name: "Technical");

AIAgent product = new ChatClientAgent(chat,
    instructions: "You are a product manager. In two sentences, critique the user value.",
    name: "Product");

AIAgent risk = new ChatClientAgent(chat,
    instructions: "You are a risk officer. In two sentences, name the biggest risk.",
    name: "Risk");

// Build a concurrent workflow and aggregate the three outputs into one summary.
Workflow workflow = AgentWorkflowBuilder.BuildConcurrent(
    [technical, product, risk],
    aggregator: outputs => [new ChatMessage(ChatRole.Assistant,
        string.Join("\n\n", outputs
            .Where(m => m.Count > 0)
            .Select((m, i) => $"--- Reviewer {i + 1} ---\n{m[^1].Text}")))]);

// A workflow runs through the same agent interface. Compose, don't rewrite.
AIAgent panel = workflow.AsAIAgent(name: "ReviewPanel");

const string proposal =
    "Ship an AI chat feature for our docs site in six weeks using the .NET AI chat template.";

AgentResponse review = await panel.RunAsync(proposal);

Console.WriteLine("=== Review panel ===");
Console.WriteLine(review.Text);
Console.WriteLine();
Console.WriteLine("Agents compose like middleware. One input, many specialists, one result.");
