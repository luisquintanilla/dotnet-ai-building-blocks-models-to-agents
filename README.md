# From Models to Agents: The Essential Building Blocks of AI Apps

A conference talk and a standalone [reveal.js](https://revealjs.com) deck about building
AI apps in .NET. The idea is simple: modern AI apps are not "call a model." They are
composable, interoperable building blocks that live in the `Microsoft.Extensions.*` family
and fit into the .NET apps you already build.

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
| `talk/` | Speaker outline and abstract. |

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

## Deploy

A GitHub Actions workflow builds the deck and publishes it to GitHub Pages on every push to
`main`. Enable Pages with the "GitHub Actions" source in the repository settings.

## Credits

Fonts are Space Grotesk and Open Sans, both under the SIL Open Font License. Built with
reveal.js.
