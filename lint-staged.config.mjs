/**
 * pre-commit tasks, run by lint-staged from the repo root.
 *
 * Each key is a glob; the value receives the staged files matching it.
 * lint-staged appends nothing to a function — the function builds the whole
 * command.
 */

import path from "node:path";

/** POSIX-quote a path so a filename can never break out of the argument. */
const shellQuote = (file) => `'${file.replaceAll("'", "'\\''")}'`;

/**
 * `dotnet format --include` matches paths relative to the solution, so an
 * absolute path silently matches nothing: it exits 0 having changed no files.
 * lint-staged hands over absolute paths, so convert them back.
 */
const toRepoRelative = (file) =>
  path.relative(process.cwd(), path.resolve(file));

export default {
  /**
   * C#. `dotnet format` takes a project or solution, not a file list, so it is
   * pointed at the solution and narrowed with --include. Loading the workspace
   * (MSBuild evaluation) is the cost — measured at roughly 4-7s for a single
   * staged file on this repo, one invocation for all staged .cs files.
   */
  "*.cs": (files) =>
    `dotnet format whitespace FitSync.sln --include ${files
      .map((file) => shellQuote(toRepoRelative(file)))
      .join(" ")} --verbosity minimal`,

  /** Gui (React/TS) plus JSON/YAML/Markdown config anywhere in the repo. */
  "*.{ts,tsx,js,jsx,mjs,cjs,json,css,scss,html,yaml,yml,md}": [
    "prettier --write",
  ],
};
