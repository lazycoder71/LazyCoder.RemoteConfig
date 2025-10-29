#if LAZYCODER_FIREBASE_REMOTECONFIG

using System;
using LazyCoder.Core;
using Firebase;
using Firebase.Extensions;
using Firebase.RemoteConfig;

namespace LazyCoder.RemoteConfig
{
    public static class RemoteConfigService
    {
        // Accepts an optional CancellationToken
        public static void FetchData()
        {
            try
            {
                FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
                {
                    var dependencyStatus = task.Result;
                    if (dependencyStatus == DependencyStatus.Available)
                    {
                        // Call the async fetch method without awaiting
                        FetchRemoteData();
                    }
                    else
                    {
                        LDebug.LogError(typeof(RemoteConfigService),
                            $"Could not resolve all Firebase dependencies: {dependencyStatus}\n" +
                            "Firebase Unity SDK is not safe to use here");
                    }
                });
            }
            catch (Exception e)
            {
                LDebug.Log(typeof(RemoteConfigService), $"Error while checking dependencies: {e}");
            }
        }

        // Propagates the token to Firebase fetch/activate calls and checks during the loop
        private static void FetchRemoteData()
        {
            LDebug.Log(typeof(RemoteConfigService), "Fetching data...");

            FirebaseRemoteConfig.DefaultInstance
                .FetchAsync(TimeSpan.Zero)
                .ContinueWithOnMainThread(task =>
                {
                    if (!task.IsCompleted)
                    {
                        LDebug.Log(typeof(RemoteConfigService), "Fetching data did not completed!");
                        return;
                    }

                    var info = FirebaseRemoteConfig.DefaultInstance.Info;

                    if (info.LastFetchStatus == LastFetchStatus.Success)
                    {
                        FirebaseRemoteConfig.DefaultInstance
                            .ActivateAsync().ContinueWithOnMainThread(task =>
                            {
                                LDebug.Log(typeof(RemoteConfigService),
                                    $"Remote data loaded and ready (last fetch time {info.FetchTime}).");

                                foreach (var data in RemoteConfigSos.Data)
                                {
                                    if (string.IsNullOrEmpty(data.Key))
                                        continue;

                                    data.FetchData(FirebaseRemoteConfig.DefaultInstance.GetValue(data.Key));
                                }

                                LDebug.Log(typeof(RemoteConfigService), "Fetching data completed successfully!");
                            });
                    }
                });
        }
    }
}

#endif