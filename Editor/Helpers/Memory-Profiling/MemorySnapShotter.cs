using System;
using System.IO;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Profiling;
using Unity.Profiling.Memory;
using UnityEditor;
using UnityEngine;

namespace CoreToolkit.Editor.Helpers.Memory_Profiling
{
    public class MemorySnapShotter : EditorWindow
    {
        private MemorySnapshotter _snapshotter;

        [MenuItem("Tools/CoreToolkit/MemoryProfiler/TakeMemorySnapShot")]
        public static void ShowWindow()
        {
            // Opens the window as a dockable editor panel
            GetWindow<MemorySnapShotter>("Memory Snapshotter");
        }

        private void OnEnable()
        {
            _snapshotter = new MemorySnapshotter();
        }

        private void OnGUI()
        {
            GUILayout.Label("Memory Snapshot Tool", EditorStyles.boldLabel);
            if (GUILayout.Button("Take Memory Snapshot"))
            {
                _snapshotter.TakeSnapshot();
            }
        }
    }

    // This class contains the core snapshot functionality.
    // It also makes use of unsafe code.
    public class MemorySnapshotter
    {
        const string capturesFolder = "MemoryCaptures";
        const string snapshotPrefix = "MemoryProfiler_";
        const string snapshotExt = ".snap";
        const string screenshotExt = ".png";

        string memoryCapturesPath = string.Empty;
        string snapshotPath = string.Empty;

        public MemorySnapshotter()
        {
            // Get the project folder (parent of Assets)
            string projectPath = Directory.GetParent(Application.dataPath)?.FullName;
            memoryCapturesPath = Path.Combine(projectPath, capturesFolder);

            if (!Directory.Exists(memoryCapturesPath))
            {
                Directory.CreateDirectory(memoryCapturesPath);
            }
        }

        public void TakeSnapshot()
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            snapshotPath = Path.Combine(memoryCapturesPath, $"{snapshotPrefix}{timestamp}{snapshotExt}");

            // MemoryProfiler takes a snapshot and calls the provided callbacks when complete.
            MemoryProfiler.TakeSnapshot(snapshotPath, OnSnapshotFinished, OnScreenshotCaptured);
        }

        static void OnSnapshotFinished(string path, bool result)
        {
            if (result)
            {
                Debug.Log($"Snapshot saved to {path}");
            }
            else
            {
                Debug.LogError("Snapshot failed");
            }
        }

        static void OnScreenshotCaptured(string path, bool result, DebugScreenCapture screenCapture)
        {
            if (result)
            {
                SaveScreenshot(path, screenCapture);
            }
            else
            {
                Debug.LogError("Screenshot capture failed");
            }
        }

        static void SaveScreenshot(string path, DebugScreenCapture screenCapture)
        {
            string screenshotPath = Path.ChangeExtension(path, screenshotExt);
            Debug.Log($"Saving screenshot to {screenshotPath} with size {screenCapture.RawImageDataReference.Length} bytes.");
            var imageData = CopyDataToManagedArray(screenCapture.RawImageDataReference);

            Texture2D screenshot = new Texture2D(screenCapture.Width, screenCapture.Height, screenCapture.ImageFormat, false);
            screenshot.LoadRawTextureData(imageData);
            screenshot.Apply();

            byte[] pngImage = screenshot.EncodeToPNG();
            File.WriteAllBytes(screenshotPath, pngImage);

            UnityEngine.Object.DestroyImmediate(screenshot);
        }

        static byte[] CopyDataToManagedArray(NativeArray<byte> nativeArray)
        {
            var managedArray = new byte[nativeArray.Length];
            unsafe
            {
                fixed (byte* destination = managedArray)
                {
                    Buffer.MemoryCopy(
                        NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(nativeArray),
                        destination,
                        nativeArray.Length,
                        nativeArray.Length);
                }
            }
            return managedArray;
        }
    }
}