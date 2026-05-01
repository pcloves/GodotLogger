# Agents Instructions

## Bilingual READMEs
- `README.md` (English) and `README.zh-CN.md` (Chinese) must stay in sync
- When adding features/docs, update English first, then mirror changes in Chinese — same section order, same code samples, same formatting

## Build & Warnings
- Build command: `dotnet build --configuration Release --no-restore`
- `<GenerateDocumentationFile>true</GenerateDocumentationFile>` is enabled — all `public` members need XML doc comments or the build emits warnings (nullable annotations also contribute)
- Zero warnings required before committing

## NuGet Publishing
- CI pushes to NuGet only on tags matching `v*` (see `.github/workflows/build.yml`)
- Update `<Version>` in `GodotLogger.csproj` when cutting a release

## SDK Pin
- `global.json` requires .NET SDK `8.0.x` (no prerelease); roll-forward allowed within `8.x`

## No Tests
- The repo has no test project — do not look for or run test commands

## PR Workflow
- `master` branch is protected — all changes must go through PRs
- Branch naming: `feature/*`, `fix/*`, `release/*`, `chore/*`
- Use **squash merge** strategy
- See `CONTRIBUTING.md` for full details

## Release Process
- Create `release/vX.Y.Z` branch → bump version (drop `-dev*` suffix) → PR → merge → tag `vX.Y.Z` → push
- After release, bump to `X.Y.(Z+1)-dev1` via separate PR
- CI publishes NuGet and creates GitHub Release on tag push
