using UnityEngine;

namespace Board
{
    public class Ladder : MonoBehaviour
    {
        public int From { get; private set; }
        public int To { get; private set; }

        public void Init(int from, int to)
        {
            From = from;
            To = to;
        }

        public bool IsIndexMatchingWithLadderStartPoint(int index)
        {
            return index == From;
        }
    }
}