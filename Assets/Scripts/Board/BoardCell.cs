using UnityEngine;

namespace Board
{
    public class BoardCell : MonoBehaviour
    {
        public int CellIndex { get; private set; }
        
        public void Init(int cellIndex)
        {
            CellIndex = cellIndex;
        }
    }
}