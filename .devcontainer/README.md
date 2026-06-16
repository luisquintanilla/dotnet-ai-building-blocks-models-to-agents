# Dev container

Spin this up when you want to run the samples, the presentation, or the demo app without
installing anything locally. You get the .NET 10 SDK, Node, the GitHub CLI, the Azure CLI,
and Docker in one container.

Open it two ways:

- **Codespaces:** on GitHub, press <kbd>.</kbd> or use **Code → Codespaces → Create codespace**.
- **Local:** install Docker and the VS Code **Dev Containers** extension, then
  **Reopen in Container**.

## What it covers

- `samples/` — the .NET 10 file-based apps, run against GitHub Models.
- The RevealJS deck — `npm run preview`, served on port 8000 (auto-forwarded).
- `demo/ChatApp` — the Aspire app. The Aspire CLI is preinstalled (devcontainer feature) and Docker
  is available through docker-in-docker, so `aspire run` can pull and run its markitdown container.
  Follow [`demo/README.md`](../demo/README.md) to configure Azure OpenAI (keyless) and run it.

## Set GITHUB_TOKEN for the samples

The samples read `GITHUB_TOKEN`. What you need depends on where you run.

**Codespaces.** Nothing to do. Codespaces provides `GITHUB_TOKEN` for you, and it works with
GitHub Models out of the box. The GitHub docs confirm this: running in a codespace needs no
token setup, while running locally needs a personal access token. See
[Prototyping with AI models](https://docs.github.com/en/github-models/use-github-models/prototyping-with-ai-models).

**Local Dev Containers.** Create a personal access token with the `models:read` permission
(see [GitHub Models](https://github.com/marketplace/models)), then export it on your host
before you open the container:

```bash
export GITHUB_TOKEN=your_token_here
```

Then uncomment this line in `devcontainer.json` so the host value flows in:

```jsonc
"remoteEnv": { "GITHUB_TOKEN": "${localEnv:GITHUB_TOKEN}" }
```

We keep that line commented by default. `localEnv` is empty in Codespaces, so an uncommented
forward would shadow the token Codespaces already provides.

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

# Demo app (Aspire + Azure OpenAI, keyless). Docker is available, so this works here too.
az login
cd demo/ChatApp
aspire run
```

`npm install` already ran when the container was created, so the deck is ready to build, and the
Aspire CLI is installed by the devcontainer feature. For the demo app, sign in with `az login` and
set its Azure OpenAI endpoint user-secret first, as shown in [`demo/README.md`](../demo/README.md).
