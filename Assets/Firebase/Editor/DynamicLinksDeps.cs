// <copyright file="DynamicLinksDeps.cs" company="Google Inc.">
// Copyright (C) 2016 Google Inc. All Rights Reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;

/// <summary>
/// This file is used to define dependencies, and pass them along to a system
/// which can resolve dependencies.
/// </summary>
[InitializeOnLoad]
public class FirebaseDynamicLinksDeps : AssetPostprocessor
{
    /// <summary>
    /// This is the entry point for "InitializeOnLoad". It will register the
    /// dependencies with the dependency tracking system.
    /// </summary>
    static FirebaseDynamicLinksDeps()
    {
        SetupDeps();
    }

    static void SetupDeps()
    {
#if UNITY_ANDROID
        // Setup the resolver using reflection as the module may not be
        // available at compile time.
        Type playServicesSupport = Google.VersionHandler.FindClass(
            "Google.JarResolver", "Google.JarResolver.PlayServicesSupport");
        if (playServicesSupport == null) {
            return;
        }

        Google.VersionHandler.InvokeStaticMethod(
            Google.VersionHandler.FindClass(
                "Google.JarResolver",
                "GooglePlayServices.PlayServicesResolver"),
            "AddAutoResolutionFilePatterns",
            new object[] { new HashSet<Regex> { new Regex(".*Deps\\.cs$") } });

        object svcSupport = Google.VersionHandler.InvokeStaticMethod(
            playServicesSupport, "CreateInstance",
            new object[] {
                "FirebaseDynamicLinks",
                EditorPrefs.GetString("AndroidSdkRoot"),
                "ProjectSettings"
            });

        Google.VersionHandler.InvokeInstanceMethod(
            svcSupport, "DependOn",
            new object[] {
                "com.google.android.gms",
                "play-services-appinvite",
                "11.2.0"
            },
            namedArgs: new Dictionary<string, object>() {
                { "packageIds",
                    new string[] {
                        "extra-google-m2repository"
                    }
                },
                { "repositories",
                    null
                }
            });
        Google.VersionHandler.InvokeInstanceMethod(
            svcSupport, "DependOn",
            new object[] {
                "com.google.firebase",
                "firebase-core",
                "11.2.0"
            },
            namedArgs: new Dictionary<string, object>() {
                { "packageIds",
                    new string[] {
                        "extra-google-m2repository",
                        "extra-android-m2repository"
                    }
                },
                { "repositories",
                    null
                }
            });
        Google.VersionHandler.InvokeInstanceMethod(
            svcSupport, "DependOn",
            new object[] {
                "com.google.firebase",
                "firebase-dynamic-links-unity",
                "4.1.0"
            },
            namedArgs: new Dictionary<string, object>() {
                { "packageIds",
                    null
                },
                { "repositories",
                    new string[] {
                        "Assets/Modules/umm@firebase_core/Firebase/m2repository",
                        "Assets/Modules/umm@firebase_dynamiclinks/Firebase/m2repository"
                    }
                }
            });
#elif UNITY_IOS
        Type iosResolver = Google.VersionHandler.FindClass(
            "Google.IOSResolver", "Google.IOSResolver");
        if (iosResolver == null) {
            return;
        }
        Google.VersionHandler.InvokeStaticMethod(
            iosResolver, "AddPod",
            new object[] { "Firebase/DynamicLinks" }, 
            new Dictionary<string, object>() { 
                { "version", "4.1.0" },
                { "minTargetSdk", null },
                { "sources", null }
            });
#endif
    }

    // Handle delayed loading of the dependency resolvers.
    private static void OnPostprocessAllAssets(
            string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromPath) {
        foreach (string asset in importedAssets) {
            if (asset.Contains("IOSResolver") ||
                asset.Contains("JarResolver")) {
                SetupDeps();
                break;
            }
        }
    }
}

