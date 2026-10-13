using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace cowsins
{
    /// <summary>
    /// Service for full-screen color flash effects
    /// </summary>
    public class ScreenFlashService : MonoBehaviour
    {
        [Tooltip("Image overlay used for damage/heal/collect screen flashes"), SerializeField] private Image effectImage;

        [SerializeField] private float fadeOutTime = 4f;

        private Coroutine _fadeCoroutine;

        /// <summary>
        /// Triggers a screen flash with the given color
        /// </summary>
        public void Flash(Color color)
        {
            if (effectImage == null) return;

            effectImage.color = color;

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeOut());
        }

        private IEnumerator FadeOut()
        {
            if (effectImage == null) yield break;

            while (effectImage.color.a > 0)
            {
                effectImage.color -= new Color(0, 0, 0, Time.deltaTime * fadeOutTime);
                yield return null;
            }
            _fadeCoroutine = null;
        }
    }
}
