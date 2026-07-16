# EasyDocs AI Development Guide

## Scope

These instructions apply to the entire repository. Use the repository-local
`.agents/skills/dotnet-csharp/SKILL.md` for .NET or C# implementation, debugging,
testing, packaging, CLI, Markdown rendering, and static-site generation work.

Do not create reports, summaries, migration notes, or delivery documents unless
the user explicitly requests them. Keep documentation changes limited to
behavior, configuration, or workflows changed by the task.

## What This Repository Does

EasyDocs is a .NET 10 command-line tool (`ezdoc`) that turns Markdown content
into a static documentation and blog site.

The main execution flow is:

1. `src/BuildSite/Program.cs` parses `init` and `build`.
2. `src/Share/Command.cs` creates or reads `webinfo.json`.
3. `DocsBuilder` builds versioned/multilingual documentation, navigation,
   search data, edit links, and documentation homepages.
4. `HtmlBuilder` builds blogs, the homepage, about page, JSON data, assets, and
   `sitemap.xml`.
5. Markdig extensions in `src/Share/MarkdownExtension` convert links and render
   highlighted fenced code through the embedded ColorCode implementation.

## Repository Map

- `src/BuildSite`: .NET global-tool entry point and NuGet package metadata.
- `src/Share`: commands, builders, Markdown pipeline, embedded HTML templates,
  and the packaged static asset archive.
- `src/Models`: configuration and generated-site models.
- `src/ColorCode.Core`: language definitions, compiler, parser, and styles.
- `src/ColorCode.HTML`: HTML formatters for highlighted code.
- `src/ColorCode.Core.Tests`: MSTest regression tests for ColorCode parsing.
- `src/NodePackage`: experimental/stale Node API wrapper; do not treat it as
  the primary product path without first reconciling its API and frameworks.
- `Content`: sample/source Markdown content used by this repository.
- `WebApp`: checked-in generated preview plus the CSS/JavaScript files consumed
  by `pack.ps1`.
- `src/Share/template`: embedded HTML templates and `web.zip`, which supplies
  release-time static assets.

## Source-of-Truth Rules

- Treat `webinfo.json` and `src/Models/WebInfo.cs` together as the configuration
  contract. Preserve the existing JSON property name `ContetPath` unless a task
  explicitly includes a compatibility migration.
- Treat `Content/docs/<name>/<language>/<version>` and each `DocInfo` entry as
  one contract. Directory names must match configured names, languages, and
  versions.
- Use `.order` files without `.md` suffixes to control document and directory
  ordering.
- Keep template placeholders in `src/Share/template/*.tpl` synchronized with
  every `.Replace(...)` chain that renders the template.
- When changing files under `WebApp/css`, `WebApp/js`, or the packaged favicon,
  run `pack.ps1` so `src/Share/template/web.zip` receives the same changes.
- Do not hand-edit generated pages merely to change generator behavior. Fix the
  builder/template/source asset first, then regenerate the affected output.
- Preserve relative Markdown link conversion from `.md` to `.html`; absolute
  HTTP(S) links must remain unchanged.
- Preserve `BaseHref` behavior, including its required trailing slash and
  root-deployment value `/`.

## .NET and C# Conventions

- Target the frameworks already declared by each project. The primary generator
  projects target `net10.0`.
- Keep nullable reference types and implicit usings enabled.
- Prefer small, focused changes in the owning layer:
  models for configuration/data shape, builders for generation, Markdown
  extensions for parsing/rendering, and templates/assets for presentation.
- Use `Path.Combine`, `Path.GetRelativePath`, and explicit slash normalization
  at URL boundaries. Do not build filesystem paths with URL assumptions.
- Use ordinal-ignore-case comparisons for extensions, URLs, and paths where the
  existing behavior is case-insensitive.
- Keep file output deterministic and explicitly UTF-8 when writing generated
  text.
- Preserve user-facing CLI error shapes and localized messages when fixing a
  concrete failure.
- Avoid unrelated modernization of the vendored ColorCode implementation.
  Changes there require focused parser tests, especially for regex timeout or
  ReDoS behavior.

## Validation

Do not use `dotnet build EasyDocs.slnx` as the default validation command. The
solution contains a legacy Website entry for `WebApp`, and `dotnet` MSBuild
fails with `MSB4249` even when the SDK-style C# projects compile.

Use the narrowest relevant commands:

```powershell
dotnet build .\src\BuildSite\BuildSite.csproj -c Release
dotnet test .\src\ColorCode.Core.Tests\ColorCode.Core.Tests.csproj -c Release
```

Use Release when invoking the CLI from source:

```powershell
dotnet run --project .\src\BuildSite\BuildSite.csproj -c Release -- build .\webinfo.json
```

Debug builds call the local `Debug()` generation path before processing command
arguments and can modify generated output unexpectedly.

For generation changes, prefer a temporary config/content/output directory.
Verify representative generated HTML, JSON, copied images, links, `BaseHref`,
and sitemap/canonical output. If repository sample output is intentionally
regenerated, review the full diff and do not discard unrelated user changes.

Known baseline limitations:

- `BuildSite` currently compiles with existing warnings in
  `BetterCodeBlockRenderer.cs` and `Program.cs`.
- Restoring the test project may resolve `Microsoft.NET.Test.Sdk` 17.9.1 to
  17.10.0 with `NU1603`.
- `src/NodePackage` currently cannot build its `net8.0` target because `Share`
  targets only `net10.0`; its JavaScript command contract is also older than the
  current `Command.Build(string configPath)` API.

Do not fix these baseline issues unless they are in task scope, but do not
silently introduce additional warnings or failures.

