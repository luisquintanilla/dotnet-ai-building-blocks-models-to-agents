// Build script: assembles the static site into public/ for GitHub Pages.
// Copies the deck (index.html, slides.md), styles, assets, and the
// reveal.js dist + plugins from node_modules. Zero extra dependencies.

import { cpSync, rmSync, mkdirSync, existsSync, readFileSync, writeFileSync } from "node:fs";
import { createHash } from "node:crypto";
import { join } from "node:path";

const out = "public";

rmSync(out, { recursive: true, force: true });
mkdirSync(out, { recursive: true });

// Files and folders that make up the deck.
const items = ["index.html", "slides.md", "css", "assets"];
for (const item of items) {
  if (existsSync(item)) {
    cpSync(item, join(out, item), { recursive: true });
  }
}

// Cache-bust the deck assets. Browsers HTTP-cache <link> stylesheets, so a
// content change to the CSS could otherwise be paired with a stale cached copy
// on a returning viewer, collapsing the custom layout. We stamp a short hash of
// the styles + deck onto every asset URL so any change forces a fresh fetch.
const hashSource = ["css/theme.css", "css/components.css", "slides.md"]
  .filter(existsSync)
  .map((f) => readFileSync(f))
  .reduce((h, buf) => h.update(buf), createHash("sha256"));
const ver = hashSource.digest("hex").slice(0, 10);

const indexPath = join(out, "index.html");
let html = readFileSync(indexPath, "utf8");
html = html
  .replace('href="css/theme.css"', `href="css/theme.css?v=${ver}"`)
  .replace('href="css/components.css"', `href="css/components.css?v=${ver}"`)
  .replace('data-markdown="slides.md"', `data-markdown="slides.md?v=${ver}"`);
writeFileSync(indexPath, html);

// Vendor reveal.js (dist + the plugins we load) into public/reveal/.
const reveal = "node_modules/reveal.js";
if (!existsSync(reveal)) {
  console.error("reveal.js not found. Run `npm install` first.");
  process.exit(1);
}
cpSync(join(reveal, "dist"), join(out, "reveal", "dist"), { recursive: true });
cpSync(join(reveal, "plugin"), join(out, "reveal", "plugin"), { recursive: true });

console.log("Built site into ./public");
