# AI Chat with Custom Data

This project is an AI chat application that demonstrates how to chat with custom data using an AI language model. Please note that this template is currently in an early preview stage. If you have feedback, please take a [brief survey](https://aka.ms/dotnet-chat-templatePreview2-survey).

>[!NOTE]
> Before running this project you need to configure the API keys or endpoints for the providers you have chosen. See below for details specific to your choices.

### Known Issues

#### Errors running Ollama or Docker

A recent incompatibility was found between Ollama and Docker Desktop. This issue results in runtime errors when connecting to Ollama, and the workaround for that can lead to Docker not working for Aspire projects.

This incompatibility can be addressed by upgrading to Docker Desktop 4.41.1. See [ollama/ollama#9509](https://github.com/ollama/ollama/issues/9509#issuecomment-2842461831) for more information and a link to install the version of Docker Desktop with the fix.

# Configure the AI Model Provider

## Using Azure OpenAI (keyless)

This app is configured for Azure OpenAI with **keyless** authentication. There is no API key to
manage — the client uses `DefaultAzureCredential` (your `az login`).

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

The app expects a chat deployment named `chat` and an embeddings deployment named `embedding`.
Adjust the names in `ChatApp.Web/Program.cs` if yours differ.

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
