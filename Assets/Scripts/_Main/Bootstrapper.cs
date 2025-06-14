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
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            contextHolder.Dispose();
        }

        private void OnActiveSceneChanged(Scene current, Scene next)
        {
            var status = next == gameObject.scene;
            foreach (var obj in objectsToHideOnSceneSwitch)
            {
                obj.SetActive(status);
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