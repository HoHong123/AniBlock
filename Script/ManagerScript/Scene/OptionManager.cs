using UnityEngine;
using UnityEngine.UI;

namespace Manager.Util
{
    public class OptionManager : MonoBehaviour
    {
        [Header("====OPTIONS====")]
        public Image BTN_Speaker = null; // Sound effect
        public Image BTN_Music = null; // Sound music
        public Image BTN_Algorithm = null; // Algorithm

        [Header("====리소스====")]
        public Sprite SPT_BGMOn     = null;
        public Sprite SPT_BGMOff    = null;
        public Sprite SPT_SoundOn   = null;
        public Sprite SPT_SoundOff  = null;
        public Sprite SPT_AlgoOn     = null;
        public Sprite SPT_AlgoOff    = null;


        //LANG KEY
        public const string USER_LANG_KEY = "USER_LANGUAGE";
        public const string USER_LANG_KOR = "korean";
        public const string USER_LANG_JPN = "Japanese";
        public const string USER_LANG_ESP = "Spanish (Mexico)";
        public const string USER_LANG_USA = "English";
        public const string USER_LANG_RUS = "Russian";
        public const string USER_LANG_DEU = "German";
        public const string USER_LANG_FRA = "French";
        public const string USER_LANG_NLD = "Dutch (Netherlands)";
        public const string USER_LANG_CHS = "Chinese (Simplified)";
        public const string USER_LANG_CHT = "Chinese (Traditional)";
        public const string USER_LANG_POL = "Polish";
        public const string USER_LANG_ITA = "Italian";


        private void Start()
        {
            string _Language = I2.Loc.LocalizationManager.CurrentLanguage;

            SystemLanguage sl = Application.systemLanguage;

            if (!PlayerPrefs.HasKey("W_Algorithm")) PlayerPrefs.SetInt("W_Algorithm", 1);
            BTN_Algorithm.sprite = (PlayerPrefs.GetInt("W_Algorithm") == 0) ? SPT_AlgoOn : SPT_AlgoOff;

            BTN_Speaker.sprite = (Manager.Sound.SoundManager.S.Get_IsSoundEffectON()) ? SPT_SoundOn : SPT_SoundOff;
            BTN_Music.sprite = (Manager.Sound.SoundManager.S.Get_IsMusicON()) ? SPT_BGMOn : SPT_BGMOff; 
        }

        public void SetVolum(int _num)
        {
            if (_num == 0)
            {
                BTN_Speaker.sprite = (Manager.Sound.SoundManager.S.Set_IsEffect()) ? SPT_SoundOn : SPT_SoundOff;
            }
            else if (_num == 1)
            {
                BTN_Music.sprite = (Manager.Sound.SoundManager.S.Set_IsMusic()) ? SPT_BGMOn : SPT_BGMOff;
            }
        }

        public void SetAlgorithm()
        {
            if (PlayerPrefs.GetInt("W_Algorithm") == 0)
            {
                PlayerPrefs.SetInt("W_Algorithm", 1);
                BTN_Algorithm.sprite = SPT_AlgoOff;
            }
            else
            {
                PlayerPrefs.SetInt("W_Algorithm", 0);
                BTN_Algorithm.sprite = SPT_AlgoOn;
            }
        }
    }
}
