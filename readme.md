# RetriX

RetriX is an emulator front end for UWP, on all the hardware platforms it supports: it serves the same purpose as RetroArch but is built from the ground up to integrate as well as possible with Windows.
It can run on desktop, tablet, phone and Xbox One (in dev mode). Holographic, Mixed Reality and Continuum on phone should work too but haven't beed tested.

## Systems supported

ReriX supports any [LibRetriX](https://github.com/Aftnet/LibRetriX) conformant cores.

## Demo

[![Youtube link](https://img.youtube.com/vi/212kBK0IB1w/0.jpg)](https://youtu.be/212kBK0IB1w)

Click image to play video

Original demo video [here](https://youtu.be/1mzS54HhcEM)

## How to use

Check the [wiki](https://github.com/Aftnet/RetriX/wiki)

## Want to support RetriX?

Consider [donating via Patreon](https://www.patreon.com/aftnet).

## Installing

1. Check the [releases](https://github.com/Aftnet/RetriX/releases) section for the latest binary build
2. Download and double click on the .appxbundle file, follow the instructions

## Design goals

- Full compliance with UWP sandboxing
- Having cores render to SwapChain panels, allowing [XAML and DirectX interop](https://docs.microsoft.com/en-us/windows/uwp/gaming/directx-and-xaml-interop) and front ends to use native UWP controls for the UI
- Support Windows 10 on all platforms (Desktop, Mobile, Xbox, VR) and architectures (x86, x64, ARM)

## Project aim

- Creating a UWP emulation front end for a better experience (proper DPI scaling, native look and feel, fullscreen as borderless window, integration with Windows's modern audio pipeline)
- Increasing safety by having emulation code run sandboxed
- Create a meaningful use case for UWP sideloading on Windows, since Microsoft has decided to ban emulators from the store