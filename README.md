# Re-format on copy

A small utility that monitors the clipboard for content from designated processes and re-formats copied text to remove unnecessary line breaks.

I use this to copy text from PDF into Markdown. The copied text usually contains lots of line breaks that 
exist for the physical page layout of the PDF, which are irrelevant for a digital medium.

You probably can't use the program as-is (there are hard-coded heuristics), but if you are comfortable tweaking it, it may be a good starting point.

## Presentation
A quick presentation on the journey to make the line splitting _fast_: [slides](https://chklauser.github.io/ReformatOnCopy/) (done with [sli.dev](https://sli.dev/))

## Prerequisites

 - .NET 6.0.202 SDK
 - Rust 1.60.0
 - Windows (Clipboard interception only implemented for Windows)

## Build

```bash
# Build Rust library
cd libreformat
cargo build --release
cd ..

# Build .NET CLI
dotnet build
```
