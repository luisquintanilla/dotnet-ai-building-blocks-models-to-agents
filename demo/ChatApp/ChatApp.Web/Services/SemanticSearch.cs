using ChatApp.Web.Services.Ingestion;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace ChatApp.Web.Services;

public class SemanticSearch(
    VectorStoreCollection<string, IngestedChunk> vectorCollection,
    [FromKeyedServices("ingestion_directory")] DirectoryInfo ingestionDirectory,
    DataIngestor dataIngestor,
    IChatClient chatClient)
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

        var searchOptions = new VectorSearchOptions<IngestedChunk>
        {
            Filter = documentIdFilter is { Length: > 0 } ? record => record.DocumentId == documentIdFilter : null,
        };

        var fetchCount = Math.Max(maxResults * 3, maxResults);
        var queries = await ExpandSearchQueriesAsync(text);
        Dictionary<Guid, RankedChunk> rankedChunks = [];

        foreach (var query in queries)
        {
            var rank = 0;
            await foreach (var result in vectorCollection.SearchAsync(query, fetchCount, searchOptions))
            {
                rank++;
                if (!rankedChunks.TryGetValue(result.Record.Key, out var rankedChunk))
                {
                    rankedChunk = new(result.Record);
                    rankedChunks.Add(result.Record.Key, rankedChunk);
                }

                rankedChunk.Score += 1.0 / (60 + rank);
            }
        }

        return rankedChunks.Values
            .OrderByDescending(result => result.Score)
            .Take(maxResults)
            .Select(result => result.Chunk)
            .ToList();
    }

    private async Task<IReadOnlyList<string>> ExpandSearchQueriesAsync(string text)
    {
        var expanded = await RewriteQueryAsync(text);
        return string.Equals(text.Trim(), expanded, StringComparison.OrdinalIgnoreCase)
            ? [text]
            : [text, expanded];
    }

    private async Task<string> RewriteQueryAsync(string text)
    {
        try
        {
            var response = await chatClient.GetResponseAsync(
                $"Rephrase this search query in one concise sentence using likely document terms. Return only the query.\n\nQuery: {text}",
                new ChatOptions { MaxOutputTokens = 60 });

            var rewritten = response.Text?.Trim().Trim('"', '\'');
            return string.IsNullOrWhiteSpace(rewritten) ? text : rewritten;
        }
        catch
        {
            return text;
        }
    }

    private sealed class RankedChunk(IngestedChunk chunk)
    {
        public IngestedChunk Chunk { get; } = chunk;
        public double Score { get; set; }
    }
}
