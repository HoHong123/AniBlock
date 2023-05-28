using UnityEngine;
using CutScene;

namespace CutScene.AnimationController
{
    public class CUT_AnimationHandler : MonoBehaviour
    {
        [Header("Drawing Canvas")]
        [SerializeField] PaintCraft.Controllers.CanvasController SCRIPT_CanvasController;

        [Header("Animator")]
        [SerializeField] Animator ANI_Cup;

        [Header("Bodies")]
        [SerializeField] MeshRenderer[] MR_Body = new MeshRenderer[0];

        [Header("Faces")]
        [SerializeField] MeshRenderer[] MR_Face = new MeshRenderer[0];

        [Header("SceneHandler")]
        [SerializeField] CutScene.SceneManager.CUT_SceneHandler SceneHandler;

        private void Start()
        {

        }
        
        public void StopAnimation()
        {
            ANI_Cup.enabled = false;
        }
        // 애니메이션 실행 취소
        public void Set_PlayAnimation()
        {
            ANI_Cup.enabled = !ANI_Cup.enabled;
        }

        // 캐릭터 몸통 텍스쳐 변경
        public void Set_BodyTexture(int _selectedBody)
        {
            MR_Body[_selectedBody].material.mainTexture = SCRIPT_CanvasController.BackLayerController.RenderTexture;
        }

        // 캐릭터 표정 텍스쳐 변형
        public void FacesAnimationEventHandler(AnimationEvent _animationEvent)
        {
            MR_Face[_animationEvent.intParameter].material.mainTexture = (Texture)_animationEvent.objectReferenceParameter;
        }

        public void SendEndSignal()
        {
            SceneHandler.BTN_ActivatePhoto();
        }
    }

}