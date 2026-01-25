using UnityEngine;

/// <summary>
/// Secret Document - Pickupable object
/// REFACTORED: Thêm Show/Hide để chỉ hiện khi mission active
/// </summary>
public class SecretDocument : MonoBehaviour
{
    [Header("Pickup Settings")]
    [Tooltip("Khoảng cách để pickup")]
    public float pickupDistance = 2f;
    
    [Tooltip("Phím để pickup")]
    public KeyCode pickupKey = KeyCode.E;

    [Header("Visual Effects")]
    [Tooltip("Tốc độ xoay (degrees/second)")]
    public float rotationSpeed = 50f;
    
    [Tooltip("Độ cao bobbing")]
    public float bobbingHeight = 0.3f;
    
    [Tooltip("Tốc độ bobbing")]
    public float bobbingSpeed = 2f;

    [Header("References")]
    [Tooltip("Light component (optional)")]
    public Light documentLight;

    private Transform player;
    private Vector3 startPosition;
    private bool isPlayerNear = false;
    private bool isVisible = true;
    private bool isPickedUp = false;
    private MeshRenderer meshRenderer;
    private Collider documentCollider;

    void Start()
    {
        startPosition = transform.position;

        // Tìm player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // Get components
        meshRenderer = GetComponent<MeshRenderer>();
        documentCollider = GetComponent<Collider>();

        // Setup collider if not exists
        if (documentCollider == null)
        {
            documentCollider = gameObject.AddComponent<SphereCollider>();
            ((SphereCollider)documentCollider).isTrigger = true;
            ((SphereCollider)documentCollider).radius = pickupDistance;
        }
    }

    void Update()
    {
        if (!isVisible || isPickedUp) return;

        // Visual effects
        UpdateVisualEffects();

        // Check player interaction
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            isPlayerNear = distance <= pickupDistance;

            if (isPlayerNear && Input.GetKeyDown(pickupKey))
            {
                PickUp();
            }
        }
    }

    void UpdateVisualEffects()
    {
        // Rotate
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Bob up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    #region Show/Hide

    /// <summary>
    /// Show document - Mission active
    /// </summary>
    public void Show()
    {
        isVisible = true;
        gameObject.SetActive(true);
        
        if (meshRenderer != null)
            meshRenderer.enabled = true;
        
        if (documentLight != null)
            documentLight.enabled = true;

        Debug.Log("[SecretDocument] Document shown");
    }

    /// <summary>
    /// Hide document - Mission inactive/complete
    /// </summary>
    public void Hide()
    {
        isVisible = false;
        gameObject.SetActive(false);
        
        if (meshRenderer != null)
            meshRenderer.enabled = false;
        
        if (documentLight != null)
            documentLight.enabled = false;

        Debug.Log("[SecretDocument] Document hidden");
    }

    #endregion

    #region Pickup

    void PickUp()
    {
        if (isPickedUp) return;

        // Check mission active
        var missionManager = StealthMissionManager.Instance;
        if (missionManager == null)
        {
            Debug.LogWarning("[SecretDocument] StealthMissionManager not found!");
            return;
        }

        if (missionManager.GetCurrentState() != StealthMissionManager.MissionState.Active)
        {
            Debug.Log("[SecretDocument] Cannot pickup - mission not active!");
            return;
        }

        Debug.Log("[SecretDocument] Document picked up!");
        isPickedUp = true;

        // Notify mission manager
        missionManager.OnDocumentPickedUp();

        // Hide document (player đã lấy rồi)
        Hide();
    }

    #endregion

    #region Reset

    /// <summary>
    /// Reset document về trạng thái ban đầu
    /// </summary>
    public void ResetDocument()
    {
        Debug.Log("[SecretDocument] Resetting document");
        
        transform.position = startPosition;
        isPickedUp = false;
        
        // Show lại nếu mission active
        var missionManager = StealthMissionManager.Instance;
        if (missionManager != null && 
            missionManager.GetCurrentState() == StealthMissionManager.MissionState.Active)
        {
            Show();
        }
    }

    #endregion

    #region Trigger

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            
            // TODO: Show pickup hint UI
            Debug.Log("[SecretDocument] Player near - Press E to pickup");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            
            // TODO: Hide pickup hint UI
        }
    }

    #endregion

    #region Debug

    void OnDrawGizmosSelected()
    {
        if (!isVisible) return;

        Gizmos.color = isPlayerNear ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupDistance);
    }

    #endregion
}
