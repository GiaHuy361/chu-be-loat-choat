using UnityEngine;

public class PlayerRoot : MonoBehaviour
{
    public static PlayerRoot Instance;

    private void Awake()
    {
        // Singleton Pattern: ??m b?o ch? có 1 Player t?n t?i
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // H?y nhân v?t m?i n?u ?ã có nhân v?t c?
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Gi? nhân v?t này khi chuy?n Scene
    }
}