using ChatApp.Web.Services.Ingestion;
using Microsoft.Extensions.DataRetrieval;
using Microsoft.Extensions.VectorData;

namespace ChatApp.Web.Services;

public class SemanticSearch(
    VectorStoreCollection<string, IngestedChunk> vectorCollection,
    [FromKeyedServices("ingestion_directory")] DirectoryInfo ingestionDirectory,
    DataIngestor dataIngestor,
    RetrievalPipeline retrievalPipeline)
{
    private Task? _ingestionTask;

    public async Task LoadDocumentsAsync() => await ( _ingestionTask ??= dataIngestor.IngestDataAsync(ingestionDirectory, searchPattern: "*.*"));

    public async Task<IReadOnlyList<IngestedChunk>> SearchAsync(string text, string? documentIdFilter, int maxResults)
    {
        // Ensure documents have been loaded before searching
        await LoadDocumentsAsync();

        if (maxResults <= 0)
        {
            return [];
        }

        // The real DataRetrieval pipeline: rewrite the query, over-fetch, rerank by true
        // relevance, then drop weak matches with a quality gate (CRAG). Composed in Program.cs.
        var results = await retrievalPipeline.RetrieveAsync(
            vectorCollection,
            text,
            topK: maxResults,
            contentSelector: chunk => chunk.Text,
            cancellationToken: default);

        LastRetrievalMetadata = results.Metadata;

        // Map RetrievalChunks back to IngestedChunk records for the UI
        var chunks = results.Chunks
            .Select(c =>
            {
                c.Record.TryGetValue(nameof(IngestedChunk.Key), out var keyObj);
                c.Record.TryGetValue(nameof(IngestedChunk.DocumentId), out var docIdObj);
                return new IngestedChunk
                {
                    Key = keyObj?.ToString() ?? "",
                    DocumentId = docIdObj?.ToString() ?? "",
                    Text = c.Content
                };
            })
            .Where(c => documentIdFilter is not { Length: > 0 } || c.DocumentId == documentIdFilter)
            .ToList();

        return chunks;
    }

    /// <summary>
    /// Pipeline metadata from the last retrieval (e.g., CRAG score, reranking info).
    /// </summary>
    public IDictionary<string, object?>? LastRetrievalMetadata { get; private set; }
}
