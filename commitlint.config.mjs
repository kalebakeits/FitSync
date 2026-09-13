/**
 * Commit message rules, enforced by the commit-msg hook.
 *
 * Conventional Commits, with the scopes this repo actually uses. Note that
 * `scope-enum` only rejects scopes that are *not* in the list — it does not
 * require one — so `chore: bump deps` is still valid.
 */

const scopes = [
  "api",
  "gui",
  "purger",
  "provider",
  "wahoo",
  "garmin",
  "zwift",
  "shared",
  "db",
  "apphost",
  "sourcegen",
  "mock",
  "k8s",
  "ci",
  "deps",
  "repo",
  "release",
];

export default {
  extends: ["@commitlint/config-conventional"],
  rules: {
    "scope-enum": [2, "always", scopes],
    "header-max-length": [2, "always", 100],
  },

  // Only used by the optional interactive CLI (@commitlint/prompt-cli), but it
  // costs nothing to leave a useful list of scopes here.
  prompt: {
    questions: {
      type: {
        description: "What kind of change is this?",
      },
      scope: {
        description: `What is the scope? One of: ${scopes.join(", ")} (or leave empty).`,
      },
      subject: {
        description:
          "Short imperative description, e.g. 'correct token refresh' (no trailing period).",
      },
    },
    settings: {
      enableMultipleScopes: false,
    },
  },
};
