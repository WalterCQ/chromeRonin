using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private float _shakeTimeRemaining;
    private float _shakePower;
    private float _shakeFadeTime;
    private float _shakeRotation;

    public float rotationMultiplier = 5f;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void Shake(float power, float duration)
    {
        _shakePower = power;
        _shakeTimeRemaining = duration;
        _shakeFadeTime = power / duration;
    }

    void LateUpdate()
    {
        if (_shakeTimeRemaining > 0)
        {
            _shakeTimeRemaining -= Time.unscaledDeltaTime;

            float xAmount = Random.Range(-1f, 1f) * _shakePower * 0.1f;
            float yAmount = Random.Range(-1f, 1f) * _shakePower * 0.1f;

            transform.position += new Vector3(xAmount, yAmount, 0f);

            _shakeRotation = Mathf.MoveTowards(_shakeRotation, 0f, Time.unscaledDeltaTime * _shakeFadeTime * rotationMultiplier);
            transform.rotation = Quaternion.Euler(0f, 0f, _shakeRotation * Random.Range(-1f, 1f));

            _shakePower = Mathf.MoveTowards(_shakePower, 0f, Time.unscaledDeltaTime * _shakeFadeTime);
        }
        else
        {
            transform.rotation = Quaternion.identity;
        }
    }
}