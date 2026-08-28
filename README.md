<div align="center">

# WandEnhancer Fork

**A modified build of [WandEnhancer](https://github.com/k1tbyte/Wand-Enhancer) with additional patches and fixes**

</div>

> **Disclaimer:** This project is a fork/modification of [WandEnhancer](https://github.com/k1tbyte/Wand-Enhancer) by [k1tbyte](https://github.com/k1tbyte). All original code remains under the [Apache License 2.0](LICENSE.md). This project is not affiliated with or endorsed by the original author.

## About

This is a community-modified build of WandEnhancer that includes the latest upstream patches (v1.0.9.4) along with some additional UI tweaks. It maintains full compatibility with Wand (WeMod) and includes all upstream patches and fixes.

### What's Included

- All features from WandEnhancer v1.0.9.4
- MultiFile patch support for patches that appear in multiple JS bundle chunks
- New `setUserAccountReducer` patch to prevent Pro subscription wipe
- Fixed process kill loop bug
- Fixed backup restore to handle `app.asar.unpacked`
- Custom UI theme and branding

## Credits

- **Original project:** [k1tbyte/Wand-Enhancer](https://github.com/k1tbyte/Wand-Enhancer) — the base of this project
- **Original author:** [k1tbyte](https://github.com/k1tbyte) — all credit for the core patching engine, Remote Web Panel, and ASAR manipulation goes to them
- **Modifications by:** [charlotte-zee](https://github.com/charlotte-zee)

## What Does It Do?

This tool modifies local Wand (WeMod) client configurations to:

- Activate Pro features locally
- Provide a Remote Web Panel for phone control
- Allow custom JavaScript script injection
- Open DevTools with F12
- Disable automatic updates
- Advanced layout and theme customization

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
Copyright 2026 charlotte-zee (modifications)
```

You may use, modify, and distribute this software under the terms of the Apache License 2.0. See [LICENSE.md](LICENSE.md) for full details.

## Acknowledgments

This project would not exist without the incredible work of [k1tbyte](https://github.com/k1tbyte) and the [WandEnhancer](https://github.com/k1tbyte/Wand-Enhancer) project. All respect and credit goes to the original author for creating and maintaining the base tool.

---

> **Note:** This is a personal/community build. It is not affiliated with, endorsed by, or connected to the original WandEnhancer project or its author.
