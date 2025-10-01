using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    private static MusicPlayer instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // persiste tra le scene
        }
        else
        {
            Destroy(gameObject); // evita duplicati
        }
    }
}
