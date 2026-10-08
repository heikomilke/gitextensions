# Git Extensions on Linux (Mono port)

This branch (`linux-mono`) carries a Linux-only port of **Git Extensions 3.5.4**, the last
release built on .NET Framework 4.6.1. Later releases (4.x and up) target modern .NET with
Windows-only Windows Forms and cannot run on Mono, so 3.5.4 is the newest version that can
run natively on Linux.

The port deliberately drops Windows support: Win32-only calls are removed or replaced, not
guarded. Do not expect this branch to build or run on Windows.

Upstream project: https://github.com/gitextensions/gitextensions (GPLv3, see `LICENSE.md`).
This fork is independent and does not send changes upstream.

## What you need

* **Docker** (the build runs in a container, nothing .NET-related is needed on the host).
* **Mono 6.x** on the host to run the result, including `libgdiplus`. On Ubuntu 26.04:
  `sudo apt install mono-runtime` (pulls `mono-libraries` and `libgdiplus`).
* `git` on the host.

Note: Ubuntu's `mono-libraries` package ships without the .NET facade assemblies and
`netstandard.dll`. `linux/extract-facades.sh` copies them from the official `mono:6.12`
image into `artifacts/mono-facades`, and `linux/run.sh` puts that directory on `MONO_PATH`.
If your Mono has `/usr/lib/mono/4.5/Facades`, this step is harmless.

## Build

```bash
git clone --branch linux-mono --recurse-submodules git@github.com:heikomilke/gitextensions.git
cd gitextensions
linux/build.sh
```

`linux/build.sh`:

1. builds the Docker image `gitext-mono-build` from `linux/Dockerfile`
   (official `mono:6.12` image, Debian Buster from the archive mirrors, plus `git`);
2. runs `msbuild /restore` for the application and all plugins that can build on Linux
   (the Team Foundation Server plugins and the WiX installer are skipped);
3. extracts the Mono facade assemblies (see above).

Output lands in `artifacts/bin/GitExtensions/Release/net461/`. The NuGet cache is kept in
`.nuget-cache/` inside the checkout so repeated builds are fast.

Warnings are not treated as errors and StyleCop analyzers are disabled for this build;
both are build-time options only.

## Run

```bash
linux/run.sh                # opens the dashboard
linux/run.sh browse /path/to/repo
```

`linux/run.sh` keeps settings in `~/.config/gitextensions-linux` (override with
`GITEXT_PROFILE=/some/dir`) so an existing Git Extensions 2.x installation that uses
`~/.config/GitExtensions` is left untouched. `HOME` is not changed, so git still finds your
normal `~/.gitconfig`.

To get a launcher in your desktop menu, create `~/.local/share/applications/gitextensions-linux.desktop`:

```ini
[Desktop Entry]
Type=Application
Name=Git Extensions (Linux)
Exec=/path/to/gitextensions/linux/run.sh
Icon=git
Categories=Development;
StartupWMClass=GitExtensions
```

## What was changed for Linux

* Resource paths with wrong case (`Images.resx`) and a plugin folder that existed twice
  with different casing (`GitHub3` / `Github3`).
* WPF assembly references removed; two call sites that pulled WPF in through overload
  resolution now use reflection.
* Direct Win32 calls (`user32`, `uxtheme`, `gdi32`, `wininet`) replaced by no-ops or
  managed equivalents: DPI awareness, theme fonts, list view group messages, rich text box
  update suspension, custom text box borders, jump lists, connectivity checks.
* Reflection into Windows Forms internals that Mono does not have (`ToolStrip.Grip`,
  `Control.BeginUpdateInternal`, `ListView.GetItemRectOrEmpty`, ...) tolerates `null`.
* All palette-based PNG icons converted to 8-bit RGBA. libgdiplus loads palette PNGs as
  indexed bitmaps, which cannot be drawn on or locked as 32-bit ARGB.
* Fixed a shared `Font` instance (handed out by the settings cache) being disposed by the
  branch tree, which invalidated the font of every control.
* Transparent `BackColor` on tool strips removed (Mono rejects it on those controls).
* The crash reporter no longer uses the Windows task dialog; errors go to stderr and to the
  built-in bug report form.
* Mono's Windows Forms imports the desktop (GTK) colour scheme into `SystemColors`, but only
  partially, so a dark desktop theme produced a mix of dark and white surfaces. At startup the
  application now resets Mono's colour table to the light Windows palette
  (`GitExtUtils/GitUI/MonoSystemColors.cs`); the UI is always light regardless of the
  desktop theme.

## Known limitations

* No embedded console (ConEmu is Windows-only); use the "Open terminal" style actions that
  start an external shell instead.
* The Team Foundation Server build server plugins are not built.
* Themes that rely on Win32 theming hooks (`dark`, `darksilver`) are not supported; the
  application uses the default light scheme.
* Mono's Windows Forms rendering differs from Windows in places (list view groups, DPI
  scaling, some custom-drawn controls).
