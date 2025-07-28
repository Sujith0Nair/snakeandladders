using System.Collections;
using UnityEngine;

namespace SaL.Player
{
    /// <summary>
    /// Manages the visual representation of a player on the board. This script handles
    /// the smooth movement between cells and is controlled by its corresponding Player data object.
    /// </summary>
    public class PlayerPawn : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 5f;
        
        private Coroutine _moveCoroutine;

        /// <summary>
        /// Starts the process of moving the pawn to a new target cell.
        /// </summary>
        /// <param name="targetPosition">The world-space position of the target cell.</param>
        public void MoveToCell(Vector3 targetPosition)
        {
            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
            }
            _moveCoroutine = StartCoroutine(MoveCoroutine(targetPosition));
        }

        private IEnumerator MoveCoroutine(Vector3 targetPosition)
        {
            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, _moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = targetPosition; // Snap to final position
            _moveCoroutine = null;
        }
    }
}
