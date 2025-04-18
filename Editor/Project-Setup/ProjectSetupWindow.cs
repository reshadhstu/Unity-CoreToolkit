using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using static UnityEditor.AssetDatabase;
using static System.IO.Path;

namespace CoreToolkit.Editor.Project_Setup
{
    public class ProjectSetupWindow : EditorWindow
    {
        [MenuItem("Tools/CoreToolkit/Project Setup Window")]
        public static void ShowWindow()
        {
            GetWindow<ProjectSetupWindow>("Project Setup");
        }

        private string _foldersText = "_Project\n_Project/Animations\n_Project/Animators\n_Project/Art\n_Project/Art/3D-Models\n_Project/Art/3D-Models/Materials\n_Project/Art/3D-Models/Textures\n_Project/Art/UI\n_Project/Art/Shaders\n_Project/Art/Particles\n_Project/Audio\n_Project/Audio/Music\n_Project/Audio/SFX\n_Project/Scripts\n_Project/Scripts/Editor\n_Project/Scripts/Runtime\n_Project/Scripts/Runtime/Installers\n_Project/Scripts/Runtime/Managers\n_Project/Scripts/Runtime/UI\n_Project/Scripts/Runtime/Helpers\n_Project/Scripts/Runtime/Extras\n_Project/Scripts/Runtime/Signals\n_Project/Scripts/Runtime/SO\n_Project/Docs\n_Project/Prefabs\n_Project/Resources\n_Project/RenderTextures\n_Project/ScriptableObjects\n_Project/Videos\n_Project/Presets\n_Project/Settings";
        private List<string> _unityPackages;
        private List<string> _gitPackages;
        private List<string> _assetStoreAssets;
        private string _companyName = "Alienide Interactive";
        private string _productName = "My Project";
        private string _version = "0.0.1";

        private bool _isTaskRunning;
        private float _currentProgress;
        private string _currentMessage = "";
        private Vector2 _scrollPosition;
        private Vector2 _foldersScrollPos;
        private Vector2 _scrollUnityPackages;
        private Vector2 _scrollGitPackages;
        private Vector2 _scrollAssetStoreAssets;

        private bool _foldersFoldout = true;
        private bool _unityPackagesFoldout = true;
        private bool _gitPackagesFoldout = true;
        private bool _assetStoreAssetsFoldout = true;
        private bool _projectSettingsFoldout = true;

        private int _unityPackageToRemove = -1;
        private int _gitPackageToRemove = -1;
        private int _assetStoreAssetToRemove = -1;

        private void OnEnable()
        {
            // Load folders text
            string defaultFolders = "_Project\n_Project/Animations\n_Project/Animators\n_Project/Art\n_Project/Art/3D-Models\n_Project/Art/3D-Models/Materials\n_Project/Art/3D-Models/Textures\n_Project/Art/UI\n_Project/Art/Shaders\n_Project/Art/Particles\n_Project/Audio\n_Project/Audio/Music\n_Project/Audio/SFX\n_Project/Scripts\n_Project/Scripts/Editor\n_Project/Scripts/Runtime\n_Project/Scripts/Runtime/Installers\n_Project/Scripts/Runtime/Managers\n_Project/Scripts/Runtime/UI\n_Project/Scripts/Runtime/Helpers\n_Project/Scripts/Runtime/Extras\n_Project/Scripts/Runtime/Signals\n_Project/Scripts/Runtime/SO\n_Project/Docs\n_Project/Prefabs\n_Project/Resources\n_Project/RenderTextures\n_Project/ScriptableObjects\n_Project/Videos\n_Project/Presets\n_Project/Settings";
            _foldersText = EditorPrefs.GetString("ProjectSetup_FoldersText", defaultFolders);

            // Load project settings
            _companyName = EditorPrefs.GetString("ProjectSetup_CompanyName", "Alienide Interactive");
            _productName = EditorPrefs.GetString("ProjectSetup_ProductName", "My Project");
            _version = EditorPrefs.GetString("ProjectSetup_Version", "0.0.1");
            
            // Load packages
            string defaultUnityPackages = "com.unity.memoryprofiler\ncom.unity.inputsystem";
            string savedUnityPackages = EditorPrefs.GetString("ProjectSetup_UnityPackages", defaultUnityPackages);
            _unityPackages = string.IsNullOrEmpty(savedUnityPackages) ? new List<string>() : savedUnityPackages.Split('\n').ToList();

            // Load git packages
            string defaultGitPackages = "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask";
            string savedGitPackages = EditorPrefs.GetString("ProjectSetup_GitPackages", defaultGitPackages);
            _gitPackages = string.IsNullOrEmpty(savedGitPackages) ? new List<string>() : savedGitPackages.Split('\n').ToList();

            // Load asset store assets
            string defaultAssetStoreAssets = "Odin Inspector and Serializer\nOdin Validator\nDOTween Pro\nExtenject Dependency Injection IOC";
            string savedAssetStoreAssets = EditorPrefs.GetString("ProjectSetup_AssetStoreAssets", defaultAssetStoreAssets);
            _assetStoreAssets = string.IsNullOrEmpty(savedAssetStoreAssets) ? new List<string>() : savedAssetStoreAssets.Split('\n').ToList();
        }

        private void OnDisable()
        {
            // Save folder structure to EditorPrefs
            EditorPrefs.SetString("ProjectSetup_FoldersText", _foldersText);

            // Save project settings to EditorPrefs
            EditorPrefs.SetString("ProjectSetup_CompanyName", _companyName);
            EditorPrefs.SetString("ProjectSetup_ProductName", _productName);
            EditorPrefs.SetString("ProjectSetup_Version", _version);
            
            // Save packages to EditorPrefs
            EditorPrefs.SetString("ProjectSetup_UnityPackages", string.Join("\n", _unityPackages));
            EditorPrefs.SetString("ProjectSetup_GitPackages", string.Join("\n", _gitPackages));
            EditorPrefs.SetString("ProjectSetup_AssetStoreAssets", string.Join("\n", _assetStoreAssets));
        }

        private void OnGUI()
        {
            if (_isTaskRunning)
            {
                EditorGUI.ProgressBar(new Rect(10, 10, position.width - 20, 20), _currentProgress, _currentMessage);
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.Space(30); // Space below the progress bar

            EditorGUILayout.LabelField("Project Setup Configuration", EditorStyles.boldLabel);

            // Folders Section
            _foldersFoldout = EditorGUILayout.Foldout(_foldersFoldout, "Folders");
            if (_foldersFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Folders", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Enter folders to create, one per line. Use '/' for subfolders (e.g., '_Project/Art').", MessageType.Info);
                EditorGUILayout.BeginVertical(GUILayout.Height(100));
                _foldersScrollPos = EditorGUILayout.BeginScrollView(_foldersScrollPos);
                _foldersText = EditorGUILayout.TextArea(_foldersText, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
                GUI.enabled = !_isTaskRunning;
                if (GUILayout.Button("Create Folders"))
                {
                    RunTask(progressCallback => ProjectSetupClass.CreateFoldersAsync(GetFolders(), progressCallback));
                }
                GUI.enabled = true;
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();

            // Unity Packages Section
            _unityPackagesFoldout = EditorGUILayout.Foldout(_unityPackagesFoldout, "Unity Packages");
            if (_unityPackagesFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Unity Packages", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Enter Unity package IDs (e.g., 'com.unity.inputsystem').", MessageType.Info);
                EditorGUILayout.BeginVertical(GUILayout.Height(100));
                _scrollUnityPackages = EditorGUILayout.BeginScrollView(_scrollUnityPackages);
                if (_unityPackages.Count == 0)
                {
                    EditorGUILayout.LabelField("No packages added.", EditorStyles.centeredGreyMiniLabel);
                }
                else
                {
                    for (int i = 0; i < _unityPackages.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        _unityPackages[i] = EditorGUILayout.TextField(_unityPackages[i]);
                        if (GUILayout.Button("-", GUILayout.Width(20)))
                        {
                            _unityPackageToRemove = i;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+", GUILayout.Width(50), GUILayout.Height(25)))
                {
                    _unityPackages.Add("");
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUI.enabled = !_isTaskRunning && _unityPackages.Any(p => !string.IsNullOrWhiteSpace(p));
                if (GUILayout.Button("Install Essential Unity Packages"))
                {
                    RunTask(progressCallback => ProjectSetupClass.InstallPackagesFromUnityRegistryAsync(GetUnityPackages(), progressCallback));
                }
                GUI.enabled = true;
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();

            // Git Packages Section
            _gitPackagesFoldout = EditorGUILayout.Foldout(_gitPackagesFoldout, "Git Packages");
            if (_gitPackagesFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Git Packages", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Enter Git URLs (e.g., 'https://github.com/user/repo.git?path=/path').", MessageType.Info);
                EditorGUILayout.BeginVertical(GUILayout.Height(100));
                _scrollGitPackages = EditorGUILayout.BeginScrollView(_scrollGitPackages);
                if (_gitPackages.Count == 0)
                {
                    EditorGUILayout.LabelField("No packages added.", EditorStyles.centeredGreyMiniLabel);
                }
                else
                {
                    for (int i = 0; i < _gitPackages.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        _gitPackages[i] = EditorGUILayout.TextField(_gitPackages[i]);
                        if (GUILayout.Button("-", GUILayout.Width(20)))
                        {
                            _gitPackageToRemove = i;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+", GUILayout.Width(50), GUILayout.Height(25)))
                {
                    _gitPackages.Add("");
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUI.enabled = !_isTaskRunning && _gitPackages.Any(p => !string.IsNullOrWhiteSpace(p));
                if (GUILayout.Button("Install Essential Git Packages"))
                {
                    RunTask(progressCallback => ProjectSetupClass.InstallPackagesFromGitAsync(GetGitPackages(), progressCallback));
                }
                GUI.enabled = true;
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();

            // Asset Store Assets Section
            _assetStoreAssetsFoldout = EditorGUILayout.Foldout(_assetStoreAssetsFoldout, "Asset Store Assets");
            if (_assetStoreAssetsFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Asset Store Assets", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Enter asset names as in Unity Package Manager (e.g., 'Odin Inspector and Serializer').", MessageType.Info);
                EditorGUILayout.BeginVertical(GUILayout.Height(100));
                _scrollAssetStoreAssets = EditorGUILayout.BeginScrollView(_scrollAssetStoreAssets);
                if (_assetStoreAssets.Count == 0)
                {
                    EditorGUILayout.LabelField("No assets added.", EditorStyles.centeredGreyMiniLabel);
                }
                else
                {
                    for (int i = 0; i < _assetStoreAssets.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        _assetStoreAssets[i] = EditorGUILayout.TextField(_assetStoreAssets[i]);
                        if (GUILayout.Button("-", GUILayout.Width(20)))
                        {
                            _assetStoreAssetToRemove = i;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+", GUILayout.Width(50), GUILayout.Height(25)))
                {
                    _assetStoreAssets.Add("");
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUI.enabled = !_isTaskRunning && _assetStoreAssets.Any(p => !string.IsNullOrWhiteSpace(p));
                if (GUILayout.Button("Install Essential Assets From Asset Store"))
                {
                    RunTask(progressCallback => ProjectSetupClass.ImportEssentialsFromAssetStoreAsync(GetAssetStoreAssets(), progressCallback));
                }
                GUI.enabled = true;
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();

            // Project Settings Section
            _projectSettingsFoldout = EditorGUILayout.Foldout(_projectSettingsFoldout, "Project Settings");
            if (_projectSettingsFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Project Settings", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Company Name:");
                _companyName = EditorGUILayout.TextField(_companyName);
                EditorGUILayout.LabelField("Product Name:");
                _productName = EditorGUILayout.TextField(_productName);
                EditorGUILayout.LabelField("Version:");
                _version = EditorGUILayout.TextField(_version);
                GUI.enabled = !_isTaskRunning;
                if (GUILayout.Button("Update Project Settings"))
                {
                    RunTask(progressCallback => ProjectSetupClass.UpdateProjectSettingsAsync(
                        _companyName, _productName, _version, progressCallback));
                }
                GUI.enabled = true;
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            // Run All Tasks Button
            GUI.enabled = !_isTaskRunning;
            if (GUILayout.Button("Run All the Tasks"))
            {
                RunAllTasks();
            }
            GUI.enabled = true;

            // Handle removals after GUI layout
            if (_unityPackageToRemove >= 0 && _unityPackageToRemove < _unityPackages.Count)
            {
                _unityPackages.RemoveAt(_unityPackageToRemove);
                _unityPackageToRemove = -1;
            }
            if (_gitPackageToRemove >= 0 && _gitPackageToRemove < _gitPackages.Count)
            {
                _gitPackages.RemoveAt(_gitPackageToRemove);
                _gitPackageToRemove = -1;
            }
            if (_assetStoreAssetToRemove >= 0 && _assetStoreAssetToRemove < _assetStoreAssets.Count)
            {
                _assetStoreAssets.RemoveAt(_assetStoreAssetToRemove);
                _assetStoreAssetToRemove = -1;
            }
        }

        private string[] GetFolders() => _foldersText.Split('\n').Select(f => f.Trim()).Where(f => !string.IsNullOrEmpty(f)).ToArray();
        private string[] GetUnityPackages() => _unityPackages.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
        private string[] GetGitPackages() => _gitPackages.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
        private string[] GetAssetStoreAssets() => _assetStoreAssets.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();

        private async void RunTask(Func<Action<float, string>, Task> taskFunc)
        {
            try
            {
                if (_isTaskRunning) return;
                _isTaskRunning = true;
                _currentProgress = 0f;
                _currentMessage = "Starting...";
                try
                {
                    await taskFunc(UpdateProgress);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Task failed: {ex.Message}");
                    _currentMessage = $"Error: {ex.Message}";
                }
                finally
                {
                    _isTaskRunning = false;
                    _currentProgress = 1f;
                    _currentMessage = "Task completed.";
                    Repaint();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"An unexpected error occurred: {e.Message}\n{e.StackTrace}");
            }
        }

        private async void RunAllTasks()
        {
            try
            {
                if (_isTaskRunning) return;
                _isTaskRunning = true;
                _currentProgress = 0f;
                _currentMessage = "Starting all tasks...";
                try
                {
                    await ProjectSetupClass.CreateFoldersAsync(GetFolders(), UpdateProgress);
                    await ProjectSetupClass.InstallPackagesFromUnityRegistryAsync(GetUnityPackages(), UpdateProgress);
                    await ProjectSetupClass.InstallPackagesFromGitAsync(GetGitPackages(), UpdateProgress);
                    await ProjectSetupClass.ImportEssentialsFromAssetStoreAsync(GetAssetStoreAssets(), UpdateProgress);
                    await ProjectSetupClass.UpdateProjectSettingsAsync(_companyName, _productName, _version, UpdateProgress);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Setup failed: {ex.Message}");
                    _currentMessage = $"Error: {ex.Message}";
                }
                finally
                {
                    _isTaskRunning = false;
                    _currentProgress = 1f;
                    _currentMessage = "All tasks completed.";
                    Repaint();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"An unexpected error occurred during all tasks: {e.Message}\n{e.StackTrace}");
                _currentMessage = $"Error: {e.Message}";
                _isTaskRunning = false;
                Repaint();
            }
        }

        private void UpdateProgress(float progress, string message)
        {
            _currentProgress = progress;
            _currentMessage = message;
            Repaint();
        }
    }

    public static class ProjectSetupClass
    {
        public static async Task CreateFoldersAsync(string[] folders, Action<float, string> progressCallback = null)
        {
            progressCallback?.Invoke(0f, "Creating folders...");
            for (int i = 0; i < folders.Length; i++)
            {
                Folders.Create(folders[i]);
                progressCallback?.Invoke((float)(i + 1) / folders.Length, $"Created {folders[i]}");
            }

            Refresh();
            Folders.Move("_Project", "Scenes");
            Folders.Move("_Project", "Settings");
            Folders.Delete("TutorialInfo");
            Refresh();

            MoveAsset("Assets/InputSystem_Actions.inputactions", "Assets/_Project/Settings/InputSystem_Actions.inputactions");
            DeleteAsset("Assets/Readme.asset");
            Refresh();

            progressCallback?.Invoke(1f, "Folders created and organized.");
            await Task.Yield();
        }

        public static async Task InstallPackagesFromUnityRegistryAsync(string[] packages, Action<float, string> progressCallback = null)
        {
            progressCallback?.Invoke(0f, "Installing Unity packages...");
            for (int i = 0; i < packages.Length; i++)
            {
                await Packages.InstallPackageAsync(packages[i]);
                progressCallback?.Invoke((float)(i + 1) / packages.Length, $"Installed {packages[i]}");
            }
            progressCallback?.Invoke(1f, "Unity packages installed.");
        }

        public static async Task InstallPackagesFromGitAsync(string[] packages, Action<float, string> progressCallback = null)
        {
            progressCallback?.Invoke(0f, "Installing Git packages...");
            for (int i = 0; i < packages.Length; i++)
            {
                await Packages.InstallPackageAsync(packages[i]);
                progressCallback?.Invoke((float)(i + 1) / packages.Length, $"Installed {packages[i]}");
            }
            progressCallback?.Invoke(1f, "Git packages installed.");
        }

        public static async Task ImportEssentialsFromAssetStoreAsync(string[] displayNames, Action<float, string> progressCallback = null)
        {
            progressCallback?.Invoke(0f, "Importing Asset Store assets...");
            foreach (var name in displayNames)
            {
                PackageManagerHelper.SearchAssetStorePurchases(name);
                await Task.Delay(500);
            }

            var tasks = displayNames.Select(name => TryImportAssetAsync(name, 3, 1000)).ToArray();
            for (int i = 0; i < tasks.Length; i++)
            {
                await tasks[i];
                progressCallback?.Invoke((float)(i + 1) / tasks.Length, $"Imported {displayNames[i]}");
            }
            progressCallback?.Invoke(1f, "Asset Store assets imported.");
        }

        public static async Task UpdateProjectSettingsAsync(string companyName, string productName, string version, Action<float, string> progressCallback = null)
        {
            progressCallback?.Invoke(0f, "Updating project settings...");
            PlayerSettings.companyName = companyName;
            PlayerSettings.productName = productName;
            PlayerSettings.bundleVersion = version;
            Debug.Log($"Project settings updated: Company={companyName}, Product={productName}, Version={version}");
            progressCallback?.Invoke(1f, "Project settings updated.");
            await Task.Yield();
        }

        #region Helper Classes
        private static class Folders
        {
            public static void Create(string folderPath)
            {
                var fullPath = Combine(Application.dataPath, folderPath);
                Directory.CreateDirectory(fullPath);
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

        private static class Packages
        {
            private static AddRequest _request;

            public static async Task InstallPackageAsync(string package)
            {
                _request = Client.Add(package);
                while (!_request.IsCompleted)
                {
                    await Task.Delay(100);
                }
                if (_request.Status == StatusCode.Success)
                {
                    Debug.Log("Installed: " + _request.Result.packageId);
                }
                else if (_request.Status >= StatusCode.Failure)
                {
                    Debug.LogError(_request.Error.message);
                    throw new Exception($"Failed to install package {package}: {_request.Error.message}");
                }
            }
        }

        private static async Task TryImportAssetAsync(string displayName, int maxAttempts, int delayMilliseconds)
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
            throw new Exception($"Failed to import package '{displayName}' after {maxAttempts} attempts. Ensure it is purchased and available.");
        }
        #endregion
    }
}