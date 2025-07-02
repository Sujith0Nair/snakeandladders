using Data;
using UnityEngine;
using Unity.Services.Multiplayer;

namespace _Main
{
    public class World : MonoBehaviour
    {
        public static World Get;
        public DataBoard Board { get; private set; }
        public ISession ActiveSession { get; set; }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Initialize()
        {
            var _ = new GameObject("[WORLD]", typeof(World));
        }

        private void Awake()
        {
            Get = this;
            Board = new DataBoard();
            DontDestroyOnLoad(this);
        }
        
        private void OnDestroy()
        {
            DataBoard.ClearData();
            Get = null;
        }
    }
}