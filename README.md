# BadBuilder

[![CI](https://github.com/garvvin/bad-builder-mac/actions/workflows/ci.yml/badge.svg)](https://github.com/garvvin/bad-builder-mac/actions/workflows/ci.yml)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
[![License](https://img.shields.io/badge/license-BSD--3--Clause-blue)](LICENSE)

> macOS-native tool for building an Xbox 360 BadUpdate exploit USB drive — with cross-platform XEX patching and homebrew support.

BadBuilder automates formatting a USB drive, downloading the exploit payload and homebrew files, extracting archives, and laying out the directory structure required by the [BadUpdate](https://github.com/grimdoomer/Xbox360BadUpdate) hypervisor exploit — all through an interactive terminal UI.

This is a macOS port of [Pdawg-bytes/BadBuilder](https://github.com/Pdawg-bytes/BadBuilder), rewritten in .NET 10 with native macOS disk detection and formatting (no Windows dependencies).

⭐ If you find this useful, star it on GitHub!

[Requirements](#requirements) • [Quick Start](#quick-start) • [How to Use](#how-to-use) • [Technical Overview](#technical-overview) • [CI/CD & Security](#cicd--security) • [Credits](#credits)

## Features

### USB Formatting
- FAT32 + MBR formatting via macOS `diskutil` — no `sudo` required
- Only external drives are detected and listed; system drives are excluded
- Bypassable format step if drive is already prepared

### File Management
- Fetches the latest release assets from GitHub (BadUpdate, FreeMyXe, XeUnshackle) via Octokit
- Downloads XeXmenu, Rock Band Blitz game data, and Simple 360 NAND Flasher from fixed mirrors
- Detects previously downloaded files and reuses them — skip re-downloads or supply local paths
- Parallel downloads and extractions with progress bars, ETA, and transfer speed

### Exploit & Homebrew
- Copies files in priority order via a priority queue — exploit payload always lands last
- Patches Xbox 360 XEX executables in pure C# (removes region locks, media restrictions, and retail signing; repairs SHA1 header hash)
- Picks between FreeMyXe or XeUnshackle as the default app launched by BadUpdate
- Adds homebrew apps with automatic `.xex` entry-point detection and in-place patching

> [!IMPORTANT]
> BadBuilder expects archives to match the expected folder structure. If your provided archive has a different layout, extraction or copying may fail.

## Requirements

- **macOS** 11+ (Apple Silicon or Intel)
- **.NET 10.0 SDK** (for building from source)
- A USB drive (any size — formatting is automatic)

## Quick Start

### Download (pre-built)

Download the latest binary from [Releases](https://github.com/garvvin/bad-builder-mac/releases).

### Build from source

```bash
git clone https://github.com/garvvin/bad-builder-mac.git
cd bad-builder-mac
dotnet publish --configuration Release -r osx-arm64 -o publish
```

> [!TIP]
> On an Intel Mac, use `-r osx-x64` instead of `-r osx-arm64`.

## How to Use

1. **Launch the executable.** A Terminal window opens with the welcome banner.
2. **Select a disk.** BadBuilder lists detected external USB drives.
3. **Confirm formatting.** All data on the selected drive will be erased.

   > [!CAUTION]
   > Make sure you have selected the right drive before confirming. The author is not responsible for any data loss.

4. **Download files.** BadBuilder fetches the required exploit files from GitHub or lets you point to local copies.
5. **Extract files.** Archives are extracted automatically with progress feedback.
6. **Select a default program.** Choose between [FreeMyXe](https://github.com/FreeMyXe/FreeMyXe) or [XeUnshackle](https://github.com/Byrom90/XeUnshackle).
7. **Copy files.** The priority queue writes everything to the correct locations on the USB drive.
8. **Add homebrew (optional):**
   - Provide the root folder of your homebrew application
   - BadBuilder detects the `.xex` entry point automatically
   - Files are mirrored to the USB drive and the XEX is patched in place

### Example Homebrew Folder

To add Aurora, select the **root folder**:

```
Aurora 0.7b.2 - Release Package/
├── Data/
├── Media/
├── Plugins/
├── Skins/
├── User/
├── Aurora.xex
├── live.json
└── nxeart
```

BadBuilder detects `Aurora.xex` as the entry point and patches it.

> [!IMPORTANT]
> Homebrew apps without an entry point in the root folder require manually entering the path.

## Technical Overview

### Tech Stack

| Component | Library | Version |
|---|---|---|
| Runtime | .NET | 10.0 |
| Terminal UI | Spectre.Console | 0.57.2 |
| GitHub API | Octokit | 14.0.0 |
| Archive extraction | SharpCompress | 1.0.0 |

### Architecture

- **ConsoleExperience partial classes** — `DiskExperience`, `DownloadExperience`, `ExtractExperience`, and `HomebrewExperience` split the wizard flow across four files, all part of `partial class Program`
- **Platform abstraction** — `IPlatformDiskService` with a macOS implementation that uses `statfs` P/Invoke (`libSystem.dylib`) to check the `MNT_REMOVABLE` flag, and parses `diskutil list -plist external` XML for device identifiers
- **Priority queue** — `ActionQueue` (`SortedDictionary<int, Queue<Func<Task>>>`) enqueues file-copy operations by priority; exploit files run at priority 10 (last), ensuring the USB layout matches what BadUpdate expects
- **Static helpers** — `DownloadHelper` (Octokit + HTTP), `ArchiveHelper` (SharpCompress), `PatchHelper` (XEX patching), `FileSystemHelper` (directory mirroring), `DiskHelper` (formatting facade)

> [!NOTE]
> A `WindowsDiskService` fallback exists for non-macOS platforms, but FAT32 validation is broken on Linux where the filesystem is reported as `vfat`.

### XEX Patching

BadBuilder replaces the old Windows-only `XexTool.exe` dependency with **cross-platform C# byte-level patching**. The `PatchHelper`:

1. Validates the XEX2 magic number (`0x58455832`)
2. Sets image flags to `0xFFFFFFFF` at the security-info offset (disables region and media restrictions)
3. Clears the retail-signing bit in `imageFlags`
4. Zeroes out the key vault area
5. Repairs the SHA1 header hash using `IncrementalHash` so the Xbox 360 console accepts the modified executable

## CI/CD & Security

GitHub Actions builds macOS arm64 and x64 artifacts on every push and PR to `main`:

- **Vulnerability scanning** — `dotnet list package --vulnerable` fails the build if any package has a known vulnerability
- **SBOM generation** — `dotnet list package --include-transitive` produces a software bill of materials
- **Dependabot** — keeps NuGet packages up to date weekly
- **Supply chain audit** — third-party risk assessment on all dependencies

## Credits

BadBuilder was originally created for Windows by **Pdawg-bytes** as the [BadBuilder](https://github.com/Pdawg-bytes/BadBuilder) project.

This fork (`bad-builder-mac`) extends it with a macOS port and .NET 10 rewrite by:
- **garvvin** — macOS native disk detection (statfs P/Invoke), diskutil formatting, cross-platform XEX patching, CI/CD pipeline, supply chain audit

Exploit and homebrew tooling:
- **Grimdoomer** — [BadUpdate](https://github.com/grimdoomer/Xbox360BadUpdate)
- **InvoxiPlayGames** — [FreeMyXe](https://github.com/FreeMyXe/FreeMyXe)
- **Byrom90** — [XeUnshackle](https://github.com/Byrom90/XeUnshackle)
- **Swizzy** — [Simple 360 NAND Flasher](https://github.com/Swizzy/XDK_Projects)
- **Team XeDEV** — XeXMenu