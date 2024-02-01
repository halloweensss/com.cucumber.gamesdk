using UnityEngine;

namespace GameSDK.Core
{
    public class GameAppRunner : MonoBehaviour
    {
        private void Awake()
        {
            GameApp.Instance.RegisterRunner(this);
        }
    }
}