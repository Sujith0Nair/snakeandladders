using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Common
{
    public class LoadingScreen : MonoBehaviour
    {
        private const string LoadingScreenPrefabPath = "LoadingScreen";
        
        private static LoadingScreen _instance;

        [SerializeField] private Slider slider;

        private Func<bool> getPredicate;
        private Func<float> getProgress;
        private Action onCompleted;
        private float customWaitForSeconds;
        private Coroutine loadingRoutine;
        
        public static void ShowLoadingScreen(Func<bool> showTill, Func<float> progress, Action onDone, float customSecondsWait = 0)
        {
            if (!_instance)
            {
                _instance = CreateLoadingScreen();
            }
            _instance.Show(showTill, progress, onDone, customSecondsWait);
        }

        private static LoadingScreen CreateLoadingScreen()
        {
            var resObj = Resources.Load<LoadingScreen>(LoadingScreenPrefabPath);
            if (resObj == null)
            {
                Debug.LogError("LoadingScreen prefab is missing");
                return null;
            }
            var instance = Instantiate(resObj);
            return instance;
        }

        public static void HideLoadingScreen()
        {
            if (!_instance)
            {
                Debug.LogError("LoadingScreen was never displayed to hide!");
                return;
            }
            
            _instance.Hide();
        }

        private void Show(Func<bool> showTill, Func<float> progress, Action onDone, float customSecondsWait = 0)
        {
            getPredicate = showTill;
            getProgress = progress;
            customWaitForSeconds = customSecondsWait;
            onCompleted = onDone;
            slider.minValue = 0;
            slider.maxValue = 1;
            gameObject.SetActive(true);
            loadingRoutine = StartCoroutine(LoadingRoutine());
        }

        private void Hide()
        {
            if (loadingRoutine != null)
            {
                StopCoroutine(loadingRoutine);
            }

            loadingRoutine = null;
            gameObject.SetActive(false);
        }

        private IEnumerator LoadingRoutine()
        {
            while (getPredicate())
            {
                slider.value = getProgress();
                yield return null;
            }
            yield return new WaitForSeconds(customWaitForSeconds);
            loadingRoutine = null;
            onCompleted?.Invoke();
            Hide();
        }

        private void OnDestroy()
        {
            _instance = null;
        }
    }
}