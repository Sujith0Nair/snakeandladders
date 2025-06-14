using System;
using UnityEngine;

namespace _Main
{
    public abstract class Context : MonoBehaviour
    {
        public int StateIndex { get; set; }
        public abstract void Enter(Action onMoveBack, Action onMoveForward);
        public abstract void Exit();

        public void Awake()
        {
            gameObject.name = gameObject.name.Replace("(Clone)", "");
        }
    }
}