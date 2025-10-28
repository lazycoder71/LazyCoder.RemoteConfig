using System;
using Sirenix.OdinInspector;
using UnityEngine;

#if LAZYCODER_FIREBASE_REMOTECONFIG
using Firebase.RemoteConfig;
#endif

namespace LazyCoder.RemoteConfig
{
    [Serializable]
    public class RemoteConfigData
    {
        [SerializeField] private string _key;

        [SerializeField] private RemoteConfigDataType _type;

        [ShowIf("@_type == RemoteConfigDataType.String")]
        [SerializeField] private string _defaultValueString;

        [ShowIf("@_type == RemoteConfigDataType.String")]
        [ReadOnly]
        [ShowInInspector] private string _currentValueString;

        [ShowIf("@_type == RemoteConfigDataType.Boolean")]
        [SerializeField] private bool _defaultValueBool;

        [ShowIf("@_type == RemoteConfigDataType.Boolean")]
        [ReadOnly]
        [ShowInInspector] private bool _currentValueBool;

        [ShowIf("@_type == RemoteConfigDataType.Int")]
        [SerializeField] private int _defaultValueInt;

        [ShowIf("@_type == RemoteConfigDataType.Int")]
        [ReadOnly]
        [ShowInInspector] private int _currentValueInt;

        [ShowIf("@_type == RemoteConfigDataType.Float")]
        [SerializeField] private float _defaultValueFloat;

        [ShowIf("@_type == RemoteConfigDataType.Float")]
        [ReadOnly]
        [ShowInInspector] private float _currentValueFloat;

        public string Key => _key;

        public string CurrentValueString => _currentValueString;

        public bool CurrentValueBool => _currentValueBool;

        public int CurrentValueInt => _currentValueInt;

        public float CurrentValueFloat => _currentValueFloat;

        public RemoteConfigDataType Type => _type;

        private string PlayerPrefsKey => $"{nameof(LazyCoder.RemoteConfig)}_{_key}";

        public void Load()
        {
            switch (_type)
            {
                case RemoteConfigDataType.String:
                    _currentValueString = PlayerPrefs.GetString(PlayerPrefsKey, _defaultValueString);
                    break;

                case RemoteConfigDataType.Boolean:
                    _currentValueBool = PlayerPrefs.GetInt(PlayerPrefsKey, _defaultValueBool ? 1 : 0) > 0;
                    break;

                case RemoteConfigDataType.Int:
                    _currentValueInt = PlayerPrefs.GetInt(PlayerPrefsKey, _defaultValueInt);
                    break;

                case RemoteConfigDataType.Float:
                    _currentValueFloat = PlayerPrefs.GetFloat(PlayerPrefsKey, _defaultValueFloat);
                    break;
            }
        }

#if LAZYCODER_FIREBASE_REMOTECONFIG
        public void FetchData(ConfigValue result)
        {
            if (result.Source != ValueSource.RemoteValue)
                return;

            switch (_type)
            {
                case RemoteConfigDataType.String:
                    PlayerPrefs.SetString(PlayerPrefsKey, result.StringValue);
                    break;

                case RemoteConfigDataType.Boolean:
                    PlayerPrefs.SetInt(PlayerPrefsKey, result.BooleanValue ? 1 : 0);
                    break;

                case RemoteConfigDataType.Int:
                    int.TryParse(result.StringValue, out int intValue);

                    PlayerPrefs.SetInt(PlayerPrefsKey, intValue);
                    break;

                case RemoteConfigDataType.Float:
                    float.TryParse(result.StringValue, out float floatValue);

                    PlayerPrefs.SetFloat(PlayerPrefsKey, floatValue);
                    break;
            }

            Load();
        }
    }
#endif

    public enum RemoteConfigDataType
    {
        String,
        Boolean,
        Int,
        Float,
    }
}