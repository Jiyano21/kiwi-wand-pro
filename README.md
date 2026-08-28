<div align="center">

# Kiwi Wand Pro

**A modified build of [WandEnhancer](https://github.com/k1tbyte/Wand-Enhancer) with additional features**

</div>

> **Disclaimer:** This project is a fork/modification of [WandEnhancer](https://github.com/k1tbyte/Wand-Enhancer) by [k1tbyte](https://github.com/k1tbyte). All original code remains under the [Apache License 2.0](LICENSE.md). This project is not affiliated with or endorsed by the original author.

## About

Kiwi Wand Pro is a community-modified build of WandEnhancer that adds licensing and UI customizations on top of the original feature set. It maintains full compatibility with Wand (WeMod) and includes all upstream patches and fixes.

### What's Different from WandEnhancer?

| Feature | WandEnhancer | Kiwi Wand Pro |
|---------|-------------|---------------|
| Pro activation patches | ✅ | ✅ |
| Remote Web Panel | ✅ | ✅ |
| Custom script injection | ✅ | ✅ |
| DevTools on F12 | ✅ | ✅ |
| Disable updates | ✅ | ✅ |
| License key system | ❌ | ✅ |
| Single-instance enforcement | ❌ | ✅ |
| Custom UI branding | ❌ | ✅ |
| Launch button | ❌ | ✅ |

## Credits

- **Original project:** [k1tbyte/Wand-Enhancer](https://github.com/k1tbyte/Wand-Enhancer) — the base of this project
- **Original author:** [k1tbyte](https://github.com/k1tbyte) — all credit for the core patching engine, Remote Web Panel, and ASAR manipulation goes to them
- **Modifications by:** [charlotte-zee](https://github.com/charlotte-zee)

## What Does It Do?

Kiwi Wand Pro modifies local Wand (WeMod) client configurations to:

- ✅ Activate Pro features locally
- ✅ Provide a Remote Web Panel for phone control
- ✅ Allow custom JavaScript script injection
- ✅ Open DevTools with F12
- ✅ Disable automatic updates
- ✅ Advanced layout and theme customization

## How to Build

### Requirements

- Visual Studio 2022+ with .NET Framework 4.8 targeting pack
- CMake
- Node.js and pnpm
- NSIS (for installer)

### Build Steps

1. Clone this repository
2. Run `build.cmd` or open `Wand-Enhancer.sln` in Visual Studio
3. Build in Release configuration
4. Use `installer.nsi` with NSIS to create the setup executable

## License

This project is licensed under the [Apache License 2.0](LICENSE.md) — same as the original WandEnhancer.

```
Copyright 2024 k1tbyte (original WandEnhancer)
Copyright 2026 charlotte-zee (Kiwi Wand Pro modifications)
```

You may use, modify, and distribute this software under the terms of the Apache License 2.0. See [LICENSE.md](LICENSE.md) for full details.

## Acknowledgments

This project would not exist without the incredible work of [k1tbyte](https://github.com/k1tbyte) and the [WandEnhancer](https://github.com/k1tbyte/Wand-Enhancer) project. All respect and credit goes to the original author for creating and maintaining the base tool.

---

> **Note:** This is a personal/community build. It is not affiliated with, endorsed by, or connected to the original WandEnhancer project or its author.
