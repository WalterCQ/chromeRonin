using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "ChromeRonin/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [System.Serializable]
    public struct Sentence
    {
        public string speakerName;      // 名字 (如: Zane)
        [TextArea(3, 10)]
        public string text;             // 英文内容
        public AudioClip voiceOver;     // 配音
        public Sprite avatar;           // 头像
    }

    public List<Sentence> sentences;
}