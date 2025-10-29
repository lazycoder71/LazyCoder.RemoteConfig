using System.IO;
using System.Text;
using LazyCoder.SO;
using UnityEngine;
using Sirenix.OdinInspector;

namespace LazyCoder.RemoteConfig
{
    public class RemoteConfigSos : ScriptableObjectSingleton<RemoteConfigSos>
    {
        [ListDrawerSettings(ListElementLabelName = "Key")]
        [SerializeField] private RemoteConfigData[] _data;

        public static RemoteConfigData[] Data => Instance._data;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitOnStartup()
        {
            foreach (var remoteConfigData in Instance._data)
            {
                remoteConfigData.Load();
            }

            FetchData();
        }

        [Button]
        private static void FetchData()
        {
#if LAZYCODER_FIREBASE_REMOTECONFIG
            RemoteConfigService.FetchData();
#endif      
        }

#if UNITY_EDITOR
        [FolderPath]
        [SerializeField] private string _scriptGeneratedPath = "Assets/_ROOT/Scripts";

        [SerializeField] private string _scriptGeneratedClassName = "RemoteConfig";

        [SerializeField] private string _scriptGeneratedNamespace = "Game";

        [Button]
        private void GenerateRemoteDataClass()
        {
            // Create directory if not exists
            if (!Directory.Exists(_scriptGeneratedPath))
                Directory.CreateDirectory(_scriptGeneratedPath);

            var filePath = $"{_scriptGeneratedPath}/{_scriptGeneratedClassName}.cs";

            StringBuilder str = new StringBuilder();

            str.AppendLine($"using LazyCoder.RemoteConfig;");
            str.AppendLine();
            str.AppendLine($"namespace {_scriptGeneratedNamespace}\n{{");
            str.AppendLine($"\tpublic struct {_scriptGeneratedClassName}\n\t{{");

            var dataArray = _data;

            for (int i = 0; i < dataArray.Length; i++)
            {
                var dataKey = dataArray[i].Key;

                switch (dataArray[i].Type)
                {
                    case RemoteConfigDataType.String:
                        str.AppendLine(
                            $"\t\tpublic static string {dataKey} => RemoteConfigSos.Data[{i}].CurrentValueString;");
                        break;

                    case RemoteConfigDataType.Boolean:
                        str.AppendLine(
                            $"\t\tpublic static bool {dataKey} => RemoteConfigSos.Data[{i}].CurrentValueBool;");
                        break;

                    case RemoteConfigDataType.Int:
                        str.AppendLine(
                            $"\t\tpublic static int {dataKey} => RemoteConfigSos.Data[{i}].CurrentValueInt;");
                        break;

                    case RemoteConfigDataType.Float:
                        str.AppendLine(
                            $"\t\tpublic static float {dataKey} => RemoteConfigSos.Data[{i}].CurrentValueFloat;");
                        break;
                }
            }

            str.AppendLine("\t}");
            str.AppendLine("\n}");

            var writer = new StreamWriter(filePath, false);
            writer.Write(str);
            writer.Close();

            UnityEditor.AssetDatabase.ImportAsset(filePath);
        }
#endif
    }
}