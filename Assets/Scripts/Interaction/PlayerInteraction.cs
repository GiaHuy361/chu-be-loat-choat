using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Cài đặt tương tác TPP")]
    public float interactRadius = 2f;
    public Transform interactionCenter;

    private IInteractable currentInteractable;

    void Update()
    {
        CheckForInteractable();

        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable.OnInteract();
        }
    }

    void CheckForInteractable()
    {
        Vector3 center = interactionCenter != null ? interactionCenter.position : transform.position + Vector3.up * 1f;
        Collider[] hitColliders = Physics.OverlapSphere(center, interactRadius);

        IInteractable closestInteractable = null;
        float closestDistance = float.MaxValue;

        foreach (var hitCollider in hitColliders)
        {
            // Bỏ qua vật thể đã bị ẩn
            if (!hitCollider.gameObject.activeInHierarchy) continue;

            IInteractable interactable = hitCollider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                float distance = Vector3.Distance(center, hitCollider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }

        if (closestInteractable != null)
        {
            currentInteractable = closestInteractable;

            if (UIManager.Instance != null)
            {
                string promptMsg = currentInteractable.GetInteractPrompt();

                // NẾU CHỮ RỖNG -> TỰ ĐỘNG ẨN PANEL
                if (string.IsNullOrEmpty(promptMsg))
                {
                    UIManager.Instance.HideInteractPrompt();
                }
                else
                {
                    UIManager.Instance.ShowInteractPrompt(promptMsg);
                }
            }
        }
        else
        {
            currentInteractable = null;
            if (UIManager.Instance != null)
            {
                UIManager.Instance.HideInteractPrompt();
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = interactionCenter != null ? interactionCenter.position : transform.position + Vector3.up * 1f;
        Gizmos.DrawWireSphere(center, interactRadius);
    }
}