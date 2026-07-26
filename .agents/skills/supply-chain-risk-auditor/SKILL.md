---
name: supply-chain-risk-auditor
description: 'Identifies dependencies at heightened risk of exploitation or takeover. Use when assessing supply chain attack surface, evaluating dependency health, or scoping security engagements.'
---

# Supply Chain Risk Auditor

Identifies dependencies at heightened risk of exploitation or takeover.

## Usage

Run this skill against a project's dependency manifest to produce a risk assessment report.

## Audit Process

1. Parse dependency manifest (package.json, .csproj, Cargo.toml, etc.)
2. For each dependency, evaluate:
   - Maintainer count and bus factor
   - Release frequency and recency
   - Security policy presence
   - Known vulnerabilities
   - Supply chain attack history
3. Generate risk ratings and recommendations

## Risk Levels

| Level | Criteria |
|-------|----------|
| Critical | Single maintainer, no recent releases, known vulns |
| High | Small maintainer team, infrequent releases |
| Medium | Adequate maintenance, minor concerns |
| Low | Well-maintained, active community |

## Output

A markdown report at `.supply-chain-risk-auditor/results.md` with:
- Executive summary
- Per-dependency risk analysis
- Recommendations
- Mitigation strategies