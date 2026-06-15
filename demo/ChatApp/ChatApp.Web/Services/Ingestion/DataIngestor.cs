using Microsoft.Extensions.AI;
using Microsoft.Extensions.DataIngestion;
using Microsoft.Extensions.DataIngestion.Chunkers;
using Microsoft.Extensions.VectorData;
using Microsoft.ML.Tokenizers;

namespace ChatApp.Web.Services.Ingestion;

public class DataIngestor(
    ILogger<DataIngestor> logger,
    ILoggerFactory loggerFactory,
    VectorStore vectorStore,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    IChatClient chatClient)
{
    public async Task IngestDataAsync(DirectoryInfo directory, string searchPattern)
    {
        EnricherOptions enricherOptions = new(chatClient)
        {
            LoggerFactory = loggerFactory,
            ChatOptions = new() { MaxOutputTokens = 120 }
        };

        using var writer = new VectorStoreWriter<string>(vectorStore, dimensionCount: IngestedChunk.VectorDimensions, new()
        {
            CollectionName = IngestedChunk.CollectionName,
            DistanceFunction = IngestedChunk.VectorDistanceFunction,
            IncrementalIngestion = false,
        });

        using var pipeline = new IngestionPipeline<string>(
            reader: new DocumentReader(directory),
            chunker: new SemanticSimilarityChunker(embeddingGenerator, new(TiktokenTokenizer.CreateForModel("gpt-4o"))),
            writer: writer,
            loggerFactory: loggerFactory)
        {
            ChunkProcessors =
            {
                new SummaryEnricher(enricherOptions, maxWordCount: 60)
            }
        };

        await foreach (var result in pipeline.ProcessAsync(directory, searchPattern))
        {
            logger.LogInformation("Completed processing '{id}'. Succeeded: '{succeeded}'.", result.DocumentId, result.Succeeded);
        }
    }
}
