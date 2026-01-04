<<<<<<< HEAD
using UnityEngine;
using UnityEngine.UI;

public class OverclockVisuals : MonoBehaviour
{
    [Header("UI Tint")]
    public Image screenTintPanel;
    public float targetAlpha = 0.5f;
    public float fadeSpeed = 5f;

    [Header("Player Highlight")]
    private SpriteRenderer playerSprite; 
    public Color highlightColor = Color.cyan;
    private Color _defaultColor;

    private bool _isActive = false;

    void Start()
    {
        if (screenTintPanel)
        {
            Color c = screenTintPanel.color;
            c.a = 0f;
            screenTintPanel.color = c;
        }
    }

    void Update()
    {
        if (playerSprite == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) 
            {
                playerSprite = playerObj.GetComponent<SpriteRenderer>();
                if (playerSprite) _defaultColor = playerSprite.color;
            }
        }

        bool isSlow = Time.timeScale < 0.9f && Time.timeScale > 0f; 

        if (screenTintPanel)
        {
            float target = isSlow ? targetAlpha : 0f;
            Color c = screenTintPanel.color;
            c.a = Mathf.Lerp(c.a, target, Time.unscaledDeltaTime * fadeSpeed);
            screenTintPanel.color = c;
        }

        if (playerSprite)
        {
            if (isSlow && !_isActive)
            {
                playerSprite.color = highlightColor;
                _isActive = true;
            }
            else if (!isSlow && _isActive)
            {
                playerSprite.color = _defaultColor;
                _isActive = false;
            }
        }
    }
=======
using UnityEngine;
using UnityEngine.UI;

public class OverclockVisuals : MonoBehaviour
{
    [Header("UI Tint")]
    public Image screenTintPanel;
    public float targetAlpha = 0.5f;
    public float fadeSpeed = 5f;

    [Header("Player Highlight")]
    private SpriteRenderer playerSprite; 
    public Color highlightColor = Color.cyan;
    private Color _defaultColor;

    private bool _isActive = false;

    void Start()
    {
        if (screenTintPanel)
        {
            Color c = screenTintPanel.color;
            c.a = 0f;
            screenTintPanel.color = c;
        }
    }

    void Update()
    {
        if (playerSprite == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) 
            {
                playerSprite = playerObj.GetComponent<SpriteRenderer>();
                if (playerSprite) _defaultColor = playerSprite.color;
            }
        }

        bool isSlow = Time.timeScale < 0.9f && Time.timeScale > 0f; 

        if (screenTintPanel)
        {
            float target = isSlow ? targetAlpha : 0f;
            Color c = screenTintPanel.color;
            c.a = Mathf.Lerp(c.a, target, Time.unscaledDeltaTime * fadeSpeed);
            screenTintPanel.color = c;
        }

        if (playerSprite)
        {
            if (isSlow && !_isActive)
            {
                playerSprite.color = highlightColor;
                _isActive = true;
            }
            else if (!isSlow && _isActive)
            {
                playerSprite.color = _defaultColor;
                _isActive = false;
            }
        }
    }
>>>>>>> upstream/dev
}