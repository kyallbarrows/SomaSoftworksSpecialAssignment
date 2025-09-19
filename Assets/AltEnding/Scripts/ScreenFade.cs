using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace AltEnding
{
    public enum TransitionStatus
    {
        Starting = 0,
        Hold = 1,
        Ending = 2,
        Complete = 3
    }

    public class ScreenFade : PersistentSingleton<ScreenFade>
    {
        //Protected constructor to prevent creation of any additional instances
        protected ScreenFade() { }

        public delegate void FadeEvent(TransitionStatus status);

        public static event FadeEvent fadeEvent;

        public Image myFadeImage;

        [SerializeField, Min(0.001f)] protected float maxFadeAlphaDelta = 0.1f;

        private Color m_Color = Color.black;
        private TransitionStatus currentFadeSatus;

        Coroutine fadeCoroutine;

        public TransitionStatus CurrentFadeStatus
        {
            get { return currentFadeSatus; }
        }

        public bool Fading
        {
            get { return currentFadeSatus != TransitionStatus.Complete; }
        }

        public bool FadeFirstHalf
        {
            get
            {
                return currentFadeSatus == TransitionStatus.Starting ||
                       instance.currentFadeSatus == TransitionStatus.Hold;
            }
        }

        public bool FadeSecondHalf
        {
            get
            {
                return currentFadeSatus == TransitionStatus.Ending ||
                       instance.currentFadeSatus == TransitionStatus.Complete;
            }
        }

        public Color FadeColor
        {
            get { return m_Color; }
        }

        protected override void Awake()
        {
            base.Awake();
            currentFadeSatus = TransitionStatus.Complete;
        }

        #region Coroutines

        private IEnumerator FullFadeCoroutine(float aFadeOutTime, float aFadeInDelay, float aFadeInTime, Color aColor)
        {
            yield return StartFadeCoroutine(aFadeOutTime, aColor, false);
            yield return new WaitForSeconds(aFadeInDelay);
            yield return EndFadeCoroutine(aFadeInTime, false);
        }

        private IEnumerator StartFadeCoroutine(float aFadeOutTime, Color aColor,
            bool nullCoroutineReferenceWhenDone = true)
        {
            if (myFadeImage != null)
            {
                m_Color = aColor;
                m_Color.a = 0;
                myFadeImage.color = m_Color;

                //Fade out (image fades in)
                currentFadeSatus = TransitionStatus.Starting;
                if (fadeEvent != null) fadeEvent(TransitionStatus.Starting);

                if (aFadeOutTime > 0)
                {
                    float alphaDelta;
                    while (m_Color.a < 1.0f)
                    {
                        yield return new WaitForEndOfFrame();
                        alphaDelta = Mathf.Min(Time.unscaledDeltaTime / aFadeOutTime, maxFadeAlphaDelta);
                        m_Color.a = Mathf.Clamp01(m_Color.a + alphaDelta);
                        myFadeImage.color = m_Color;
                    }
                }

                m_Color.a = 1.0f;
                myFadeImage.color = m_Color;

                //Fade out finished (image is opaque)
                currentFadeSatus = TransitionStatus.Hold;
                if (fadeEvent != null) fadeEvent(TransitionStatus.Hold);
            }

            if (nullCoroutineReferenceWhenDone) fadeCoroutine = null;
        }

        private IEnumerator EndFadeCoroutine(float aFadeInTime, bool nullCoroutineReferenceWhenDone = true)
        {
            if (myFadeImage != null)
            {
                //Fade in (image fades out)
                currentFadeSatus = TransitionStatus.Ending;
                if (fadeEvent != null) fadeEvent(TransitionStatus.Ending);
                if (aFadeInTime > 0)
                {
                    float alphaDelta;
                    while (m_Color.a > 0.0f)
                    {
                        yield return new WaitForEndOfFrame();
                        alphaDelta = Mathf.Min(Time.unscaledDeltaTime / aFadeInTime, maxFadeAlphaDelta);
                        m_Color.a = Mathf.Clamp01(m_Color.a - alphaDelta);
                        myFadeImage.color = m_Color;
                    }
                }

                m_Color.a = 0.0f;
                myFadeImage.color = m_Color;

                //Fade in finished (image is transparent)
                currentFadeSatus = TransitionStatus.Complete;
                if (fadeEvent != null) fadeEvent(TransitionStatus.Complete);
            }

            currentFadeSatus = TransitionStatus.Complete;
            if (nullCoroutineReferenceWhenDone) fadeCoroutine = null;
        }

        #endregion


        #region Trigger Calls

        public void FullFade(float aFadeOutTime, float aFadeInDelay, float aFadeInTime, Color aColor,
            bool force = false)
        {
            if (force && fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            if (fadeCoroutine == null)
                fadeCoroutine = StartCoroutine(FullFadeCoroutine(aFadeOutTime, aFadeInDelay, aFadeInTime, aColor));
        }

        public void StartFade(float aFadeOutTime, bool force = false) => StartFade(aFadeOutTime, Color.black, force);

        public void StartFade(float aFadeOutTime, Color aColor, bool force = false)
        {
            if (force && fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            if (fadeCoroutine == null)
                fadeCoroutine = StartCoroutine(StartFadeCoroutine(aFadeOutTime, aColor));
        }

        public void EndFade(float aFadeInTime, bool force = false)
        {
            if (force && fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            if (fadeCoroutine == null)
                fadeCoroutine = StartCoroutine(EndFadeCoroutine(aFadeInTime));
        }

        public Coroutine StartAndReturnFade(float aFadeOutTime, Color aColor, bool force = false)
        {
            if (force && fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            if (fadeCoroutine == null)
            {
                fadeCoroutine = StartCoroutine(StartFadeCoroutine(aFadeOutTime, aColor));
                return fadeCoroutine;
            }
            else return null;
        }

        public Coroutine EndAndReturnFade(float aFadeInTime, bool force = false)
        {
            if (force && fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            if (fadeCoroutine == null)
            {
                fadeCoroutine = StartCoroutine(EndFadeCoroutine(aFadeInTime));
                return fadeCoroutine;
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}