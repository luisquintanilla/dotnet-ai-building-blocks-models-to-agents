// Build script: assembles the static site into public/ for GitHub Pages.
// Copies the deck (index.html, slides.md), styles, assets, and the
// reveal.js dist + plugins from node_modules. Zero extra dependencies.

import { cpSync, rmSync, mkdirSync, existsSync } from "node:fs";
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

// Vendor reveal.js (dist + the plugins we load) into public/reveal/.
const reveal = "node_modules/reveal.js";
if (!existsSync(reveal)) {
  console.error("reveal.js not found. Run `npm install` first.");
  process.exit(1);
}
cpSync(join(reveal, "dist"), join(out, "reveal", "dist"), { recursive: true });
cpSync(join(reveal, "plugin"), join(out, "reveal", "plugin"), { recursive: true });

console.log("Built site into ./public");
