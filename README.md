# Unity-CoreToolkit

![Unity-CoreToolkit](https://github.com/user-attachments/assets/e77e0526-7c01-420d-b820-a38b4692a0f7)

A collection of powerful editor and runtime tools to supercharge your Unity development workflow!

## Overview

**Unity-CoreToolkit** is a comprehensive Unity Package Manager (UPM) package designed to enhance your Unity projects with a wide array of editor and runtime utilities. It includes:

### Editor Extensions:
- **HotKeys:** Quick access to your custom tools via keyboard shortcuts.
- **Custom Property Drawers:** Simplify and enhance the Unity Inspector.
- **Helper Functions:** Streamline your workflow with useful utilities.
- **Design Hierarchies:** Improve project organization in the Unity hierarchy.
- **Script Icons:** Easily assign and manage custom icons for your scripts.
- **New Project Setup:** Automate initial project configuration.

### Runtime Utilities:
- **Custom Attributes & Helper Functions:** Simplify and standardize your code.
- **Advanced Debug Logs:** Improved logging for better troubleshooting.
- **Runtime Scriptables:** Manage and persist game data effectively using ScriptableObjects.

These scripts are curated from contributions by colleagues, friends, and connections, with special inspiration from the **Git Amend** YouTube tutorials.

## Features

### Editor Tools
- **Editor Extensions & HotKeys:** Access custom tools through convenient menus and keyboard shortcuts.
- **Helper Functions & Property Drawers:** Intuitive utilities to streamline your workflow.
- **Design Hierarchies:** Enhanced hierarchy views for better project organization.
- **Script Icons:** Assign and manage custom script icons for visual clarity.
- **New Project Setup:** Automate initial project configuration steps.

### Runtime Tools
- **Custom Attributes & Utilities:** Reusable attributes and helper methods to simplify your code.
- **Custom Debug Logs:** Enhanced logging functionality for effective troubleshooting.
- **Runtime Scriptables:** Manage and persist game data using ScriptableObjects.

## Installation

Add **Unity-CoreToolkit** to your project via the Unity Package Manager (UPM).

### Option 1: Using Git URL

1. Open the **Unity Package Manager**.
2. Click the **"+"** button and select **"Install package from git URL..."**.
3. Enter the following URL: [https://github.com/reshadhstu/Unity-CoreToolkit.git](https://github.com/reshadhstu/Unity-CoreToolkit.git)

### Option 2: Using manifest.json

Add the package directly to your project's Packages/manifest.json:

```json
{
  "dependencies": {
    "com.reshad.coretoolkit": "https://github.com/reshadhstu/Unity-CoreToolkit.git"
  }
}
```

## Usage
Once installed, access the tools via the Tools > CoreToolkit menu in the Unity Editor. To use runtime utilities in your scripts, import the relevant namespaces:

```csharp
using CoreToolkit.Runtime.Custom_Debug_Log.Scripts;
using UnityEngine;

public class ExampleUsage : MonoBehaviour
{
    private void Start()
    {
        CustomDebug.Log("Hello from Unity-CoreToolkit!");
    }
}
```

## Contributing
Contributions are welcome! If you have ideas, improvements, or bug fixes, please open an issue or submit a pull request on the [GitHub repository](https://github.com/reshadhstu/Unity-CoreToolkit).

## License
This project is licensed under the [MIT License](https://github.com/reshadhstu/Unity-CoreToolkit/blob/main/LICENSE.md).

## Special Thanks
- **Git Amend** – for inspiring many of the solutions through his YouTube tutorials.
- Colleagues, friends, and connections for their contributions and ideas.

Stay tuned for updates and new features in **Unity-CoreToolkit**!
