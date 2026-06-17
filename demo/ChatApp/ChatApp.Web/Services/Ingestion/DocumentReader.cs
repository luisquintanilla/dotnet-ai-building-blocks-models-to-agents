using Microsoft.Extensions.DataIngestion;
using UglyToad.PdfPig.DataIngestion;

namespace ChatApp.Web.Services.Ingestion;

internal sealed class DocumentReader(DirectoryInfo rootDirectory) : IngestionDocumentReader
{
    private readonly MarkdownReader _markdownReader = new();
    // PDFs are extracted with a vision model (the gpt-5-mini deployment),
    // layout and all. No ONNX model file, no external converter container.
    private readonly PdfPigReader _pdfReader = new(mode: PdfReadingMode.VisionOnly);

    public override Task<IngestionDocument> ReadAsync(FileInfo source, string identifier, string? mediaType = null, CancellationToken cancellationToken = default)
    {
        if (Path.IsPathFullyQualified(identifier))
        {
            // Normalize the identifier to its relative path
            identifier = Path.GetRelativePath(rootDirectory.FullName, identifier);
        }

        mediaType = GetCustomMediaType(source) ?? mediaType;
        return base.ReadAsync(source, identifier, mediaType, cancellationToken);
    }

    public override Task<IngestionDocument> ReadAsync(Stream source, string identifier, string mediaType, CancellationToken cancellationToken = default)
        => mediaType switch
        {
            "application/pdf" => _pdfReader.ReadAsync(source, identifier, mediaType, cancellationToken),
            "text/markdown" => _markdownReader.ReadAsync(source, identifier, mediaType, cancellationToken),
            _ => throw new InvalidOperationException($"Unsupported media type '{mediaType}'"),
        };

    private static string? GetCustomMediaType(FileInfo source)
        => source.Extension switch
        {
            ".md" => "text/markdown",
            _ => null
        };
}
