using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Utilities.Animation
{
    public class SpriteAnimation : MonoBehaviour
    {
        [SerializeField]
        private string nowPlaying;

        [SerializeField]
        private List<SpriteChunk> animationList;

        [SerializeField]
        private Image targetImage;

        private string beforePlaying = "beforePlaying";
        private string tempPlaying = "tempPlaying";
        private SpriteChunk nowPlay;

        private float time = 0f;
        private float delayTime = 0f;
        private int frame = 0;
        private bool loop = false;

        void Start()
        {
            targetImage = GetComponent<Image>();
        }

        void Update()
        {
            if (tempPlaying == nowPlaying)
            {
                time += Time.deltaTime;

                if (time >= nowPlay.DelayTime)
                {
                    Sprite sprite = nowPlay.GetAnimationFrame(frame);

                    if (sprite == null)
                    {
                        if (loop.Equals(false))
                        {
                            return;
                        }
                        else
                            frame = 0;
                    }
                    else
                    {
                        frame++;
                        targetImage.sprite = sprite;
                    }

                    Reset();
                }
            }
            else
            {
                for (int i = 0; i < animationList.Count; i++)
                {
                    if (animationList[i].AnimationName == nowPlaying)
                    {
                        if (nowPlay != null) nowPlay.IsPlaying = false;
                        nowPlay = animationList[i];
                        nowPlay.IsPlaying = true;
                        Reset();
                        tempPlaying = nowPlaying;
                        break;
                    }
                }
            }
        }

        public void Reset()
        {
            time = 0f;
            delayTime = nowPlay.DelayTime;
            loop = nowPlay.Loop;
        }

        /// <summary>
        /// 현재 플레이 리스트를 바꿉니다.
        /// </summary>
        /// <param name="name"></param>
        public void ChangePlaying(string name)
        {
            beforePlaying = nowPlaying;
            nowPlaying = name;
        }

        /// <summary>
        /// 전체 애니메이션을 바꿉니다.
        /// </summary>
        /// <param name="chunk"></param>
        public void ChangeAnimation(List<SpriteChunk> chunk)
        {
            animationList = chunk;
        }
    }
}