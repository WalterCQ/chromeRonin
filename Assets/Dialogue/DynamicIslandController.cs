using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class SmartIslandController : MonoBehaviour
{
    public static SmartIslandController Instance;

    [Header("UI 组件连接")]
    public RectTransform islandRect; 
    public Image avatarImage;       
    public TextMeshProUGUI textA;
    public TextMeshProUGUI textB;
    public AudioSource audioSource;
    
    [Header("声波图组件")]
    public RectTransform waveformContainer; // 拖入刚才创建的 WaveformContainer
    public List<RectTransform> audioBars;   // 拖入那几个 Bar 的 RectTransform

    [Header("位置设置")]
    public float onScreenY = 80f;    
    public float offScreenY = -150f; 

    [Header("外观与故障设置")]
    public float textLeftOffset = 80f;          
    public float expandSpeed = 0.4f;            
    public Vector2 idleSize = new Vector2(100, 60); 
    public float baseWidth = 80f;               
    public float textPadding = 50f;             
    public float minDuration = 2.0f;
    
    [Header("声波灵敏度")]
    public float sensitivity = 100f; // 调节这个让条跳得更高

    private bool isUsingA = true;
    private Coroutine dialogueCoroutine;
    private float[] spectrumData = new float[64]; // 存储音频数据

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        islandRect.anchoredPosition = new Vector2(0, offScreenY);
        islandRect.sizeDelta = idleSize;
        
        textA.text = ""; textB.text = "";
        textA.alpha = 0; textB.alpha = 0;
        avatarImage.color = new Color(1, 1, 1, 0);
        
        textA.rectTransform.anchoredPosition = new Vector2(textLeftOffset, 0);
        textB.rectTransform.anchoredPosition = new Vector2(textLeftOffset, 0);
        
        // 初始隐藏声波
        if(waveformContainer) waveformContainer.gameObject.SetActive(false);
        
        // Apply saved SFX volume to dialogue audio
        ApplySFXVolume();
    }
    
    private void ApplySFXVolume()
    {
        if (audioSource != null)
        {
            float volume = SFXManager.Instance != null ? SFXManager.GetVolume() : PlayerPrefs.GetFloat("SFXVolume", 0.75f);
            audioSource.volume = volume;
        }
    }

    // --- 每帧更新声波图 ---
    void Update()
    {
        // 只有当有声音播放，且声波容器开启时才计算
        if (audioSource.isPlaying && waveformContainer.gameObject.activeSelf)
        {
            // 1. 获取频谱数据 (FFT)
            audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.Rectangular);

            // 2. 把数据映射到每一个 Bar 的高度上
            // 我们跳着取样，因为低频(靠前的index)能量比较大
            for (int i = 0; i < audioBars.Count; i++)
            {
                if (i >= spectrumData.Length) break;

                // 获取数据并放大
                float intensity = spectrumData[i * 2] * sensitivity;
                // 限制高度在 2 到 40 之间
                float height = Mathf.Clamp(intensity * 50, 2f, 40f); 

                // 平滑插值，不要跳得太生硬
                Vector2 targetSize = new Vector2(audioBars[i].sizeDelta.x, height);
                audioBars[i].sizeDelta = Vector2.Lerp(audioBars[i].sizeDelta, targetSize, Time.deltaTime * 15f);
            }
        }
    }

    public void PlayDialogue(DialogueData data)
    {
        if (dialogueCoroutine != null) StopCoroutine(dialogueCoroutine);
        dialogueCoroutine = StartCoroutine(RunDialogueRoutine(data));
    }

    IEnumerator RunDialogueRoutine(DialogueData data)
    {
        yield return StartCoroutine(AnimateEntry());

        foreach (var line in data.sentences)
        {
            ShowSingleLine(line); // 显示故障文字

            // 处理语音
            float waitTime = minDuration;
            if (line.voiceOver != null)
            {
                audioSource.Stop();
                audioSource.clip = line.voiceOver;
                audioSource.Play();
                waitTime = line.voiceOver.length;
                
                // 开启声波
                if(waveformContainer) waveformContainer.gameObject.SetActive(true);
            }
            else
            {
                // 没声音就关掉声波
                if(waveformContainer) waveformContainer.gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(waitTime + 0.5f);
        }

        // 关闭声波
        if(waveformContainer) waveformContainer.gameObject.SetActive(false);
        AnimateExit();
    }

    IEnumerator AnimateEntry()
    {
        islandRect.sizeDelta = idleSize;
        Tween entryTween = islandRect.DOAnchorPosY(onScreenY, 0.5f).SetEase(Ease.OutBack);
        yield return entryTween.WaitForCompletion();
    }

    void ShowSingleLine(DialogueData.Sentence line)
    {
        TextMeshProUGUI currentText = isUsingA ? textA : textB;
        TextMeshProUGUI nextText = isUsingA ? textB : textA;

        // --- 核心改动：故障文字效果 ---
        // 1. 先把文字设为空，透明度设为1 (因为 DOText 需要可见才能看到乱码变化)
        nextText.text = "";
        nextText.alpha = 1; 

        // 2. 拼接最终文字
        string fullContent = $"<color=#FFD700><b>{line.speakerName}:</b></color> {line.text}";

        // 3. 使用 DOTween 的 ScrambleMode 生成乱码解码效果
        // ScrambleMode.All = 随机字符; ScrambleMode.Numerals = 只有数字(适合机器人)
        DOTween.To(() => nextText.text, x => nextText.text = x, fullContent, 1.0f)
       .SetOptions(true, ScrambleMode.All);


        // --- 头像和岛的尺寸逻辑 (和以前一样) ---
        if (line.avatar != null)
        {
            avatarImage.sprite = line.avatar;
            avatarImage.DOFade(1, expandSpeed);
        }

        nextText.text = fullContent; // 先赋值算一下宽度
        nextText.ForceMeshUpdate();
        float finalWidth = Mathf.Clamp(baseWidth + nextText.preferredWidth + textPadding, idleSize.x, 2000f);
        nextText.text = ""; // 算完宽度再清空，等待动画播放

        Sequence seq = DOTween.Sequence();
        
        // 变宽
        seq.Join(islandRect.DOSizeDelta(new Vector2(finalWidth, idleSize.y), expandSpeed).SetEase(Ease.OutBack));

        // 旧文字处理 (稍微快点淡出)
        seq.Join(currentText.DOFade(0, 0.2f));

        // 新文字位置复位
        nextText.rectTransform.anchoredPosition = new Vector2(textLeftOffset, 0); // 故障风通常不位移，原地变化比较帅

        isUsingA = !isUsingA;
    }

    void AnimateExit()
    {
        Sequence seq = DOTween.Sequence();
        TextMeshProUGUI currentText = !isUsingA ? textA : textB;
        
        seq.Join(currentText.DOFade(0, 0.2f));
        seq.Join(avatarImage.DOFade(0, 0.2f));
        
        seq.AppendCallback(() => {
            textA.text = ""; textB.text = "";
            if(waveformContainer) waveformContainer.gameObject.SetActive(false);
        });

        seq.Append(islandRect.DOSizeDelta(idleSize, expandSpeed).SetEase(Ease.InBack));
        seq.Append(islandRect.DOAnchorPosY(offScreenY, 0.5f).SetEase(Ease.InBack));
    }
}