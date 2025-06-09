using UnityEngine;

public class DisableDebug : MonoBehaviour
{
    void Start()
    {
#if !UNITY_EDITOR
        Debug.unityLogger.logEnabled = false;
#endif
    }
}
