#if LAZYCODER_FIREBASE_REMOTECONFIG

using System;
using Cysharp.Threading.Tasks;
using LazyCoder.Core;
using UnityEngine;
using Firebase;
using Firebase.RemoteConfig;

namespace LazyCoder.RemoteConfig
{
    public static class RemoteConfigService
    {
        public static async UniTaskVoid FetchData()
        {
            var status = await FirebaseApp.CheckAndFixDependenciesAsync().AsUniTask();
            if (status == DependencyStatus.Available)
            {
                await FetchDataAsync();
            }
            else
            {
                LDebug.LogError(typeof(RemoteConfigService),
                    "Could not resolve all Firebase dependencies: " + status);
            }
        }

        private static async UniTask FetchDataAsync()
        {
            LDebug.Log(typeof(RemoteConfigService), "Fetching data...");

            try
            {
                await FirebaseRemoteConfig.DefaultInstance
                    .FetchAsync(TimeSpan.Zero)
                    .AsUniTask();

                LDebug.Log(typeof(RemoteConfigService), "Fetch completed successfully!");
            }
            catch (OperationCanceledException)
            {
                LDebug.Log(typeof(RemoteConfigService), "Fetch canceled.");
                return;
            }
            catch (Exception)
            {
                LDebug.Log(typeof(RemoteConfigService), "Fetch encountered an error.");
                // continue to check info and handle gracefully
            }

            var info = FirebaseRemoteConfig.DefaultInstance.Info;
            if (info.LastFetchStatus == LastFetchStatus.Success)
            {
                try
                {
                    await FirebaseRemoteConfig.DefaultInstance
                        .ActivateAsync()
                        .AsUniTask();

                    LDebug.Log(typeof(RemoteConfigService),
                        $"Remote data loaded and ready (last fetch time {info.FetchTime}).");

                    foreach (var data in RemoteConfigSos.Data)
                    {
                        if (string.IsNullOrEmpty(data.Key))
                            continue;

                        data.FetchData(FirebaseRemoteConfig.DefaultInstance.GetValue(data.Key));
                    }

                    LDebug.Log(typeof(RemoteConfigService), "Fetching data completed successfully!", Color.green);
                }
                catch (Exception e)
                {
                    LDebug.Log(typeof(RemoteConfigService), $"Activating remote config failed.\n{e}", Color.red);
                }
            }
            else
            {
                LDebug.Log(typeof(RemoteConfigService), "Fetching data did not completed!", Color.red);
            }
        }
    }
}

#endif