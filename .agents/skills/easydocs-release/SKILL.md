---
name: easydocs-release
description: >-
  Run the authorized EasyDocs release workflow: pass Release unit tests, bump
  the patch version and release notes, update the changelog and affected
  README documentation, push the current branch, fast-forward and push the
  nuget branch, then return to the starting branch.
metadata:
  short-description: Release EasyDocs to NuGet
---

# EasyDocs Release

Use this skill only when the user explicitly requests an EasyDocs release or
NuGet publication. The workflow changes local and remote Git state. Never
force-push, reset, discard unrelated work, create a tag, or publish directly
to NuGet unless the user separately requests it.

## Preflight

Run these read-only checks from the repository root before changing files:

```powershell
$releaseStartBranch = git branch --show-current
git status --short
git branch -vv
git remote -v
git fetch origin --prune
```

Remember `$releaseStartBranch` for the final branch switch. Stop if the
repository is detached, the starting branch is `nuget`, the release scope is
unclear, or unrelated worktree changes cannot be separated from the release.
Stage only paths that belong to the requested release. Do not use stash, reset,
or checkout commands to hide or discard user changes.

If commands are run in separate shell sessions, record the branch output and
replace `$releaseStartBranch` with that literal branch name in later commands;
do not assume the shell variable persists.

The normal source branch is `dev` and the publication branch is `nuget`, but
use the captured starting branch as the source unless the user specifies
otherwise. After fetching, stop and report the exact divergence if the source
branch or `nuget` has remote-only commits, or if `nuget` cannot be advanced by
fast-forwarding from the source branch.

## 0. Run unit tests first

Run the Release unit tests serially, before changing the version, release
notes, changelog, or README files:

```powershell
dotnet test .\tests\Share.Tests\Share.Tests.csproj -c Release
dotnet test .\src\ColorCode.Core.Tests\ColorCode.Core.Tests.csproj -c Release
```

Do not continue to version, stage, commit, merge, or push if either command
fails. A platform-specific skipped test is acceptable when the test runner
reports it without failures.

## 1. Update version and package notes

Read `<Version>` from `src/BuildSite/BuildSite.csproj`. If the user did not
specify a version, increment only the patch component (`2.3.6` → `2.3.7`).
Confirm the new version is not already released. Update
`<PackageReleaseNotes>` with concise notes describing the actual changes in
this release; replace stale notes rather than accumulating unrelated history.

After the edit, validate the CLI project in Release:

```powershell
dotnet build .\src\BuildSite\BuildSite.csproj -c Release
```

## 2. Update the changelog

Add the new version entry to `docs\Changelogs.md`, preserving its existing
format and historical entries. Include the release date and concise bullets
for the user-visible changes. If the file only contains its title, add the
first version section below that title.

## 3. Update README documentation when needed

Review the release diff for changes to user configuration or commands. This
includes `webinfo.json`/`src/Models`, CLI parsing or help, `init`/`build`
arguments, generated-site configuration, or command behavior. When affected,
update the relevant sections in `README.md` and the matching localized content
in `README_cn.md`; keep examples consistent with the current CLI contract.
Do not change README files for releases that do not alter user-facing
configuration or commands.

## 4. Commit, publish, and return

Review the complete diff, then stage all uncommitted changes. Use a
conventional commit message(summary changes) with the repository's emoji convention, for
example:

```text
🔖 release(2.3.7): <summary changes>
```

Commit and push the source branch:

```powershell
git add <approved release paths>
git commit -m "🔖 release(2.3.7): summary changes"
git push origin $releaseStartBranch
```

If commit or source-branch push fails, stop and report the command and current
Git state. Do not improvise recovery.

Update the publication branch using fast-forward-only operations:

```powershell
git fetch origin --prune
git switch nuget
git pull --ff-only origin nuget
git merge --ff-only $releaseStartBranch
git push origin nuget
```

The push to `nuget` triggers `.github/workflows/build.yml`, which runs
`pack.ps1` and publishes the generated package to NuGet. Do not manually push
the package or create a release tag unless explicitly requested.

On success, return to the exact branch captured at the start and verify the
final state:

```powershell
git switch $releaseStartBranch
git status --short
git branch -vv
```

Report the released version, commit, pushed branches, test totals, and the
NuGet CI follow-up. If the merge or publication fails, leave the repository in
the safest inspectable state, report the exact failure, and do not claim the
release is complete.
