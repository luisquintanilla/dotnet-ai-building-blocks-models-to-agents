#:package Azure.AI.OpenAI@2.9.0-beta.1
#:package Microsoft.Extensions.AI.OpenAI@10.7.0
#:package Azure.Identity@1.21.0

// Block 1 - Models are multimodal.
// The problem: the Models block is not just chat. Your app can generate images
// too, and you still want one abstraction when the provider changes.
// Microsoft.Extensions.AI gives you IImageGenerator for that.
//
// GitHub Models doesn't host an image model, so this sample points at your own
// image-capable endpoint. Nothing here is hard-coded: you bring the endpoint and
// deployment through environment variables, and auth is keyless.
//   AZURE_OPENAI_ENDPOINT          your resource, e.g. https://YOUR-RESOURCE.openai.azure.com/
//   AZURE_OPENAI_IMAGE_DEPLOYMENT  your image deployment name (default: gpt-image-1-mini)
// DefaultAzureCredential signs in with your `az login` (or managed identity in
// the cloud), so there is no API key to copy or leak. See the README for setup.
// Prefer OpenAI instead of Azure? Swap the provider line for
// `new OpenAIClient(new ApiKeyCredential(Environment.GetEnvironmentVariable("OPENAI_API_KEY")))`
// and the IImageGenerator code below stays the same.

#pragma warning disable MEAI001

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;

string? endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
if (string.IsNullOrWhiteSpace(endpoint))
{
    Console.WriteLine("This sample needs an image-capable endpoint, which GitHub Models doesn't host.");
    Console.WriteLine("Set AZURE_OPENAI_ENDPOINT to your own Azure OpenAI resource (and optionally");
    Console.WriteLine("AZURE_OPENAI_IMAGE_DEPLOYMENT), then run again. Auth is keyless via `az login`.");
    Console.WriteLine("See the README for setup and docs: https://learn.microsoft.com/dotnet/ai/");
    return;
}

string deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_IMAGE_DEPLOYMENT") ?? "gpt-image-1-mini";

AzureOpenAIClient provider = new(new Uri(endpoint), new DefaultAzureCredential());

IImageGenerator generator = provider.GetImageClient(deployment).AsIImageGenerator();

ImageGenerationRequest request = new("A small friendly robot building a .NET app, watercolor icon style.");
ImageGenerationOptions options = new()
{
    Count = 1,
    MediaType = "image/png"
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

// That's it! The endpoint and deployment came from your environment, the prompt
// and IImageGenerator code stay the same. Point them at any image-capable
// provider and the rest of your app never changes.
