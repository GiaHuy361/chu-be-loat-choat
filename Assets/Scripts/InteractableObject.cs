using UnityEngine;
using UnityEngine.Events; // Quan tr?ng: ?? dùng UnityEvent

public class InteractableObject : MonoBehaviour
{
    [Header("Cài ??t ch? hi?n lên")]
    public string messageContent = "Nh?n E ?? t??ng tác";

    [Header("Hành ??ng khi nh?n E")]
    // Cho phép kéo th? hàm t? bên ngoài vào
    public UnityEvent OnInteract;

    private bool isPlayerInside = false;

    // Phát hi?n Player b??c vào
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            InteractionManager.Instance.ShowMessage(messageContent);
        }
    }

    // Phát hi?n Player b??c ra
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            InteractionManager.Instance.HideMessage();
        }
    }

    private void Update()
    {
        // N?u ?ang ??ng trong vùng và nh?n E
        if (isPlayerInside && Input.GetKeyDown(KeyCode.E))
        {
            // Th?c hi?n hành ??ng ?ã cài ??t (Load map, m? c?a, v.v.)
            OnInteract.Invoke();
        }
    }
}