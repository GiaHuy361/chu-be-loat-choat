using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Player")]
    public GameObject playerPrefab; // Kéo prefab NV Kim ??ng vào ?ây

    [Header("Spawn")]
    public Transform spawnPoint; // Kéo object v? trí spawn trong hang vào ?ây

    private void Start()
    {
        if (spawnPoint == null)
        {
            Debug.LogError("?? PlayerSpawner: spawnPoint ch?a ???c gán!");
            return;
        }

        // Tìm xem NV Kim ??ng ?ã t?n t?i t? Scene tr??c ch?a (do PlayerRoot gi? l?i)
        // ??m b?o Player c?a b?n có Tag là "Player"
        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");

        if (existingPlayer != null)
        {
            // T?t CharacterController (n?u có) tr??c khi teleport ?? tránh l?i v?t lý
            CharacterController cc = existingPlayer.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // Di chuy?n nhân v?t c? ??n v? trí m?i
            existingPlayer.transform.position = spawnPoint.position;
            existingPlayer.transform.rotation = spawnPoint.rotation;

            Debug.Log("? PlayerSpawner: ?ã di chuy?n Player c? t?i SpawnPoint");

            if (cc != null) cc.enabled = true; // B?t l?i sau khi di chuy?n
        }
        else
        {
            // N?u test tr?c ti?p scene InCave mà không qua Demo, thì t?o m?i
            if (playerPrefab != null)
            {
                Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
                Debug.Log("? PlayerSpawner: Spawn Player m?i (Test mode)");
            }
            else
            {
                Debug.LogError("?? PlayerSpawner: Không tìm th?y Player c? và ch?a gán Prefab m?i!");
            }
        }
    }
}