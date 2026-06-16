# Dev container

Spin this up when you want to run the samples or the presentation without installing
anything locally. You get the .NET 10 SDK, Node, and the GitHub CLI in one container.

Open it two ways:

- **Codespaces:** on GitHub, press <kbd>.</kbd> or use **Code → Codespaces → Create codespace**.
- **Local:** install Docker and the VS Code **Dev Containers** extension, then
  **Reopen in Container**.

## What it covers

- `samples/` — the .NET 10 file-based apps, run against GitHub Models.
- The RevealJS deck — `npm run preview`, served on port 8000 (auto-forwarded).

It does not cover `demo/ChatApp`. That app uses Aspire and a Docker container for PDF
conversion, so it needs Docker inside the container. If you want to run it here too, add
the docker-in-docker feature to `devcontainer.json`:

```jsonc
"features": {
  "ghcr.io/devcontainers/features/docker-in-docker:2": {}
}
```

Then follow `demo/README.md`.

## Set GITHUB_TOKEN for the samples

The samples read `GITHUB_TOKEN`, and that token needs the `models:read` permission.

**Codespaces.** The built-in Codespaces token does not include `models:read`, so add your own:

1. Create a fine-grained personal access token with the **Models** permission set to read.
   See [GitHub Models](https://github.com/marketplace/models).
2. Add it as a Codespaces secret named `GITHUB_TOKEN`, scoped to this repository:
   **Settings → Codespaces → Secrets**. Codespaces injects it on the next rebuild.

**Local Dev Containers.** Export the token on your host before you open the container:

```bash
export GITHUB_TOKEN=your_token_here
```

Then uncomment this line in `devcontainer.json` so the host value flows in:

```jsonc
"remoteEnv": { "GITHUB_TOKEN": "${localEnv:GITHUB_TOKEN}" }
```

We keep that line commented by default. `localEnv` is not available in Codespaces, so an
uncommented forward would overwrite the Codespaces secret with an empty value.

Either way, confirm it is set:

```bash
echo $GITHUB_TOKEN | head -c 8
```

## Use Azure OpenAI instead of GitHub Models

GitHub Models is the default in the samples because it needs only a token. When you want to
run against your own Azure OpenAI deployment, the Azure CLI in this container does the heavy
lifting. Sign in once:

```bash
az login --use-device-code
az account set --subscription "<your-subscription>"   # if you have more than one
```

You can inspect or create deployments without leaving the container:

```bash
# List Azure OpenAI resources you can reach
az cognitiveservices account list -o table

# List the model deployments on one resource
az cognitiveservices account deployment list \
  --name <resource-name> --resource-group <group> -o table
```

Then point a sample at Azure OpenAI. The cleanest path is keyless: `DefaultAzureCredential`
reuses your `az login`, so there is no key to manage. Swap the provider construction in a
sample for this (add the two packages at the top, and use your deployment name):

```csharp
#:package Azure.AI.OpenAI@*          // use the latest on NuGet.org
#:package Azure.Identity@*

using Azure.AI.OpenAI;
using Azure.Identity;

var endpoint = new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!);
AzureOpenAIClient provider = new(endpoint, new DefaultAzureCredential());

IChatClient chat = provider.GetChatClient("your-deployment-name").AsIChatClient();
```

Everything below the client stays the same, because it still sees `IChatClient`. For image
generation, the same swap applies with `provider.GetImageClient("your-image-deployment")`.

Keyless needs your signed-in identity to have the **Cognitive Services OpenAI User** role on
the resource. If you would rather use a key, set `AZURE_OPENAI_ENDPOINT` and pass an
`AzureKeyCredential` instead.

## Run things

```bash
# Samples
cd samples
dotnet run 01-chat.cs

# Presentation (build + serve on http://localhost:8000)
npm run preview
```

`npm install` already ran when the container was created, so the deck is ready to build.
