using UnityEngine;

public class DontDetroyComponent : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
