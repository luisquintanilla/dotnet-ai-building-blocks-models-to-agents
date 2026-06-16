#:package OpenAI@2.11.0
#:package Microsoft.Extensions.AI.OpenAI@10.7.0
#:package Microsoft.Extensions.AI.Evaluation@10.7.0
#:package Microsoft.Extensions.AI.Evaluation.Quality@10.7.0

// Block 5 - Evaluations.
// The problem: how do you know the answer is good, and how do you stay good as
// you change prompts and models? You score it. Microsoft.Extensions.AI.Evaluation
// runs evaluators that use an IChatClient as the judge. Same foundation, one more
// block. This is the quality gate. It is not in the chat template; you add it in
// your test project and CI to keep the app honest.

using System.ClientModel;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using OpenAI;

string token = Environment.GetEnvironmentVariable("GITHUB_TOKEN")
    ?? throw new InvalidOperationException("Set GITHUB_TOKEN to a GitHub PAT with models:read. See README.");

IChatClient chat = new OpenAIClient(
        new ApiKeyCredential(token),
        new OpenAIClientOptions { Endpoint = new Uri("https://models.inference.ai.azure.com") })
    .GetChatClient("gpt-4.1-mini")
    .AsIChatClient();

// 1. Get the response we want to grade.
List<ChatMessage> conversation =
[
    new(ChatRole.System, "You are a concise .NET assistant."),
    new(ChatRole.User, "What interface does Microsoft.Extensions.AI use for chat?")
];

ChatResponse modelResponse = await chat.GetResponseAsync(conversation);
Console.WriteLine($"Answer: {modelResponse.Text}");
Console.WriteLine();

// 2. Pick the judge. The evaluators call this IChatClient to score. A stronger
//    model makes a better judge; gpt-4.1-mini keeps this sample simple.
ChatConfiguration judge = new(chat);

// 3. Compose the evaluators you care about. They run together.
IEvaluator evaluators = new CompositeEvaluator(
    new RelevanceEvaluator(),
    new CoherenceEvaluator(),
    new GroundednessEvaluator());

// Groundedness needs the source of truth to check the answer against.
GroundednessEvaluatorContext grounding = new(
    "Microsoft.Extensions.AI defines IChatClient as its provider-agnostic chat interface.");

// 4. Score it.
EvaluationResult result = await evaluators.EvaluateAsync(
    conversation, modelResponse, judge, additionalContext: [grounding]);

// 5. Read the metrics. Each one carries a score and the judge's reasoning.
foreach (string name in new[]
{
    RelevanceEvaluator.RelevanceMetricName,
    CoherenceEvaluator.CoherenceMetricName,
    GroundednessEvaluator.GroundednessMetricName
})
{
    NumericMetric metric = result.Get<NumericMetric>(name);
    Console.WriteLine($"{name,-13}: {metric.Value:F1}/5  ({metric.Interpretation?.Rating})");
}

Console.WriteLine();
Console.WriteLine("Now put this in CI. Score every change so quality never regresses.");
