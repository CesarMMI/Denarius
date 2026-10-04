---
description: "Apply the remediation from a bug assessment and record what was changed"
---

# Fix Bug

Apply the remediation that was proposed by `__SPECKIT_COMMAND_BUG_ASSESS__` and record the changes in a fix report at `<project>/bugs/<slug>/fix.md`. This command is **only** valid after an assessment exists for the given slug.

## User Input

```text
$ARGUMENTS
```

The user input should identify the bug to fix. Accept any of:

- `slug=<bug-slug>` or `--slug <bug-slug>` or just a bare slug-like token, optionally with `project=back` or `project=front`.
- A path that contains the project and the slug (e.g. `back/bugs/login-timeout/`).
- **Nothing** — fall back to context (see below).

## Slug Resolution

Bug reports live in `back/bugs/<slug>/` and `front/bugs/<slug>/`, in the project that holds the root cause. Resolve `BUG_PROJECT` and `BUG_SLUG` in this order, stopping at the first match:

1. **Explicit user input** — a slug passed in `$ARGUMENTS` (any of the forms above). Without a project, look for the slug in both `back/bugs/` and `front/bugs/`; if it exists in both, disambiguate (step 4).
2. **Conversation context** — if the current session has just run `__SPECKIT_COMMAND_BUG_ASSESS__`, the slug and project it reported are the working ones. Reuse them without re-prompting. Confirm them by checking that `<project>/bugs/<slug>/assessment.md` exists; if it does not, fall through.
3. **Single candidate on disk** — list `back/bugs/*/assessment.md` and `front/bugs/*/assessment.md`. If exactly one matching `assessment.md` is found, use the project and slug from its path.
4. **Disambiguate**:
   - **Interactive mode**: ask the user which bug to fix and list the candidates.
   - **Automated mode**: stop with an error listing the candidates. Do not guess.

Once resolved, set `BUG_PROJECT`, `BUG_SLUG` and `BUG_DIR = <BUG_PROJECT>/bugs/<BUG_SLUG>`, and briefly state in your reply which resolution path was used (explicit / from context / single candidate / asked).

## Prerequisites

- `BUG_DIR/assessment.md` MUST exist. If it does not, stop and instruct the user to run `__SPECKIT_COMMAND_BUG_ASSESS__` first.
- If `BUG_DIR/fix.md` already exists, ask the user whether to overwrite it before continuing (interactive mode) or refuse (automated mode).
- Read `BUG_DIR/assessment.md` in full. Treat its **Proposed Remediation**, **Files likely to change**, **Tests to add or update**, and **Risks & Considerations** sections as the contract for this command.

## Execution

1. **Confirm the plan**
   - Restate, in 3–6 bullets, what you are about to change and where, based on the assessment.
   - If the assessment's verdict is `invalid`, stop — there is nothing to fix. Tell the user and exit.
   - If the verdict is `likely valid, needs reproduction` and there are unresolved `[NEEDS CLARIFICATION]` items, flag them and ask the user whether to proceed in interactive mode, or stop in automated mode.

2. **Apply the remediation**
   - Make the code changes described by the preferred remediation. Stay within the files listed by the assessment unless newly discovered evidence requires expanding scope (in which case, log the expansion explicitly in the report).
   - Add or update the tests called out in the assessment so the bug cannot regress silently.
   - Keep the change minimal — do not refactor unrelated code, do not introduce dependencies that the assessment did not call for.
   - If you discover the assessment was wrong (the proposed fix does not work, the root cause is elsewhere), STOP modifying code, document the new finding in the fix report under **Deviations from Assessment**, and recommend re-running `__SPECKIT_COMMAND_BUG_ASSESS__`.

3. **Run local checks**
   - If the project has obvious test commands (e.g., `pytest`, `npm test`, `cargo test`), run the tests that exercise the changed paths. Capture pass/fail and key output.
   - Do not run destructive or network-dependent suites without the user's consent.

4. **Write the fix report**

   Write to `BUG_DIR/fix.md` using this structure:

   ```markdown
   # Bug Fix: <short title>

   - **Slug**: <BUG_SLUG>
   - **Fixed**: <ISO 8601 date>
   - **Assessment**: ./assessment.md
   - **Status**: applied | partial | not-applied

   ## Summary

   <One or two sentences describing what was changed and why.>

   ## Changes

   | File | Change | Notes |
   |------|--------|-------|
   | `path/to/file.py` | <added / modified / removed> | <short note> |
   | `path/to/test_file.py` | added test | <short note> |

   ## Diff Highlights (optional)

   <Short, illustrative snippets of the most important hunks — not a full diff dump.>

   ## Tests Added or Updated

   - `path/to/test_file.py::test_name` — <what it pins down>

   ## Local Verification

   - Commands run: `<command>` → <result, brief>
   - Manual checks: <what was verified by hand, if anything>

   ## Deviations from Assessment

   <Empty if none. Otherwise, list any places where the actual fix departed from the proposed remediation and why.>

   ## Follow-ups

   - <suggested cleanup, monitoring, doc update, etc.>
   ```

5. **Report back** with:
   - The slug and `BUG_DIR/fix.md` path.
   - The status (`applied`, `partial`, `not-applied`).
   - The next suggested step: `__SPECKIT_COMMAND_BUG_TEST__ project=<BUG_PROJECT> slug=<BUG_SLUG>`.

## Guardrails

- Never modify files outside the project workspace.
- Never edit `assessment.md` — it is the contract you are working against. Record disagreements in `fix.md` under **Deviations from Assessment**.
- Never delete files unless the assessment explicitly required it.
- Never overwrite an existing `fix.md` without confirmation.
