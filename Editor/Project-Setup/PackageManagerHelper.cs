using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CoreToolkit.Editor.Project_Setup
{
    public static class PackageManagerHelper 
    {
        const string ServicesNamespace = "UnityEditor.PackageManager.UI.Internal";

        /// <summary>
        /// Resolves a PackageManager service by its interface name.
        /// Locates the service in the internal ServicesContainer based on the given interface name
        /// and returns the instance of the resolved service.
        /// </summary>
        /// <param name="interfaceName">The name of the interface to resolve (e.g., "IPackageDatabase").</param>
        /// <returns>The resolved service instance corresponding to the interface name.</returns>
        /// <exception cref="Exception">Thrown if any required type, property, or method cannot be found 
        /// or if the service cannot be resolved.</exception>
        static object ResolveService(string interfaceName) {
            try {
                // Retrieve the ServicesContainer type
                var servicesContainerType = Type.GetType($"{ServicesNamespace}.ServicesContainer, UnityEditor")
                                            ?? throw new Exception($"Failed to find {ServicesNamespace}.ServicesContainer type.");

                // Get the instance property from ScriptableSingleton
                var instanceProperty = typeof(ScriptableSingleton<>).MakeGenericType(servicesContainerType)
                                           .GetProperty("instance", BindingFlags.Static | BindingFlags.Public)
                                       ?? throw new Exception("Failed to find 'instance' property on ScriptableSingleton.");
                var servicesContainerInstance = instanceProperty.GetValue(null)
                                                ?? throw new Exception("Failed to get ServicesContainer instance.");

                // Retrieve the Resolve<T> method
                var resolveMethod = servicesContainerType.GetMethod("Resolve", BindingFlags.Instance | BindingFlags.Public)
                                    ?? throw new Exception("Failed to find 'Resolve' method on ServicesContainer.");

                // Resolve the interface type dynamically
                var interfaceType = Type.GetType($"{ServicesNamespace}.{interfaceName}, UnityEditor")
                                    ?? throw new Exception($"Failed to find interface type: {ServicesNamespace}.{interfaceName}.");

                // Invoke the generic Resolve<T> method
                return resolveMethod.MakeGenericMethod(interfaceType).Invoke(servicesContainerInstance, null);
            }
            catch (Exception ex) {
                throw new Exception($"Error resolving service '{interfaceName}': {ex.Message}");
            }
        }

        /// <summary>
        /// Imports a package from the Package Manager by its name.
        /// Uses the Unity Package Manager services to locate and import the package by triggering its installation.
        /// </summary>
        /// <param name="packageName">The name of the package to import (e.g., "com.unity.textmeshpro").</param>
        /// <returns>
        /// `true` if the package is successfully imported;  
        /// `false` if the service cannot be resolved, the package is not found, or an error occurs during installation.
        /// </returns>
        /// <exception cref="Exception">Thrown if any required service, method, or package property cannot be resolved.</exception>
        public static bool ImportPackageByName(string packageName) {
            try {
                // Resolve IPackageOperationDispatcher
                var dispatcher = ResolveService("IPackageOperationDispatcher")
                                 ?? throw new Exception("Failed to resolve IPackageOperationDispatcher.");

                // Find package by name
                var package = FindPackageByName(packageName)
                              ?? throw new Exception($"Package '{packageName}' not found in the cache.");

                // Retrieve m_AssetStorePackageInstaller
                var installerField = dispatcher.GetType()
                                         .GetField("m_AssetStorePackageInstaller", BindingFlags.Instance | BindingFlags.NonPublic)
                                     ?? throw new Exception("Failed to find m_AssetStorePackageInstaller field on IPackageOperationDispatcher.");
                var assetStorePackageInstaller = installerField.GetValue(dispatcher)
                                                 ?? throw new Exception("Failed to retrieve m_AssetStorePackageInstaller instance.");

                // Retrieve Install method
                var installMethod = assetStorePackageInstaller.GetType()
                                        .GetMethod("Install", BindingFlags.Instance | BindingFlags.Public)
                                    ?? throw new Exception("Failed to find Install method on AssetStorePackageInstaller.");

                // Resolve product and productId
                var product = package.GetType().GetProperty("product", BindingFlags.Instance | BindingFlags.Public)
                                  ?.GetValue(package)
                              ?? throw new Exception("Product information is null.");
                var productId = Convert.ToInt64(
                    product.GetType().GetProperty("id", BindingFlags.Instance | BindingFlags.Public)
                        ?.GetValue(product)
                    ?? throw new Exception("Failed to retrieve 'id' property on package product.")
                );

                // Invoke the install method
                installMethod.Invoke(assetStorePackageInstaller, new object[] { productId, false });

                Debug.Log($"Package '{packageName}' imported successfully.");
                return true;
            }
            catch (Exception ex) {
                Debug.LogError($"Error importing package '{packageName}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Finds a package in the Unity Package Manager by its name.
        /// Resolves the internal PackageCache and retrieves the package along with its version details.
        /// </summary>
        /// <param name="packageName">The name of the package to search for (e.g., "com.unity.textmeshpro").</param>
        /// <returns>
        /// The resolved package object if found;  
        /// `null` if the service, method, or package cannot be resolved.
        /// </returns>
        /// <exception cref="Exception">Thrown if any required service, method, or package property cannot be resolved.</exception>
        static object FindPackageByName(string packageName) {
            try {
                // Resolve the internal PackageDatabase
                var packageDatabase = ResolveService("IPackageDatabase")
                                      ?? throw new Exception("Failed to resolve IPackageDatabase.");

                // Retrieve the GetPackageAndVersionByIdOrName method
                var getPackageMethod = packageDatabase.GetType().GetMethod(
                                           "GetPackageAndVersionByIdOrName",
                                           BindingFlags.Instance | BindingFlags.Public
                                       )
                                       ?? throw new Exception("Failed to find GetPackageAndVersionByIdOrName method on IPackageDatabase.");

                // Prepare parameters and invoke the method
                object[] parameters = { packageName, null, null, true };
                getPackageMethod.Invoke(packageDatabase, parameters);

                return parameters[1]; // Return the resolved package
            }
            catch (Exception ex) {
                Debug.LogError($"Error while resolving package '{packageName}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Searches the user's Asset Store purchases based on a provided search text
        /// and ensures the results are loaded into the PackageManager cache.
        /// Utilizes Unity's Asset Store-related services to query purchased assets
        /// and populate the cache for further use.
        /// </summary>
        /// <param name="searchText">The text to search for in the Asset Store purchases (e.g., "AutoRef - Constants in Code").</param>
        /// <exception cref="Exception">Thrown if any required service or method cannot be resolved while executing the search.</exception>
        public static void SearchAssetStorePurchases(string searchText) {
            try {
                // Resolve AssetStoreClientV2
                var assetStoreClient = ResolveService("AssetStoreClientV2")
                                       ?? throw new Exception("Failed to resolve AssetStoreClientV2.");

                // Create PurchasesQueryArgs with the search text
                var purchasesQueryArgsType = typeof(ScriptableSingleton<>).Assembly.GetType("UnityEditor.PackageManager.UI.Internal.PurchasesQueryArgs")
                                             ?? throw new Exception("Failed to find PurchasesQueryArgs type.");
                var queryArgsConstructor = purchasesQueryArgsType.GetConstructor(
                                               new[] { typeof(int), typeof(int), typeof(string), Type.GetType("UnityEditor.PackageManager.UI.Internal.PageFilters, UnityEditor") }
                                           )
                                           ?? throw new Exception("Failed to find PurchasesQueryArgs constructor.");
                var queryArgs = queryArgsConstructor.Invoke(new object[] { 0, 0, searchText, null });

                // Call ListPurchases
                var listPurchasesMethod = assetStoreClient.GetType().GetMethod("ListPurchases", BindingFlags.Instance | BindingFlags.Public)
                                          ?? throw new Exception("Failed to find ListPurchases method on AssetStoreClientV2.");
                listPurchasesMethod.Invoke(assetStoreClient, new object[] { queryArgs });

                Debug.Log($"Successfully triggered a search for '{searchText}' in the Asset Store.");
            }
            catch (Exception ex) {
                Debug.LogError($"Error searching Asset Store purchases: {ex.Message}");
            }
        }

        // Testing only
        public static void ClearAssetStoreOnlineCache() {
            try {
                // Resolve AssetStoreClientV2 and retrieve m_AssetStoreCache
                var assetStoreClient = ResolveService("AssetStoreClientV2")
                                       ?? throw new Exception("Failed to resolve AssetStoreClientV2.");
                var cacheField = assetStoreClient.GetType().GetField("m_AssetStoreCache", BindingFlags.Instance | BindingFlags.NonPublic)
                                 ?? throw new Exception("m_AssetStoreCache field not found on AssetStoreClientV2.");
                var assetStoreCache = cacheField.GetValue(assetStoreClient)
                                      ?? throw new Exception("m_AssetStoreCache instance is null.");

                // Find and invoke ClearOnlineCache from m_AssetStoreCache
                var clearCacheMethod = assetStoreCache.GetType().GetMethod("ClearOnlineCache", BindingFlags.Instance | BindingFlags.Public)
                                       ?? throw new Exception("ClearOnlineCache method not found on IAssetStoreCache.");

                clearCacheMethod.Invoke(assetStoreCache, null);

                Debug.Log("Successfully cleared the Asset Store online cache.");
            }
            catch (Exception ex) {
                Debug.LogError($"Error clearing Asset Store cache: {ex.Message}");
            }
        }
    }
}