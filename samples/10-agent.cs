#:package OpenAI@2.11.0
#:package Microsoft.Extensions.AI.OpenAI@10.7.0
#:package Microsoft.Extensions.AI@10.7.0
#:package Microsoft.Agents.AI@1.10.0
#:package Microsoft.Agents.AI.Abstractions@1.10.0

// Block 6 - From blocks to agents.
// The problem: at some point you want the model to keep its own instructions,
// hold a conversation, and use tools on its own. That's an agent. Here is the
// graduated moment: you already have an IChatClient, so you wrap it. One line.
// No rewrite, no new mental model. ChatClientAgent is Microsoft Agent Framework.

using System.ClientModel;
using System.ComponentModel;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;

string token = Environment.GetEnvironmentVariable("GITHUB_TOKEN")
    ?? throw new InvalidOperationException("Set GITHUB_TOKEN to a GitHub PAT with models:read. See README.");

// The same IChatClient from Block 1.
IChatClient chat = new OpenAIClient(
        new ApiKeyCredential(token),
        new OpenAIClientOptions { Endpoint = new Uri("https://models.inference.ai.azure.com") })
    .GetChatClient("gpt-4o-mini")
    .AsIChatClient();

[Description("Returns the number of days until a given month and day this year.")]
int DaysUntil(int month, int day) =>
    (int)(new DateTime(DateTime.Now.Year, month, day) - DateTime.Now.Date).TotalDays;

// The one-liner. Give it a name, instructions, and the tools from Block 3.
AIAgent agent = new ChatClientAgent(
    chat,
    instructions: "You are a friendly .NET event planner. Be brief and upbeat.",
    name: "Planner",
    tools: [AIFunctionFactory.Create(DaysUntil)]);

// Run it like you'd expect.
AgentResponse first = await agent.RunAsync("How many days until December 25th?");
Console.WriteLine(first.Text);

// A session carries the conversation, so the agent remembers across turns.
AgentSession session = await agent.CreateSessionAsync();
await agent.RunAsync("My name is Luis and I'm planning a .NET meetup.", session);
AgentResponse remembered = await agent.RunAsync("What's my name?", session);

Console.WriteLine();
Console.WriteLine(remembered.Text);
Console.WriteLine();
Console.WriteLine("Same IChatClient, same tools, now an agent. That's the easy transition.");
