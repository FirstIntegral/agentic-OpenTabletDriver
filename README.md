> **[agentic-OpenTabletDriver](https://github.com/FirstIntegral/agentic-OpenTabletDriver)** — personal fork. See **[FORK.md](FORK.md)**.

# OpenTabletDriver

English | [한국어](docs/README_KO.md) | [Español](docs/README_ES.md) | [Русский](docs/README_RU.md) | [简体中文](docs/README_CN.md) | [Français](docs/README_FR.md) | [Deutsch](docs/README_DE.md)

OpenTabletDriver is an open source, cross platform, user mode tablet driver. The goal of OpenTabletDriver is to be as cross platform as possible with the highest compatibility in an easily configurable graphical user interface.

<p align="middle">
  <img src="https://i.imgur.com/XDYf62e.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/jBW8NpU.png" width="410" align="middle"/>
  <img src="https://i.imgur.com/ZLCy6wz.png" width="410" align="middle"/>
</p>

# Supported Tablets

See [TABLETS.md](TABLETS.md) in this repository.

# Installation

Build from this tree (below). This fork is not the packaged upstream releases.

# Running OpenTabletDriver binaries

OpenTabletDriver functions as two separate processes that interact with each other seamlessly. The active program that does all of the tablet data handling is `OpenTabletDriver.Daemon`, while the GUI frontend is `OpenTabletDriver.UX.*`, where `*` depends on your platform<sup>1</sup>. The daemon must be started in order for anything to work, however the GUI is unnecessary. If you have existing settings, they should apply when the daemon starts.

> <sup>1</sup>Windows uses `Wpf`, Linux uses `Gtk`, and MacOS uses `MacOS` respectively. This for the most part can be ignored if you don't build it from source as only the correct version will be provided.

## Building OpenTabletDriver from source

The requirements to build OpenTabletDriver are consistent across all platforms. Running OpenTabletDriver on each platform requires different dependencies.

### All platforms

- .NET 10 SDK (can be obtained from [here](https://dotnet.microsoft.com/download/dotnet/10.0) - You want the SDK for your platform, Linux users should install via package manager where possible)

#### Windows

Run `build.sh windows` to produce binary builds to 'bin' folder. These builds will run in portable mode by default.

If you don't have WSL or some other way to access BASH with a working dotnet install, the deprecated Windows build script still exists in `build.ps1`.

#### Linux

Required packages (some packages may be pre-installed for your distribution):

- libx11
- libxrandr
- libevdev2
- GTK+3

Run `./eng/bash/package.sh`. If a "package" build is desired,
there are official support for the following packaging formats:

| Package Format | Command |
| --- | --- |
| Generic binary tarball (`.tar.gz`) | `./eng/bash/package.sh --package BinaryTarBall` |
| [Simple binary package](./eng/bash/Simple/README-SimplePackage.md) (`.tar.gz`) | `./eng/bash/package.sh --package Simple` |
| Debian package (`.deb`) | `./eng/bash/package.sh --package Debian` |
| Red Hat package (`.rpm`) | `./eng/bash/package.sh --package RedHat` |
| Generic package (for package maintainers) | `./eng/bash/package.sh --package Generic` |

The generic binary tarball is designed to be extracted from the root directory.

The simple package should only be used for testing new features on existing
installs, as it does not install necessary system files.

You can also run `./build.sh linux` to generate files into `bin/`, but this does not include system files.

#### MacOS

A newer version of Bash and Coreutils is required to build OpenTabletDriver. You can install them using Homebrew.
Run `PATH="$(brew --prefix coreutils)/libexec/gnubin:$PATH" $(brew --prefix)/bin/bash ./eng/bash/package.sh -r osx-x64`.

| Package Format | Command |
| --- | --- |
| Unsigned x64 Package | `./eng/bash/package.sh --runtime osx-x64 --package macos` |
| Signed x64 Package | `./eng/bash/package.sh --signed true --runtime osx-x64 --package macos` |
| Signed arm64 Package | `./eng/bash/package.sh --signed true --runtime osx-arm64 --package macos` |

Packaging signed MacOS builds on Linux or Windows requires `rcodesign`.

As of [MacOS 11](https://developer.apple.com/documentation/macos-release-notes/macos-big-sur-11_0_1-universal-apps-release-notes/#Code-Signing), you **must** sign arm64 packages.

# Features

- Fully platform-native GUI
  - Windows: `Windows Presentation Foundation`
  - Linux: `GTK+3`
  - MacOS: `MonoMac`
- Multi-tablet support
  - Handles multiple tablets from a large selection of models and
    manufacturers, each with its own plugin pipeline and settings
- Validated tablet specifications, ensuring the smoothest transition if
  switching tablets
- Fully fledged console tool
  - Quickly acquire, change, load, or save settings
  - Scripting support (json output)
- Absolute cursor positioning
  - Screen area and tablet area
  - Center-anchored offsets
  - Precise area rotation
- Relative cursor positioning
  - px/mm horizontal and vertical sensitivity
- Advanced tablet feature compatibility
  - Pressure output
    - Windows: Needs Windows Ink OpenTabletDriver plugin for Windows Ink output
      mode, and VMulti system driver
    - Linux: Natively supported with "Linux Artist Mode" output mode
    - MacOS: Natively supported in all output modes
  - Pen tilt
  - Tablet pad wheels/dials
  - Auxiliary/Express Keys
- Pen bindings
  - Tip by pressure bindings
  - Express ("Aux") key bindings
  - Pen button bindings
  - Mouse button bindings
  - Wheel bindings
  - Keyboard bindings
  - Preset bindings
  - External plugin bindings
- Saving and loading settings
  - Persistent settings
  - Presets for quick-access to prior saved settings
- Plugins
  - Plugin Manager
  - Filters, including asynchronous filters (interpolators)
  - Output modes
- Device debugging tools
  - Tablet data analyzer ("Tablet Debugger")
  - USB Device String readings
- Automatic version update notification
  - Can be disabled with `--skipupdate` command line flag
- Vendor driver area conversions
  - Supports transforming your Wacom / XP-Pen / Huion / Gaomon / VEIKK area
- Standalone daemon, for low-spec or headless systems.


