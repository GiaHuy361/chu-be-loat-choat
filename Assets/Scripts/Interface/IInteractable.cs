using UnityEngine;

public interface IInteractable
{
    string GetInteractPrompt(); // Dòng chữ hiện lên: "Nhặt [E]"
    void OnInteract();          // Hành động khi bấm E
}