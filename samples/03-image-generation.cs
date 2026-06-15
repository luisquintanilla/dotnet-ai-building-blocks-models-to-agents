#:package OpenAI@2.11.0
#:package Microsoft.Extensions.AI.OpenAI@10.7.0

// Block 1 - Models are multimodal.
// The problem: the Models block is not just chat. Your app can generate images
// too, and you still want one abstraction when the provider changes.
// Microsoft.Extensions.AI gives you IImageGenerator for that.

#pragma warning disable MEAI001

using System.ClientModel;
using Microsoft.Extensions.AI;
using OpenAI;

// Image generation needs an image-capable endpoint. GitHub Models may not host
// one. For OpenAI, replace the provider construction below with:
// OpenAIClient provider = new(new ApiKeyCredential(token));
// For Azure OpenAI, point the endpoint at your Azure OpenAI resource and set
// this constant to your image deployment. The IImageGenerator code below is
// unchanged.
const string ImageModelOrDeployment = "dall-e-3";

string token = Environment.GetEnvironmentVariable("GITHUB_TOKEN")
    ?? throw new InvalidOperationException("Set GITHUB_TOKEN to a GitHub PAT with models:read. See README.");

OpenAIClient provider = new(
    new ApiKeyCredential(token),
    new OpenAIClientOptions { Endpoint = new Uri("https://models.inference.ai.azure.com") });

IImageGenerator generator = provider.GetImageClient(ImageModelOrDeployment).AsIImageGenerator();

ImageGenerationRequest request = new("A small friendly robot building a .NET app, watercolor icon style.");
ImageGenerationOptions options = new()
{
    Count = 1,
    MediaType = "image/png",
    ResponseFormat = ImageGenerationResponseFormat.Data
};

ImageGenerationResponse response = await generator.GenerateAsync(request, options);

if (response.Contents.Count == 0)
{
    throw new InvalidOperationException("The provider did not return an image.");
}

AIContent image = response.Contents[0];
switch (image)
{
    case DataContent data when data.Data.Length > 0:
        string outputPath = Path.GetFullPath("image-generation-output.png");
        await File.WriteAllBytesAsync(outputPath, data.Data.ToArray());
        Console.WriteLine($"Wrote image bytes to {outputPath}");
        break;

    case UriContent uri:
        Console.WriteLine($"Image URL: {uri.Uri}");
        break;

    case DataContent data when !string.IsNullOrWhiteSpace(data.Uri):
        Console.WriteLine($"Image URL: {data.Uri}");
        break;

    default:
        Console.WriteLine($"The provider returned {image.GetType().Name} content.");
        Console.WriteLine(image);
        break;
}

// That's it! Swap the provider or endpoint for an image-capable service. The
// prompt and IImageGenerator code stay the same.



