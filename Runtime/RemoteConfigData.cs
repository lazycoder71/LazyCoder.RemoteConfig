using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Firebase.RemoteConfig;

namespace LazyCoder.RemoteConfig
{
    [Serializable]
    public class RemoteConfigData
    {
        [SerializeField] private string _key;

        [SerializeField] private RemoteConfigDataType _type;

        [ShowIf("@Type == RemoteConfigDataType.String")]
        [SerializeField] private string _defaultValueString;

        [ShowIf("@Type == RemoteConfigDataType.String")]
        [ReadOnly]
        [ShowInInspector] private string _remoteValueString;

        [ShowIf("@Type == RemoteConfigDataType.Boolean")]
        [SerializeField] private bool _defaultValueBool;

        [ShowIf("@Type == RemoteConfigDataType.Boolean")]
        [ReadOnly]
        [ShowInInspector] private bool _remoteValueBool;

        [ShowIf("@Type == RemoteConfigDataType.Int")]
        [SerializeField] private int _defaultValueInt;

        [ShowIf("@Type == RemoteConfigDataType.Int")]
        [ReadOnly]
        [ShowInInspector] private int _remoteValueInt;

        public string Key => _key;
        
        public string DefaultValueString => _defaultValueString;

        public bool DefaultValueBool => _defaultValueBool;
        
        public int DefaultValueInt => _defaultValueInt;

        public RemoteConfigDataType Type => _type;

        private string PlayerPrefsKey => $"{nameof(LazyCoder.RemoteConfig)}_{_key}";

        public void SetupDataDefault()
        {
            switch (_type)
            {
                case RemoteConfigDataType.String:
                    PlayerPrefs.SetString(PlayerPrefsKey, _defaultValueString);
                    break;

                case RemoteConfigDataType.Boolean:
                    PlayerPrefs.SetInt(PlayerPrefsKey, _defaultValueBool ? 1 : 0);
                    break;

                case RemoteConfigDataType.Int:
                    PlayerPrefs.SetInt(PlayerPrefsKey, _defaultValueInt);
                    break;
            }
        }

        public void SetupData(ConfigValue result)
        {
            switch (_type)
            {
                case RemoteConfigDataType.String:
                    if (result.Source == ValueSource.RemoteValue)
                        PlayerPrefs.SetString(PlayerPrefsKey, result.StringValue);

                    _remoteValueString = PlayerPrefs.GetString(PlayerPrefsKey);
                    break;

                case RemoteConfigDataType.Boolean:
                    if (result.Source == ValueSource.RemoteValue)
                        PlayerPrefs.SetInt(PlayerPrefsKey, result.BooleanValue ? 1 : 0);

                    _remoteValueBool = PlayerPrefs.GetInt(PlayerPrefsKey) > 0;
                    break;

                case RemoteConfigDataType.Int:
                    if (result.Source == ValueSource.RemoteValue)
                        PlayerPrefs.SetInt(PlayerPrefsKey, int.Parse(result.StringValue));

                    _remoteValueInt = PlayerPrefs.GetInt(PlayerPrefsKey);
                    break;
            }
        }
    }

    public enum RemoteConfigDataType
    {
        String,
        Boolean,
        Int
    }
}