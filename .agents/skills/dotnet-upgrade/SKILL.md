---
name: dotnet-upgrade
description: 'Ready-to-use prompts for comprehensive .NET framework upgrade analysis and execution'
---

# .NET Upgrade Skill

This skill provides ready-to-use prompts for comprehensive .NET framework upgrade analysis and execution.

## Usage

When a user wants to upgrade a .NET project, use this skill to guide the process through:
1. Analysis of the current project state
2. Identification of upgrade requirements
3. Execution of the upgrade
4. Validation of the result

## Upgrade Process

### Phase 1: Analysis
- Identify current target framework
- Audit NuGet packages for compatibility
- Check for deprecated APIs
- Review project file structure

### Phase 2: Planning
- Determine target framework
- Map package upgrades needed
- Identify code changes required
- Plan rollback strategy

### Phase 3: Execution
- Update .csproj TargetFramework
- Upgrade NuGet packages
- Fix breaking API changes
- Update global.json if needed

### Phase 4: Validation
- Build the project
- Run tests
- Check for warnings
- Verify runtime behavior

## Common Upgrade Paths

| From | To | Key Changes |
|------|----|-------------|
| net8.0 | net10.0 | New language features, performance improvements |
| net6.0 | net8.0 | Minimal breaking changes |
| net48 | net8.0 | Major re-architecture required |