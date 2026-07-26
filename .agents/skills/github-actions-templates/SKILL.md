---
name: github-actions-templates
description: 'Create production-ready GitHub Actions workflows for automated testing, building, and deploying applications. Use when setting up CI/CD with GitHub Actions, automating development workflows, or creating reusable workflow templates.'
---

# GitHub Actions Templates

Create production-ready GitHub Actions workflows.

## Usage

When a user needs a CI/CD workflow, this skill provides templates for common scenarios.

## Available Templates

### 1. .NET Build & Test
- Restore, build, test with matrix strategy
- Support for multiple .NET versions
- Artifact publishing

### 2. Vulnerability Scanning
- `dotnet list package --vulnerable`
- SBOM generation
- Dependency audit

### 3. Multi-Platform Build
- Build for multiple OS and architectures
- Matrix strategy configuration
- Platform-specific steps

### 4. Release Pipeline
- Semantic versioning
- GitHub Release creation
- Artifact attachment

## Best Practices

- Use `actions/checkout@v4`
- Use `actions/setup-dotnet@v4`
- Pin action versions
- Include concurrency groups
- Add job timeouts
- Use caching when appropriate