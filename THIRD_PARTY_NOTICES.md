# Third-Party Software Notices

ChainOSC for Windows is distributed under the MIT License. It includes or is
built with third-party software. Copyright in third-party software remains
with its respective owners.

This notice was prepared for ChainOSC for Windows v0.7.0 from the locked
dependencies in `tauri/package-lock.json` and `tauri/src-tauri/Cargo.lock` for
the `x86_64-pc-windows-msvc` target.

## Tauri and related components

The application uses Tauri and its official plugins. These components are
available under the MIT License or the Apache License 2.0.

| Component | Version | License | Source |
| --- | ---: | --- | --- |
| Tauri | 2.11.5 | MIT OR Apache-2.0 | <https://github.com/tauri-apps/tauri> |
| Tauri API for JavaScript | 2.11.1 | Apache-2.0 OR MIT | <https://github.com/tauri-apps/tauri> |
| Tauri dialog plugin | 2.7.2 | MIT OR Apache-2.0 | <https://github.com/tauri-apps/plugins-workspace> |
| Tauri global-shortcut plugin | 2.3.2 | MIT OR Apache-2.0 | <https://github.com/tauri-apps/plugins-workspace> |
| Tauri opener plugin | 2.5.4 | MIT OR Apache-2.0 | <https://github.com/tauri-apps/plugins-workspace> |

The corresponding MIT and Apache-2.0 license texts are available in
`LICENSE` and `licenses/Apache-2.0.txt`.

## Apache-2.0 component

The following dependency is distributed under Apache License 2.0:

| Component | Version | License | Source |
| --- | ---: | --- | --- |
| tao | 0.35.3 | Apache-2.0 | <https://github.com/tauri-apps/tao> |

See `licenses/Apache-2.0.txt` for the complete license text. No separate
upstream `NOTICE` file was present in the locked Windows dependency set when
this notice was generated.

## MPL-2.0 components and source availability

The following components are distributed under Mozilla Public License 2.0.
Their source code is available from the listed repositories and from the
corresponding crate pages on <https://crates.io/>.

| Component | Version | Source code |
| --- | ---: | --- |
| cssparser | 0.36.0 | <https://github.com/servo/rust-cssparser> |
| cssparser-macros | 0.6.1 | <https://github.com/servo/rust-cssparser> |
| dtoa-short | 0.3.5 | <https://github.com/upsuper/dtoa-short> |
| option-ext | 0.2.0 | <https://github.com/soc/option-ext> |
| selectors | 0.36.1 | <https://github.com/servo/stylo> |

ChainOSC for Windows uses these components without modification. Their source
files remain governed by MPL-2.0. ChainOSC for Windows as a larger work remains
licensed under MIT. See `licenses/MPL-2.0.txt` for the complete MPL-2.0 text.

If a future release modifies any MPL-covered source file, the corresponding
modified source must also be made available under MPL-2.0.

## Other transitive dependencies

The remaining locked npm and Rust dependencies use permissive licenses,
including MIT, Apache-2.0, BSD-3-Clause, ISC, Unicode-3.0, Zlib, Unlicense,
CC0-1.0, MIT-0, 0BSD, and BSL-1.0. No GPL, LGPL, or AGPL dependency was found
in the Windows dependency graph for this release.

Exact component versions are reproducibly recorded in:

- `tauri/package-lock.json`
- `tauri/src-tauri/Cargo.lock`

When dependencies change, this notice must be reviewed and regenerated before
publishing a new binary release.

## Microsoft WebView2

ChainOSC for Windows uses the Microsoft Edge WebView2 Runtime provided or
installed on the user's Windows system. The portable application does not
bundle a fixed WebView2 Runtime. WebView2 is Microsoft software and is subject
to Microsoft's applicable license terms.

Distribution guidance:
<https://learn.microsoft.com/microsoft-edge/webview2/concepts/distribution>
