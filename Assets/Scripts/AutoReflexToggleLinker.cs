using UnityEngine;
using UnityEngine.UI;

public class AutoReflexToggleLinker : MonoBehaviour
{
    private Toggle _toggle;

    void Awake()
    {
        _toggle = GetComponent<Toggle>();
        _toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    void OnEnable()
    {
        if (TimeManager.Instance != null)
        {
            _toggle.isOn = TimeManager.Instance.isAutoSlowEnabled;
        }
    }

    private void OnToggleChanged(bool value)
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.SetAutoSlowPreference(value);
        }
    }
}