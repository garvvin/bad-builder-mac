---
name: code-review
description: 'Review the changes since a fixed point (commit, branch, tag, or merge-base) along two axes — Standards (does the code follow this repo''s documented coding standards?) and Spec (does the code match what the originating issue/PRD asked for?). Runs both reviews in parallel sub-agents and reports them side by side. Use when the user wants to review a branch, a PR, work-in-progress changes, or asks to "review since X".'
---

# Code Review

This skill performs a comprehensive code review on changes since a specified fixed point.

## Usage

Trigger this skill with a base point (commit hash, branch name, tag, or merge-base syntax).

The skill will:
1. Determine the diff range
2. Review changes against two axes:
   - **Standards Review**: Does the code follow this repository's documented coding standards?
   - **Spec Review**: Does the code match what the originating issue/PRD asked for?
3. Present both reviews side-by-side

## Input Format

```
review since: <base-point>
reference: <optional-issue-or-prd-reference>
```

Where `<base-point>` can be:
- A commit SHA (e.g., `abc1234`)
- A branch name (e.g., `main`, `development`)
- A tag (e.g., `v1.0.0`)
- Merge-base syntax (e.g., `main...HEAD`)

## Review Process

### 1. Standards Review
- Review against any `.editorconfig`, `.prettierrc`, `eslint.config.*`, or similar files
- Review against any `CONVENTIONS.md`, `STYLE_GUIDE.md`, or `CODING_STANDARDS.md` files
- Check for patterns established in the existing codebase
- Flag inconsistencies with project conventions

### 2. Spec Review
- If an issue/PR reference is provided, fetch it and check completeness
- Verify all requirements are addressed
- Check for regressions or unintended side effects
- Flag missing edge cases or incomplete implementations

## Output Format

```
## Standards Review
✅ / ⚠️ / ❌ - [finding]

## Spec Review
✅ / ⚠️ / ❌ - [finding]

## Summary
- Standards: X issues found
- Spec: Y issues found
```

## Extensions

When a `.github/copilot-instructions.md` or `.cursorrules` file exists with review guidelines, incorporate those rules into the standards review.