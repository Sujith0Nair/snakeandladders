using UnityEngine;

namespace Board
{
    public class Snake : MonoBehaviour
    {
        public int From { get; private set; }
        public int To { get; private set; }

        public void Init(int from, int to)
        {
            From = to;
            To = from;
        }
    }
}
