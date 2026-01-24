# cinema-practice2026
Cinema management system

## Git workflow (branches, commits, PR)
We use three branch types to keep the project stable and to avoid breaking each other’s work.

### Branches
#### `main`
- Stable branch.
- Must always be in a working state.
- We merge into `main` only when we have a stable milestone (e.g. end of week / demo ready).
- Direct pushes to `main` are not allowed.

#### `develop`
- Daily integration branch.
- All completed work is merged into `develop` via Pull Request.
- `develop` should always build and run.
- Direct pushes to `develop` are not allowed.

#### `feature/*`
- One branch per Jira issue (1 issue = 1 branch = 1 PR).
- Branch naming:
  - `feature/<JIRA-KEY>-short-name`
  - Example: `feature/SCRUM-12-admin-films`
- All commits for a task go to the corresponding `feature/*` branch.

---

### How to work on a task

1) **Update local `develop`**:
```bash
git checkout develop
git pull
```
2) **Create a new feature branch**:
```bash
git checkout -b feature/SCRUM-XX-short-name
```
3) **Work and commit changes to this feature branch**.

4) **Push the branch**:
```bash
git push -u origin feature/SCRUM-XX-short-name
```
5) **Open a Pull Request**:

**Source**: feature/SCRUM-XX-short-name <br>
**Target**: develop

- Fill in the PR template (what was done + how to test).

- After review and approval, merge the PR into develop.

- Delete the feature branch after merge.

### Commit rules
- Commit only to your feature/* branch.

- Keep commits small and meaningful.

- Use clear commit messages, for example:
```text
SCRUM-12: add films list page

SCRUM-12: implement film create/edit

SCRUM-12: fix validation for duration
```

- Recommended commit message format:
```text
<JIRA-KEY>: <short description>
```

### Pull Request rules
- No direct pushes to develop or main.

- Every change goes through a Pull Request.

- At least **one** approval is required before merging.

- If there are review comments, resolve them before merge.

- If PR introduces DB schema changes, it must include EF Core migrations.

- Code should meet [C# formatting conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).

### Database / migrations rules
- Database schema changes are done only via EF Core migrations (no manual edits in the DB).

- If you changed entities/DbContext, you must add a migration in the same PR.

- After pulling new changes from develop, update your local DB:
```bash
dotnet ef database update
```
- If you see migration conflicts, contact the DB owner before merging.
Database (MSQL Server + migrations)
