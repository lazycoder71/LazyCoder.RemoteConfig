using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LazyCoder.SO;
using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using Sirenix.OdinInspector;
using UnityEditor;

namespace LazyCoder.RemoteConfig
{
    public class RemoteConfigSos : ScriptableObjectSingleton<RemoteConfigSos>
    {
        [SerializeField] private DependencyStatus _dependencyStatus = DependencyStatus.UnavailableOther;
        [SerializeField] private bool _isSetupDefaultData = true;
        [SerializeField] private List<RemoteConfigData> _data;

        private bool _isFetchRemoteConfigCompleted = false;

        public static bool IsFetchRemoteConfigCompleted => Instance._isFetchRemoteConfigCompleted;
        public static List<RemoteConfigData> Data => Instance._data;
        public static DependencyStatus DependencyStatus => Instance._dependencyStatus;

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _isFetchRemoteConfigCompleted = false;

            if (_isSetupDefaultData)
            {
                foreach (var remoteConfigData in _data)
                {
                    remoteConfigData.SetupDataDefault();
                }
            }

            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                _dependencyStatus = task.Result;
                if (_dependencyStatus == DependencyStatus.Available)
                {
                    FetchDataAsync();
                }
                else
                {
                    Debug.LogError("Could not resolve all Firebase dependencies: " + _dependencyStatus);
                }
            });
        }

        private Task FetchDataAsync()
        {
            Debug.Log("Fetching data...");

            Task fetchTask = FirebaseRemoteConfig.DefaultInstance
                .FetchAsync(TimeSpan.Zero);

            if (fetchTask.IsCanceled)
            {
                Debug.Log("Fetch canceled.");
            }
            else if (fetchTask.IsFaulted)
            {
                Debug.Log("Fetch encountered an error.");
            }
            else if (fetchTask.IsCompleted)
            {
                Debug.Log("Fetch completed successfully!");
            }

            return fetchTask.ContinueWithOnMainThread(tast =>
            {
                var info = FirebaseRemoteConfig.DefaultInstance.Info;
                if (info.LastFetchStatus == LastFetchStatus.Success)
                {
                    FirebaseRemoteConfig.DefaultInstance.ActivateAsync().ContinueWithOnMainThread(task =>
                    {
                        Debug.Log($"Remote data loaded and ready (last fetch time {info.FetchTime}).");

                        foreach (var remoteConfigData in _data)
                        {
                            if (string.IsNullOrEmpty(remoteConfigData.Key))
                                continue;

                            remoteConfigData.SetupData(
                                FirebaseRemoteConfig.DefaultInstance.GetValue(remoteConfigData.Key));
                        }

                        _isFetchRemoteConfigCompleted = true;
                    });

                    Debug.Log("<color=Green>Firebase Remote Config Fetching completed!</color>");
                }
                else
                {
                    Debug.Log("Fetching data did not completed!");
                }
            });
        }

#if UNITY_EDITOR
        [FolderPath]
        [SerializeField] private string _scriptGeneratedPath = "Assets/_Root/Scripts";
        
        [Button]
        private void GenerateRemoteData()
        {
            if (!Directory.Exists(_scriptGeneratedPath))
            {
                Directory.CreateDirectory(_scriptGeneratedPath);
            }

            var productImplPath = $"{_scriptGeneratedPath}/RemoteData.cs";
            var str = "namespace LazyCoder.RemoteConfig\n{";
            str += "\n\tpublic struct RemoteData\n\t{";

            var listRmcData = _data;
            for (int i = 0; i < listRmcData.Count; i++)
            {
                var rmcKey = listRmcData[i].Key;

                str += $"\n\t\tpublic const string KEY_{rmcKey.ToUpper()} = \"{rmcKey}\";";

                switch (listRmcData[i].Type)
                {
                    case RemoteConfigDataType.String:
                        str +=
                            $"\n\t\tpublic const string DEFAULT_{rmcKey.ToUpper()} = \"{listRmcData[i].DefaultValueString}\";";
                        str += "\n\t\t#if VIRTUESKY_DATA";
                        str +=
                            $"\n\t\tpublic static string {rmcKey.ToUpper()} => VirtueSky.DataStorage.GameData.Get(KEY_{rmcKey.ToUpper()}, DEFAULT_{rmcKey.ToUpper()});";
                        str += "\n\t\t#else";
                        str +=
                            $"\n\t\tpublic static string {rmcKey.ToUpper()} => UnityEngine.PlayerPrefs.GetString(KEY_{rmcKey.ToUpper()}, DEFAULT_{rmcKey.ToUpper()});";
                        str += "\n\t\t#endif";
                        break;
                    case RemoteConfigDataType.Boolean:
                        str +=
                            $"\n\t\tpublic const bool DEFAULT_{rmcKey.ToUpper()} = {GetBool(listRmcData[i].DefaultValueBool)};";
                        str += "\n\t\t#if VIRTUESKY_DATA";
                        str +=
                            $"\n\t\tpublic static bool {rmcKey.ToUpper()} => VirtueSky.DataStorage.GameData.Get(KEY_{rmcKey.ToUpper()}, DEFAULT_{rmcKey.ToUpper()});";
                        str += "\n\t\t#else";
                        str +=
                            $"\n\t\tpublic static bool {rmcKey.ToUpper()} => UnityEngine.PlayerPrefs.GetInt(KEY_{rmcKey.ToUpper()}, DEFAULT_{rmcKey.ToUpper()} ? 1 : 0) > 0;";
                        str += "\n\t\t#endif";
                        break;
                    case RemoteConfigDataType.Int:
                        str +=
                            $"\n\t\tpublic const int DEFAULT_{rmcKey.ToUpper()} = {listRmcData[i].DefaultValueInt};";
                        str += "\n\t\t#if VIRTUESKY_DATA";
                        str +=
                            $"\n\t\tpublic static int {rmcKey.ToUpper()} => VirtueSky.DataStorage.GameData.Get(KEY_{rmcKey.ToUpper()}, DEFAULT_{rmcKey.ToUpper()});";
                        str += "\n\t\t#else";
                        str +=
                            $"\n\t\tpublic static int {rmcKey.ToUpper()} => UnityEngine.PlayerPrefs.GetInt(KEY_{rmcKey.ToUpper()}, DEFAULT_{rmcKey.ToUpper()});";
                        str += "\n\t\t#endif";
                        break;
                }
            }

            str += "\n\t}";
            str += "\n}";

            var writer = new StreamWriter(productImplPath, false);
            writer.Write(str);
            writer.Close();
            AssetDatabase.ImportAsset(productImplPath);

            string GetBool(bool condition)
            {
                return condition ? "true" : "false";
            }
        }
#endif
    }

    enum TypeInitRemoteConfig
    {
        InitOnAwake,
        InitOnStart
    }
}