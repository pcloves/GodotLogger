# Contributing to GodotLogger

## Workflow

The `master` branch is protected. All changes must go through Pull Requests.

### External Contributors (Fork)

1. Fork this repository
2. Create a branch in your fork and commit your changes
3. Ensure zero build warnings: `dotnet build --configuration Release --no-restore`
4. Submit a Pull Request to the `master` branch
5. Wait for CI checks to pass

### Collaborators (Write Access)

1. Create a branch from `master`:
   - New feature: `feature/short-description`
   - Bug fix: `fix/short-description`
   - Release: `release/vX.Y.Z`
   - Chore: `chore/short-description`
2. Commit your changes, ensure zero build warnings: `dotnet build --configuration Release --no-restore`
3. Push to GitHub and create a PR to `master`
4. Wait for CI checks to pass
5. Use **Squash merge** to merge

---

## Build

```bash
dotnet build --configuration Release --no-restore
```

- `<GenerateDocumentationFile>true</GenerateDocumentationFile>` is enabled — all `public` members need XML doc comments
- **Zero warnings** is required

---

## Release Process (Maintainer)

### Prerequisites

- `NUGET_API_KEY` Secret configured in GitHub repository
- Branch protection enabled on `master` (Require PR + Status Checks + Include admins)

### Steps

1. **Prepare the release branch**
   ```bash
   git checkout master
   git pull
   git checkout -b release/vX.Y.Z
   ```

2. **Bump the version**
   - Edit `<Version>` in `GodotLogger.csproj`, drop the `-dev*` suffix
   - Example: `1.0.1-dev1` → `1.0.1`

3. **Create a PR to merge into master**
   - PR: `release/vX.Y.Z → master`
   - Squash merge once CI passes

4. **Tag and publish**
   ```bash
   git checkout master
   git pull
   git tag vX.Y.Z
   git push origin vX.Y.Z
   ```

5. **CI handles the rest**
   - Build and push to NuGet.org
   - Create GitHub Release with release notes

6. **Prepare next development cycle**
   - Create `post-release-vX.Y.Z` branch from `master`
   - Bump `<Version>` to `X.Y.(Z+1)-dev1`
   - Push and create a PR → master → squash merge
