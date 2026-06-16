#:package OpenAI@2.11.0
#:package Microsoft.Extensions.AI.OpenAI@10.7.0
#:package Microsoft.Extensions.AI@10.7.0

// Block 3 - Tools, part one: function calling.
// The problem: a model can reason about text but it can't run your code, read a
// clock, or call your API. You give it tools. AIFunctionFactory wraps a normal
// C# method as a tool. UseFunctionInvocation adds one piece of middleware that
// runs the tool when the model asks for it, then feeds the result back.

using System.ClientModel;
using System.ComponentModel;
using Microsoft.Extensions.AI;
using OpenAI;

string token = Environment.GetEnvironmentVariable("GITHUB_TOKEN")
    ?? throw new InvalidOperationException("Set GITHUB_TOKEN to a GitHub PAT with models:read. See README.");

IChatClient inner = new OpenAIClient(
        new ApiKeyCredential(token),
        new OpenAIClientOptions { Endpoint = new Uri("https://models.inference.ai.azure.com") })
    .GetChatClient("gpt-4.1-mini")
    .AsIChatClient();

// One line turns automatic tool calling on. Same builder pattern as ASP.NET Core.
IChatClient chat = inner
    .AsBuilder()
    .UseFunctionInvocation()
    .Build();

// Any C# method can be a tool. The description and parameter names become the
// schema the model sees, so name things clearly.
[Description("Gets the current local date and time.")]
string GetNow() => DateTime.Now.ToString("f");

[Description("Returns the number of days until a given month and day this year.")]
int DaysUntil(int month, int day)
{
    var target = new DateTime(DateTime.Now.Year, month, day);
    return (int)(target - DateTime.Now.Date).TotalDays;
}

ChatOptions options = new()
{
    Tools = [AIFunctionFactory.Create(GetNow), AIFunctionFactory.Create(DaysUntil)]
};

// The model decides which tools to call and in what order. We never call them.
ChatResponse response = await chat.GetResponseAsync(
    "What time is it right now, and how many days until December 25th?", options);

Console.WriteLine(response.Text);
Console.WriteLine();
Console.WriteLine("You wrote plain methods. The model called them. That's tools.");
