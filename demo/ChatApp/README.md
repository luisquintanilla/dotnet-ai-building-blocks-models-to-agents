# AI Chat with Custom Data

This project is an AI chat application that demonstrates how to chat with custom data using an AI language model. Please note that this template is currently in an early preview stage. If you have feedback, please take a [brief survey](https://aka.ms/dotnet-chat-templatePreview2-survey).

>[!NOTE]
> Before running this project you need to configure the API keys or endpoints for the providers you have chosen. See below for details specific to your choices.

# Configure the AI Model Provider

## Using Azure OpenAI (keyless)

This app is configured for Azure OpenAI with **keyless** authentication. There is no API key to
manage. The client uses `DefaultAzureCredential` (your `az login`).

1. Sign in and make sure you have access (one time):

   ```sh
   az login
   ```

   Your identity needs the **Cognitive Services OpenAI User** role on the resource.

2. Set the **endpoint only** (no key) as a user secret on the app host:

   ```sh
   cd ChatApp.AppHost
   dotnet user-secrets set ConnectionStrings:openai "https://<your-resource>.openai.azure.com/"
   ```

The app expects a **vision-capable** chat deployment named `gpt-5-mini` and an embeddings
deployment named `embedding`. Adjust the names in `ChatApp.Web/Program.cs` if yours differ.
The chat model must support image input, because the PDF reader renders pages to images and
uses the chat model for OCR (see "Advanced RAG building blocks" below).

Learn more about [keyless authentication for Azure OpenAI](https://learn.microsoft.com/azure/ai-services/openai/how-to/managed-identity).

# Running the application

## Using the Aspire CLI

Sign in, then run from the `ChatApp` folder:

```sh
az login
aspire run
```

The Aspire dashboard opens; launch the web app from there.

## Using Visual Studio

1. Open the `.sln` file in Visual Studio.
2. Press `Ctrl+F5` or click the "Start" button in the toolbar to run the project.

## Using Visual Studio Code

1. Open the project folder in Visual Studio Code.
2. Install the [C# Dev Kit extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) for Visual Studio Code.
3. Once installed, Open the `Program.cs` file in the ChatApp.AppHost project.
4. Run the project by clicking the "Run" button in the Debug view.

## Trust the localhost certificate

Several Aspire templates include ASP.NET Core projects that are configured to use HTTPS by default. If this is the first time you're running the project, an exception might occur when loading the Aspire dashboard. This error can be resolved by trusting the self-signed development certificate with the .NET CLI.

See [Troubleshoot untrusted localhost certificate in Aspire](https://learn.microsoft.com/dotnet/aspire/troubleshooting/untrusted-localhost-certificate) for more information.

# Updating JavaScript dependencies

This template leverages JavaScript libraries to provide essential functionality. These libraries are located in the wwwroot/lib folder of the ChatApp.Web project. For instructions on updating each dependency, please refer to the README.md file in each respective folder.

# Learn More
To learn more about development with .NET and AI, check out the following links:

* [AI for .NET Developers](https://learn.microsoft.com/dotnet/ai/)

# Advanced RAG building blocks (advanced-demo branch)

This branch upgrades the scaffolded template to use the real, in-flight RAG building
blocks from the .NET AI stack. The Blazor UI and the shape of the app stay the same. Two
blocks change:

- **Ingestion** uses `PdfPigReader(PdfReadingMode.VisionOnly)`. It renders each PDF page to
  an image and uses the vision-capable chat model (`gpt-5-mini`) to OCR the text, then runs
  document and chunk enrichers. There is **no ONNX model and no extra container** for
  ingestion. This is the same vision path as the `PdfAIngest` reference app.
- **Retrieval** uses the real `Microsoft.Extensions.DataRetrieval` `RetrievalPipeline`,
  composed with `AddRetrievalPipeline().UseQueryExpansion().UseLlmReranking().UseCrag()`.
  CRAG grades each result and flags low-confidence answers instead of guessing. This
  replaces the template's hand-rolled query rewrite and rank fusion.

### Vendored packages

These building blocks ship from open PRs, so the branch vendors a coherent `-dev` package
set in `local-packages/` and adds a `local` NuGet source alongside nuget.org (see
`nuget.config`). Nothing extra to install. The set covers `Microsoft.Extensions.AI`,
`Microsoft.Extensions.DataIngestion`, `Microsoft.Extensions.DataRetrieval`, `MEDIExtensions`
(concrete processors + fluent DI), and `UglyToad.PdfPig.DataIngestion` (the vision reader).

### Pre-ingest once before a live demo

Vision OCR ingestion calls the chat model for every page, so the first ingest is slow. The
vector store is **SqliteVec** and persists to disk, so you only pay that cost once. Run the
app once ahead of time to ingest the sample PDFs, then the live run just queries the store.
If you change the source documents, delete the SqliteVec database file to force a re-ingest.
