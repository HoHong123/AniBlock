using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundConnector : MonoBehaviour
{
    private Manager.Sound.SoundManager SM = null;
    
    // Start is called before the first frame update
    void Start()
    {
        SM = Manager.Sound.SoundManager.S;
    }

    public void Play_ButtonSound()
    {
        SM.Play_ButtonSound();
    }

    public void Play_UISound(int _num)
    {
        SM.Play_Sound((Manager.Sound.SoundManager.UI_CLIP_LIST)_num);
    }

    public void Play_MiniSound(int _num)
    {
        SM.Play_Sound((Manager.Sound.SoundManager.MINIGAME_CLIP_LIST)_num);
    }

    public void Play_Music(int _num)
    {
        SM.Play_Music((Manager.Sound.SoundManager.BGM_CLIP_LIST)_num);
    }

    public void Stop_BGM()
    {
        SM.Stop_BGM();
    }
}
