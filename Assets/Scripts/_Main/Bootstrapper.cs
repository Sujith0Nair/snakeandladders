using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Main
{
    public class Bootstrapper : MonoBehaviour
    {
        [SerializeField] private ContextHolderSo contextHolder;
        [SerializeField] private GameObject[] objectsToHideOnSceneSwitch;
        
        private Context currentContext;

        private void Start()
        {
            currentContext = contextHolder.GetDefaultContext();
            currentContext.Enter(MoveBack, MoveForward);
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDisable()
        {
            contextHolder.Dispose();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void OnSceneUnloaded(Scene _)
        {
            SetSceneObjectsState(true);
        }

        private void OnSceneLoaded(Scene _, LoadSceneMode __)
        {
            SetSceneObjectsState(false);
        }

        private void SetSceneObjectsState(bool state)
        {
            state &= SceneManager.sceneCount == 0;
            foreach (var obj in objectsToHideOnSceneSwitch)
            {
                obj.SetActive(state);
            }
        }

        private void MoveBack()
        {
            EnterNewState(false);
        }

        private void MoveForward()
        {
            EnterNewState(true);
        }

        private void EnterNewState(bool toForward)
        {
            currentContext.Exit();
            currentContext = toForward ? contextHolder.GetNextContext(currentContext) : contextHolder.GetPreviousContext(currentContext);
            if (!currentContext)
            {
                throw new IndexOutOfRangeException("Cannot enter new state, since the requested state changes does not exist");
            }
            currentContext.Enter(MoveBack, MoveForward);
        }
    }
}