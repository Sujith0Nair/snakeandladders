using UnityEngine;

namespace Board
{
    public class Ladder : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;

        private Color originalColor;

        public int From { get; private set; }
        public int To { get; private set; }

        public bool IsBlocked { get; private set; }

        public void Init(int from, int to)
        {
            From = from;
            To = to;

            originalColor = meshRenderer.material.color;
        }

        public bool IsIndexMatchingWithLadderStartPoint(int index)
        {
            return index == From;
        }

        public void BlockLadder()
        {
            IsBlocked = true;
            meshRenderer.material.color = Color.magenta;
        }

        public void UnblockLadder()
        {
            IsBlocked = false;
            meshRenderer.material.color = originalColor;
        }
    }
}