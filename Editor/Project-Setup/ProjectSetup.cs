using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
//using TMPro.EditorUtilities;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using static UnityEditor.AssetDatabase;
using static System.IO.Path;

namespace CoreToolkit.Editor.Project_Setup
{
    public static class ProjectSetup
    {
        // Main Setup: Run all steps in order
        [MenuItem("Tools/Project Setup/First Time Setup (NEW)", false, 21)]
        public static void FirstTimeSetup()
        {
            CreateFolders();
            InstallPackages();
            ImportEssentials();
            ImportLocalPackages();
            UpdateProjectSettings();
        }

        // 1. Create Folders
        [MenuItem("Tools/Project Setup/Create Folders", false, 22)]
        public static void CreateFolders()
        {
            Folders.Create("_Project", "Animations", "Animators", "Art", "Audios",
                "Scripts", "Docs", "Prefabs", "Resources", "RenderTextures", "ScriptableObjects", "Videos", "Presets", "Settings");
            Folders.Create("_Project/Art", "3D-Models", "UI", "Shaders", "Particles");
            Folders.Create("_Project/3D-Models", "Materials", "Textures");
            Folders.Create("_Project/Audio", "Music", "SFX");
            Folders.Create("_Project/Scripts", "Editor", "Runtime");
            Folders.Create("_Project/Scripts/Runtime", "Installers", "Managers", "UI", "Helpers", "Extras", "Signals", "SO");

            Refresh();
            Folders.Move("_Project", "Scenes");
            Folders.Move("_Project", "Settings");
            Folders.Delete("TutorialInfo");
            Refresh();

            MoveAsset("Assets/InputSystem_Actions.inputactions", "Assets/_Project/Settings/InputSystem_Actions.inputactions");
            DeleteAsset("Assets/Readme.asset");
            Refresh();
        }

        // 2. Import TextMesh Pro Essentials
        //[MenuItem("Tools/Project Setup/Import TMP Text", false, 23)]
        private static void ImportTMP_Text()
        {
            // Import TextMesh Pro Essential Resources
            //ImportPackage(TMP_EditorUtility.packageFullPath + "/Package Resources/TMP Essential Resources.unitypackage", false);
            // Import Examples & Extras if needed
            //ImportPackage(TMP_EditorUtility.packageFullPath + "/Package Resources/TMP Examples & Extras.unitypackage", false);
        }

        // 3. Install Packages via Package Manager (e.g. UniTask)
        [MenuItem("Tools/Project Setup/Install Packages", false, 24)]
        public static void InstallPackages()
        {
            Packages.InstallPackages(new[]
            {
                // Uncomment or add additional packages as needed.
                //"com.unity.2d.animation",
                //"git+https://github.com/adammyhre/Unity-Utils.git",
                "git+https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask"
                // If necessary, import new Input System last (requires Editor restart)
                // "com.unity.inputsystem"
            });
        }

        // 4. Import Essential Assets from the Asset Store
        [MenuItem("Tools/Project Setup/Import Essential Assets From Asset Store", false, 25)]
        public static async void ImportEssentials()
        {
            // IMPORTANT: Use Asset Names as displayed in the Unity Package Manager
            var displayNames = new[]
            {
                "Odin Inspector and Serializer",
                "Odin Validator",
                "DOTween Pro",
                "Extenject Dependency Injection IOC"
            };

            // Fill PackageDatabase cache for each asset
            foreach (var displayName in displayNames)
            {
                PackageManagerHelper.SearchAssetStorePurchases(displayName);
                await Task.Delay(500); // Small delay to ensure the search completes
            }

            // Perform asset imports in parallel
            await Task.WhenAll(displayNames.Select(name => TryImportAssetAsync(name, 3, 1000)));
        }

        // 5. Import Local Packages
        [MenuItem("Tools/Project Setup/Import Local Packages", false, 26)]
        private static void ImportLocalPackages()
        {
            // Adjust these paths as needed
            ImportPackage("E:/Unity Storage/PlayerInputActions.unitypackage", false);
            ImportPackage("E:/Unity Storage/DustParticles.unitypackage", false);
        }

        // 6. Update project settings (e.g., Company Name)
        [MenuItem("Tools/Project Setup/Update Project Settings", false, 27)]
        private static void UpdateProjectSettings()
        {
            string newCompanyName = "Alienide Interactive";
            PlayerSettings.companyName = newCompanyName;
            Debug.Log($"Company Name updated to: {PlayerSettings.companyName}");
        }

        #region Helper Classes

        static class Folders
        {
            public static void Create(string root, params string[] folders)
            {
                var fullPath = Combine(Application.dataPath, root);
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }

                foreach (var folder in folders)
                {
                    CreateSubFolders(fullPath, folder);
                }
            }

            static void CreateSubFolders(string rootPath, string folderHierarchy)
            {
                var folderNames = folderHierarchy.Split('/');
                var currentPath = rootPath;

                foreach (var folder in folderNames)
                {
                    currentPath = Combine(currentPath, folder);
                    if (!Directory.Exists(currentPath))
                    {
                        Directory.CreateDirectory(currentPath);
                    }
                }
            }

            public static void Move(string newParent, string folderName)
            {
                var sourcePath = $"Assets/{folderName}";
                if (IsValidFolder(sourcePath))
                {
                    var destinationPath = $"Assets/{newParent}/{folderName}";
                    var error = MoveAsset(sourcePath, destinationPath);
                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogError($"Failed to move {folderName}: {error}");
                    }
                }
            }

            public static void Delete(string folderName)
            {
                var pathToDelete = $"Assets/{folderName}";
                if (IsValidFolder(pathToDelete))
                {
                    DeleteAsset(pathToDelete);
                }
            }
        }

        static class Packages
        {
            static AddRequest request;
            static Queue<string> packagesToInstall = new();

            public static void InstallPackages(string[] packages)
            {
                foreach (var package in packages)
                {
                    packagesToInstall.Enqueue(package);
                }

                if (packagesToInstall.Count > 0)
                {
                    StartNextPackageInstallation();
                }
            }

            static async void StartNextPackageInstallation()
            {
                request = Client.Add(packagesToInstall.Dequeue());

                while (!request.IsCompleted)
                    await Task.Delay(10);

                if (request.Status == StatusCode.Success)
                    Debug.Log("Installed: " + request.Result.packageId);
                else if (request.Status >= StatusCode.Failure)
                    Debug.LogError(request.Error.message);

                if (packagesToInstall.Count > 0)
                {
                    await Task.Delay(1000);
                    StartNextPackageInstallation();
                }
            }
        }

        static async Task TryImportAssetAsync(string displayName, int maxAttempts, int delayMilliseconds)
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    if (PackageManagerHelper.ImportPackageByName(displayName))
                    {
                        Debug.Log($"Successfully imported package '{displayName}' on attempt {attempt}.");
                        return;
                    }
                    Debug.LogWarning($"Attempt {attempt}: Package '{displayName}' not found. Retrying...");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error importing package '{displayName}' on attempt {attempt}: {ex.Message}");
                }
                await Task.Delay(delayMilliseconds);
            }
            Debug.LogError($"Failed to import package '{displayName}' after {maxAttempts} attempts.");
        }

        #endregion
    }
}
