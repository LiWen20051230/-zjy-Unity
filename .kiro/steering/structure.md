# Project Structure

## Root Directory
- `My project/` - Main Unity project folder containing all project files

## Key Directories

### Assets/
Main content directory for all game assets, scripts, and resources.

- `Assets/Scenes/` - Unity scene files
  - `SampleScene.unity` - Default starter scene

- `Assets/Settings/` - Project configuration and render pipeline settings
  - URP renderer assets (PC and Mobile variants)
  - Volume profiles for post-processing
  - Global render pipeline settings

- `Assets/TutorialInfo/` - Template documentation and helper scripts
  - `Scripts/` - Tutorial-related C# scripts
  - `Scripts/Editor/` - Editor-only scripts for tutorial UI

- `Assets/InputSystem_Actions.inputactions` - Input system configuration
  - Defines Player and UI action maps
  - Configures bindings for all supported input devices

### Library/
Unity-generated cache and build artifacts (not version controlled).

### Project Files
- `Assembly-CSharp.csproj` - Runtime code project file
- `Assembly-CSharp-Editor.csproj` - Editor code project file
- `.vsconfig` - Visual Studio configuration

## Conventions

### Script Organization
- Runtime scripts: Place in `Assets/` or appropriate subdirectories
- Editor scripts: Must be in folders named `Editor/`
- Each script should match its class name (Unity requirement)

### Asset Naming
- Use PascalCase for asset files
- Scene files use `.unity` extension
- All assets require `.meta` files (Unity-generated)

### Folder Structure Best Practices
- Group related assets in subdirectories under `Assets/`
- Keep editor-only code in `Editor/` folders
- Separate settings and configuration files in dedicated folders
- Use clear, descriptive folder names

## Build Output
- Build artifacts go to `Temp/bin/` (Debug or Release)
- Final builds are exported outside the project directory
