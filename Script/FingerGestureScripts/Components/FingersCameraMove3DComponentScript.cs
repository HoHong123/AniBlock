//
// Fingers Gestures
// (c) 2015 Digital Ruby, LLC
// http://www.digitalruby.com
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace DigitalRubyShared
{
    /// <summary>
    /// Allows moving a camera in 3D space using pan, tilt, rotate and zoom mechanics
    /// </summary>
    [AddComponentMenu("Fingers Gestures/Component/Fingers Camera Move 3D", 4)]
    public class FingersCameraMove3DComponentScript : MonoBehaviour
    {
        /// <summary>The transform to move, defaults to the transform on this script</summary>
        [Tooltip("The transform to move, defaults to the transform on this script")]
        public Transform Target;

        /// <summary>Controls pan (left/right for strafe, up/down for forward/back) speed in number of world units per screen units panned</summary>
        [Range(-10.0f, 10.0f)]
        [Tooltip("Controls pan (left/right for strafe, up/down for forward/back) speed in number of world units per screen units panned")]
        public float PanSpeed = -1.0f;

        /// <summary>How much to dampen movement, lower values dampen faster</summary>
        [Range(0.0f, 1.0f)]
        [Tooltip("How much to dampen movement, lower values dampen faster")]
        public float Dampening = 0.95f;

        /// <summary>
        /// The pan gesture (left/right)
        /// </summary>
        public PanGestureRecognizer PanGesture { get; private set; }
        private Vector3 moveVelocity;

        private void PanGestureCallback(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                Quaternion q = Target.rotation;
                q = Quaternion.Euler(0.0f, q.eulerAngles.y, 0.0f);
                moveVelocity += (q * Vector3.right * DeviceInfo.PixelsToUnits(gesture.DeltaX) * Time.deltaTime * PanSpeed * 500.0f);
                moveVelocity += (q * Vector3.forward * DeviceInfo.PixelsToUnits(gesture.DeltaY) * Time.deltaTime * PanSpeed * 500.0f);
            }
        }

        private void OnEnable()
        {
            if (Target == null)
            {
                Target = transform;
            }

            PanGesture = new PanGestureRecognizer();
            PanGesture.StateUpdated += PanGestureCallback;
            FingersScript.Instance.AddGesture(PanGesture);
        }

        private void OnDisable()
        {
            if (FingersScript.HasInstance)
            {
                FingersScript.Instance.RemoveGesture(PanGesture);
            }
        }

        [SerializeField] private LayerMask MASK_CharacterLayer;
        private bool b_isWorld = true;
        private int ease = 1;
        public void Set_Ease() { ease = 0; }
        public bool Get_IsWorld() { return b_isWorld; }
        public void Set_IsWorld(bool _isWorld) { b_isWorld = _isWorld; }
        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit = new RaycastHit();
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red);

                if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, MASK_CharacterLayer))
                {
                    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 1.0f);
                    switch (SceneManager.GetActiveScene().name)
                    {
                        case "Scene_World_First":
                            hit.collider.GetComponent<WORLD_CharacterSoundController>().OnHit();
                            break;
                        case "Scene_World_PRR":
                            hit.collider.GetComponent<World_CharacterSoundController_PRR>().OnHit();
                            break;
                    }
                }
            }
#elif UNITY_ANDROID || UNITY_IOS
            if (Input.touchCount > 0)
            {
                UnityEngine.Touch touch = Input.GetTouch(0);

                try { Debug.Log("Touch Began Select : " + touch.phase.Equals(UnityEngine.TouchPhase.Began)); }
                catch (System.Exception e) { }

                if (touch.phase == UnityEngine.TouchPhase.Began)
                {
                    Debug.Log("IS CASTING");
                    RaycastHit hit = new RaycastHit();
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    //Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red);

                    if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, MASK_CharacterLayer))
                    {
                        //Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 1.0f);
                        hit.collider.GetComponent<WORLD_CharacterSoundController>().OnHit();
                    }
                }
            }
#endif


            if (b_isWorld)
            {
                Target.Translate(moveVelocity * (Camera.main.fieldOfView * 0.01667f) * ease, Space.World);
                Target.position = new Vector3(Mathf.Clamp(Target.position.x, 10f, 70f), Target.position.y, Mathf.Clamp(Target.position.z, 10f, 85f));

                moveVelocity *= Dampening * ease;

                if (ease == 0) ease = 1;
            }
        }
    }
}