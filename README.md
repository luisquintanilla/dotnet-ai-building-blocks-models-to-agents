# From Models to Agents: The Essential Building Blocks of AI Apps

A conference talk and a standalone [reveal.js](https://revealjs.com) deck about building
AI apps in .NET. The idea is simple: modern AI apps are not "call a model." They are
composable, interoperable building blocks: the AI primitives every app needs, built the .NET way,
that fit into the .NET apps you already build.

The talk works bottom-up. We start with one model call, add a block only when the problem
asks for it, graduate to [Microsoft Agent Framework](https://learn.microsoft.com/agent-framework/),
then show the production [.NET AI Chat Template](https://learn.microsoft.com/dotnet/ai/quickstarts/ai-templates)
as "here's everything together."

## What's in here

| Path | What it is |
| --- | --- |
| `slides.md` | The deck. Plain Markdown, so it doubles as a portable outline. |
| `index.html` | reveal.js bootstrap that renders `slides.md`. |
| `css/` | The .NET brand theme (`theme.css`) and slide components (`components.css`). |
| `assets/diagrams/` | Hand-authored SVG architecture diagrams. |
| `assets/fonts/` | Self-hosted Space Grotesk + Open Sans (woff2). |
| `samples/` | Runnable .NET 10 file-based apps, one per building block. |
| `demo/` | The .NET AI Chat Template (Aspire), the baseline app. The `advanced-demo` branch enhances it. |
| `talk/` | Speaker outline and abstract. |

## Run in a dev container

Want a ready-made environment? This repo ships a [dev container](.devcontainer/devcontainer.json)
with the .NET 10 SDK, Node, and the GitHub CLI. Open it in a GitHub Codespace or locally with the
VS Code Dev Containers extension, and you can run the samples and the presentation right away. See
[`.devcontainer/README.md`](.devcontainer/README.md) for token setup. The container does not run
`demo/ChatApp` (that needs Docker and Aspire).

## Run the deck locally

You need [Node.js](https://nodejs.org) 18+.

```bash
npm install
npm run preview
```

Then open http://localhost:8000. Press `S` for the speaker view with notes.

- `npm run build` writes the static site into `public/`.
- `npm run serve` serves an already-built `public/` without rebuilding.

## Run the samples

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download) and a
[GitHub Models](https://github.com/marketplace/models) token. See
[`samples/README.md`](samples/README.md) for details.

```bash
cd samples
dotnet run 01-chat.cs
```

## Run the demo app

The `demo/ChatApp` folder is the production .NET AI Chat Template, scaffolded with Aspire. It's
the "everything together" app from the talk. An enhanced version (layout-aware ingestion plus
better retrieval) lives on the `advanced-demo` branch, and `git diff main..advanced-demo` is the
"upgrade, not rewrite" demo. See [`demo/README.md`](demo/README.md) for prerequisites and how to run.

## Deploy

A GitHub Actions workflow builds the deck and publishes it to GitHub Pages on every push to
`main`. Enable Pages with the "GitHub Actions" source in the repository settings.

## Credits

Fonts are Space Grotesk and Open Sans, both under the SIL Open Font License. Built with
reveal.js.
