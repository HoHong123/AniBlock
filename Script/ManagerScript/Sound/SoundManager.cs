using UnityEngine;
using Manager.Util;

namespace Manager.Sound
{
    public class SoundManager : MonoBehaviour
    {

        public static SoundManager S = null;

        public enum UI_CLIP_LIST
        {
            P_Button,

            P_RESCUE_BlockScan,
            P_RESCUE_ScanSuccess,
            P_RESCUE_ScanFail,

            P_RECOG_MarkerScan,

            Close
        }
        public enum MINIGAME_CLIP_LIST
        {
            STAGE_CLEAR,

            M_CLEAR_Success,
            M_CLEAR_Fail,

            M_Timer,

            M_LIVE_Stun,

            M_CLASSIFICATION,

            M_ARITHMETIC_Chalk,

            M_CARDGAME_CardFlip,

            M_TRICKSTER_Cup,

            M_SHOOTER_PowerUp,

            M_LIVE_GuideUp,
            M_LIVE_GuideDown,
            M_LIVE_Jump,
            M_LIVE_Ceremony,
            M_LIVE_Correct,
            M_LIVE_Wrong,

            Close
        }
        public enum BGM_CLIP_LIST
        {
            TITLE,
            MAIN,
            PUZZLE,
            LIVE_GAME,
            MINIGAME,
            WORLD,
            TUTORIAL,

            PPR_TITLE,
            PPR_MAIN,
            PPR_MINIGAME,
            PPR_WORLD,

            Close
        }


        // 이펙트 세팅
        public bool Get_IsSoundEffectON()
        {
            if (PlayerPrefs.HasKey("SOUND_EFFECT"))
            {
                return (PlayerPrefs.GetInt("SOUND_EFFECT") == 1) ? true : false;
            }

            PlayerPrefs.SetInt("SOUND_EFFECT", 1);
            return true;
        }
        public bool Set_IsEffect()
        {
            if (Get_IsSoundEffectON())
            {
                PlayerPrefs.SetInt("SOUND_EFFECT", 0);
                myEffect.Stop();
                return myEffect.enabled = false;
            }
            else
            {
                PlayerPrefs.SetInt("SOUND_EFFECT", 1);
                return myEffect.enabled = true;
            }
        }

        // BGM 세팅
        public bool Get_IsMusicON()
        {
            if (PlayerPrefs.HasKey("SOUND_MUSIC"))
            {
                return (PlayerPrefs.GetInt("SOUND_MUSIC") == 1) ? true : false;
            }

            PlayerPrefs.SetInt("SOUND_MUSIC", 1);
            return true;
        }
        public bool Set_IsMusic()
        {
            // 현재 볼륨이 1이면
            if (Get_IsMusicON())
            {
                PlayerPrefs.SetInt("SOUND_MUSIC", 0); // 볼륨 0으로
                myBGM.Stop(); // 음악 제거
                return myBGM.enabled = false; // 컴포넌트 비활성화
            }
            else // 볼륨이 0이면
            {
                PlayerPrefs.SetInt("SOUND_MUSIC", 1); // 볼륨 1로
                myBGM.enabled = true; // 컴포넌트 활성화
                myBGM.Play(); // 음악 재생
                return true; // 컴포넌트 활성화
            }
        }

        [SerializeField] private AudioSource myEffect = null;
        [SerializeField] private AudioSource myBGM = null;

        [Header("====Sound Effect====")]
        [SerializeField] private AudioClip[] myUIAudioClips = new AudioClip[(int)UI_CLIP_LIST.Close];
        [SerializeField] private AudioClip[] myMinigameAudioClips = new AudioClip[(int)MINIGAME_CLIP_LIST.Close];
        [SerializeField] private AudioClip[] myBGMAudioClips = new AudioClip[(int)BGM_CLIP_LIST.Close];
        [SerializeField] private AudioClip[] myCharacterAudioClips = new AudioClip[(int)PackageManager.FirstCollection.NumOf]; // FirstCollection 순서대로 입력


        private void Awake()
        {
            if (S != null)
            {
                Destroy(this.gameObject);
                return;
            }

            S = this;
            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            myBGM.loop = true;
            myEffect.loop = false;

            myBGM.enabled = Get_IsMusicON();
            myEffect.enabled = Get_IsSoundEffectON();
        }

        public void Play_ButtonSound()
        {
            if (!myEffect.enabled) return;
            myEffect.PlayOneShot(myUIAudioClips[0]);
        }

        public void Play_Sound(UI_CLIP_LIST _num)
        {
            if (!myEffect.enabled) return;
            myEffect.PlayOneShot(myUIAudioClips[(int)_num]);
        }

        public void Play_Sound(MINIGAME_CLIP_LIST _num)
        {
            if (!myEffect.enabled) return;
            myEffect.PlayOneShot(myMinigameAudioClips[(int)_num]);
        }

        public void Play_Music(BGM_CLIP_LIST _num)
        {
            if (!myBGM.enabled) return;
            myBGM.Stop();
            myBGM.clip = myBGMAudioClips[(int)_num];
            myBGM.PlayDelayed(1.0f);
        }

        public void Play_CharacterEffect(PackageManager.FirstCollection _num)
        {
            if (!myEffect.enabled) return;
            myEffect.PlayOneShot(myCharacterAudioClips[(int)_num]);
        }

        public void Stop_BGM()
        {
            if (!myBGM.enabled) return;
            myBGM.Pause();
        }
    }

}
