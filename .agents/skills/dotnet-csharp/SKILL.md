---
name: dotnet-csharp
description: Develop, debug, test, and review EasyDocs .NET/C# code using the repository's actual CLI, static-site generation, Markdig, ColorCode, configuration, template, packaging, and MSTest conventions. Use for changes under src/**/*.cs or *.csproj; ezdoc init/build behavior; webinfo.json models; documentation/blog generation; Markdown rendering and syntax highlighting; template placeholder wiring; NuGet tool packaging; Node API interop with the .NET projects; or investigation of .NET build and test failures in this repository.
---

# EasyDocs .NET/C#

## Start With the Owning Path

Read `AGENTS.md`, then inspect the smallest end-to-end path that owns the
requested behavior. Do not infer the architecture only from project names.

- CLI parsing or help: `src/BuildSite/Program.cs`
- Initialization/build orchestration: `src/Share/Command.cs`
- Site-wide path, catalog, Git history, canonical, and navigation behavior:
  `src/Share/Builders/BaseBuilder.cs`
- Blogs, homepage, about page, site JSON, assets, and sitemap:
  `src/Share/Builders/HtmlBuilder.cs`
- Versioned/multilingual docs, document search, tree navigation, and edit links:
  `src/Share/Builders/DocsBuilder.cs`
- Configuration and generated data contracts: `src/Models`
- Markdown transformations: `src/Share/MarkdownExtension`
- Syntax parsing/highlighting: `src/ColorCode.Core` and `src/ColorCode.HTML`
- Packaged page skeletons/assets: `src/Share/template` and `WebApp`
- Parser regression tests: `src/ColorCode.Core.Tests`

Use `rg` to find all producers and consumers before changing a public property,
template placeholder, generated filename, URL shape, command signature, or
embedded resource name.

## Preserve the Generation Contracts

Treat these boundaries as coupled:

- `WebInfo` properties, `webinfo.json`, README examples, and generated
  `data/webinfo.json`
- `DocInfo` entries and
  `Content/docs/<DocName>/<language>/<version>` directories
- `.order` values and catalog names without `.md` extensions
- `.tpl` placeholders and the builder `.Replace(...)` calls that fill them
- `BaseHref`, filesystem-relative paths, generated links, canonical URLs, and
  sitemap URLs
- `WebApp` source assets, `pack.ps1`, and embedded
  `src/Share/template/web.zip`
- language IDs/aliases, regex rules, parser scopes, and ColorCode tests

Preserve the misspelled serialized property `ContetPath` unless the task
explicitly requires a backward-compatible migration. Renaming it directly
breaks existing JSON configuration.

Remember that the current .NET CLI build command accepts one configuration path:

```text
ezdoc build <configPath>
```

Do not implement against stale help text or the older Node wrapper's
`Build(source, output)` call.

## Implement .NET/C# Changes

Keep changes in the owning layer and follow the existing SDK/project settings:

- Keep `net10.0`, nullable reference types, implicit usings, and modern C#
  collection/raw-string syntax unless compatibility scope says otherwise.
- Use models for durable configuration/data shapes rather than anonymous
  cross-layer contracts.
- Use builders for generation and orchestration. Keep reusable path/catalog
  behavior in `BaseBuilder`.
- Extend Markdig through `IMarkdownExtension` and renderer replacement instead
  of post-processing generated HTML when an AST/renderer solution exists.
- Use the existing `Languages` repository and HTML formatters for fenced-code
  support. Add aliases and tests together.
- Use `Path` APIs for filesystem paths. Normalize to `/` only when producing a
  URL or serialized web path.
- Use `StringComparison.OrdinalIgnoreCase` for case-insensitive extension and
  protocol checks.
- Write generated text with UTF-8 and keep serialization deterministic.
- Keep console behavior concise; preserve the exact error message when
  addressing a reported CLI failure.
- Do not refactor unrelated vendored ColorCode files during generator work.

## Change Templates and Assets Safely

For HTML layout or metadata changes:

1. Find the relevant template in `src/Share/template`.
2. Find every builder replacement for its placeholders.
3. Add or remove placeholders and replacements in the same change.
4. Generate a representative site and search generated output for unresolved
   `@{...}` placeholders.

For CSS, JavaScript, SVG, or favicon changes:

1. Edit the source file under `WebApp`.
2. Check whether `pack.ps1` includes it in `$filePaths`.
3. Run `.\pack.ps1` when the embedded release asset archive must change.
4. Review both the source asset diff and `src/Share/template/web.zip`.

Do not patch only checked-in generated HTML when the intended change belongs to
a template, builder, or source asset.

## Add Tests at the Right Level

Use MSTest conventions already present in `src/ColorCode.Core.Tests` for parser
and language regressions:

- derive language suites from `LanguageParsingTestBase`
- use `[TestClass]`, `[TestInitialize]`, and `[TestMethod]`
- use `VerifyParsingCompletes` for regex/backtracking risks
- test aliases case-insensitively
- keep malicious or unclosed large input coverage for ReDoS fixes

The site generator has no dedicated automated test project. For generator
behavior, either add focused tests when the task justifies new test
infrastructure or perform an isolated end-to-end generation using temporary
content/config/output. Assert the files and HTML/JSON fragments that express the
reported behavior; do not rely only on command exit code.

## Validate Proportionally

Never use the solution as the default build:

```powershell
dotnet build .\EasyDocs.slnx -c Release
```

It includes `WebApp` as a legacy Website project and fails under `dotnet`
MSBuild with `MSB4249`.

Use:

```powershell
dotnet build .\src\BuildSite\BuildSite.csproj -c Release
dotnet test .\src\ColorCode.Core.Tests\ColorCode.Core.Tests.csproj -c Release
```

Allow restore on the first test run. The current package graph may emit
`NU1603` because `Microsoft.NET.Test.Sdk` 17.9.1 resolves to 17.10.0.

Run source CLI checks in Release:

```powershell
dotnet run --project .\src\BuildSite\BuildSite.csproj -c Release -- build <configPath>
```

Debug compilation includes a call to `Debug()` before argument processing and
can generate the repository sample site as a side effect.

For end-to-end checks, create a temporary tree containing:

- a `webinfo.json` with explicit `ContetPath` and `OutputPath`
- at least one blog, `about.md`, and one configured doc/language/version
- relative Markdown links and an image if path behavior changed
- `BaseHref` and `Domain` values if URL behavior changed

Inspect generated HTML, `data/*.json`, documentation search JSON, copied assets,
canonical links, edit links, and `sitemap.xml` as relevant.

## Handle Known Baselines

- Do not report `MSB4249` from `EasyDocs.slnx` as a regression in the C#
  projects.
- `BuildSite` has existing warnings in `BetterCodeBlockRenderer.cs` and
  `Program.cs`; avoid adding new warnings.
- `src/NodePackage` currently targets `net8.0;net10.0` while `Share` targets only
  `net10.0`, so a full NodePackage build fails for `net8.0`.
- The Node wrapper uses the old `ezblog` name and an obsolete two-argument
  `Command.Build` call. Reconcile the wrapper deliberately if Node support is
  in scope; otherwise validate the NuGet tool path.

Finish by reviewing `git diff`, generated/binary changes, and `git status`.
Keep only task-relevant generated output and preserve pre-existing user changes.
