<<<<<<< HEAD
using UnityEngine;

public class LevelMusicSetter : MonoBehaviour
{
    public AudioClip thisLevelMusic;

    private void Start()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMusic(thisLevelMusic);
        }
    }
=======
using UnityEngine;

public class LevelMusicSetter : MonoBehaviour
{
    public AudioClip thisLevelMusic;

    private void Start()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMusic(thisLevelMusic);
        }
    }
>>>>>>> upstream/dev
}