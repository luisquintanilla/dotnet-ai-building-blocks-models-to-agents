using Microsoft.Extensions.AI;
using OpenAI;
using ChatApp.Web.Components;
using ChatApp.Web.Services;
using ChatApp.Web.Services.Ingestion;
using MEDIExtensions.DependencyInjection;
using MEDIExtensions.Retrieval;
using UglyToad.PdfPig.DataIngestion.Processors;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

var openai = builder.AddAzureOpenAIClient("openai");
openai.AddChatClient("gpt-5-mini")
    .UseFunctionInvocation()
    .UseOpenTelemetry(configure: c =>
        c.EnableSensitiveData = builder.Environment.IsDevelopment());
openai.AddEmbeddingGenerator("embedding");

var vectorStorePath = Path.Combine(AppContext.BaseDirectory, "vector-store.db");
var vectorStoreConnectionString = $"Data Source={vectorStorePath}";
builder.Services.AddSqliteVectorStore(_ => vectorStoreConnectionString);
builder.Services.AddSqliteCollection<string, IngestedChunk>(IngestedChunk.CollectionName, vectorStoreConnectionString);
builder.Services.AddSingleton<DataIngestor>();
builder.Services.AddSingleton<SemanticSearch>();
builder.Services.AddKeyedSingleton("ingestion_directory", new DirectoryInfo(Path.Combine(builder.Environment.WebRootPath, "Data")));

// Ingestion pipeline — compose document and chunk processors on top of the PdfPig vision reader.
// VisionOcrEnricher/VisionTableEnricher turn each rendered page image into real text + tables
// (PdfReadingMode.VisionOnly emits page-image placeholders that these enrichers fill in).
// ContextualChunkEnricher then writes a short "context" summary onto each chunk (IngestedChunk.Context).
builder.Services.AddIngestionPipeline()
    .UseDocumentProcessor<VisionOcrEnricher>()
    .UseDocumentProcessor<VisionTableEnricher>()
    .UseChunkProcessor<ContextualChunkEnricher>();

// Retrieval pipeline — rewrite the query, over-fetch, rerank by true relevance, then
// drop weak matches with a quality gate (CRAG). Composed from the real DataRetrieval abstraction.
builder.Services.AddRetrievalPipeline()
    .UseQueryExpansion()
    .UseLlmReranking()
    .UseCrag();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
