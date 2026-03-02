# Technology Stack

## Engine & Version
- Unity 6000.3.10f1 (Unity 6)
- C# 9.0 with .NET Standard 2.1
- Target Framework: .NET 4.7.1

## Rendering
- Universal Render Pipeline (URP)
- Separate render pipeline assets for PC and Mobile platforms
- Post-processing with volume profiles

## Input System
- Unity Input System (new input system)
- Input actions defined in `Assets/InputSystem_Actions.inputactions`
- Supports multiple control schemes: Keyboard & Mouse, Gamepad, Touch, Joystick, XR

## Build Configuration
- Primary target: Windows Standalone (64-bit)
- Platform: StandaloneWindows64
- Build profiles configured for multiple platforms

## Project Structure
- Language version: C# 9.0
- Assembly definitions: Assembly-CSharp (runtime), Assembly-CSharp-Editor (editor)
- Visual Studio 2022 integration with Unity analyzers

## Common Commands

Since this is a Unity project, most operations are performed through the Unity Editor rather than command line. However, common workflows include:

- Open project in Unity Editor (Unity Hub or direct launch)
- Build: File > Build Settings in Unity Editor
- Play mode testing: Play button in Unity Editor
- Script compilation: Automatic on save when Unity Editor is open
- Package management: Window > Package Manager in Unity Editor

## Development Notes
- Scripts are located in `Assets/` directory
- Editor scripts go in `Assets/*/Editor/` folders
- Unity automatically compiles C# scripts when modified
- Use Unity's built-in testing framework for unit tests (Window > General > Test Runner)
