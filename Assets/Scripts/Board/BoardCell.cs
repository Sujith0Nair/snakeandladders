using TMPro;
using UnityEngine;

namespace Board
{
    public class BoardCell : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        
        public int CellIndex { get; private set; }
        
        public void Init(int cellIndex)
        {
            CellIndex = cellIndex;
            label.text = $"{cellIndex}";
        }
    }
}