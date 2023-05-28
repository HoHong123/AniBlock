using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CutScene;
using Manager.Sound;

namespace CutScene.SceneManager
{
    public class CUT_PanToolScroll : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private RectTransform RECT_Panel;
        [SerializeField] private Button BTN_PanScroll = null;
        private bool b_isOpen = false;

        // Start is called before the first frame update
        void Start()
        {
            BTN_PanScroll = GetComponent<Button>();
        }

        #region IPointerClickHandler implementation

        public void OnPointerClick(PointerEventData eventData)
        {
            if (b_isOpen) RECT_Panel.anchoredPosition = new Vector3(0, 0, 0);
            else RECT_Panel.anchoredPosition = new Vector3(-280, 0, 0);

            b_isOpen = !b_isOpen;

            SoundManager.S.Play_ButtonSound();
        }

        #endregion

    }
}
