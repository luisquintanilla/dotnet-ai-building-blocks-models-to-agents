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

## Run things

```bash
# Samples
cd samples
dotnet run 01-chat.cs

# Presentation (build + serve on http://localhost:8000)
npm run preview
```

`npm install` already ran when the container was created, so the deck is ready to build.
