using UnityEngine;
using Unity.Netcode;
using System.Collections;

namespace Networking
{
    public class NetTestPlayer : NetworkBehaviour
    {
        private Coroutine keyBindingCoroutine;
        
        public override void OnNetworkDespawn()
        {
            if (keyBindingCoroutine != null)
            {
                StopCoroutine(keyBindingCoroutine);
            }
        }
        
        public override void OnNetworkSpawn()
        {
            keyBindingCoroutine = StartCoroutine(KeyBinding());
        }

        private IEnumerator KeyBinding()
        {
            while (IsOwner)
            {
                MovePlayerBasedOnKeyInputs();
                yield return null;
            }
            keyBindingCoroutine = null;
        }

        private void MovePlayerBasedOnKeyInputs()
        {
            if (Input.GetKey(KeyCode.W))
            {
                transform.position += Vector3.up;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                transform.position += Vector3.down;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                transform.position += Vector3.left;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                transform.position += Vector3.right;
            }
        }
    }
}