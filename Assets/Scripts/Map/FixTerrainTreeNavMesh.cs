using UnityEngine;

public class FixTerrainTreeNavMesh : MonoBehaviour
{
    [Header("Kích thước gốc cây ảo")]
    public float radius = 0.5f; // Chỉnh cho to hơn gốc cây một chút để lính không kẹt vai
    public float height = 3f;

    [ContextMenu("1. Tạo Gốc Cây Ảo Để Bake")]
    public void CreateTreeObstacles()
    {
        Terrain terrain = GetComponent<Terrain>();
        if (terrain == null || terrain.terrainData == null) return;

        // Xóa bản cũ nếu có để tránh trùng lặp
        Transform oldGroup = transform.Find("Tree_Obstacles_For_NavMesh");
        if (oldGroup != null) DestroyImmediate(oldGroup.gameObject);

        // Tạo thư mục rỗng chứa các gốc cây ảo
        GameObject parentObj = new GameObject("Tree_Obstacles_For_NavMesh");
        parentObj.transform.SetParent(transform);

        TerrainData data = terrain.terrainData;
        foreach (TreeInstance tree in data.treeInstances)
        {
            // Tính toán tọa độ thực tế của cây trên bản đồ
            Vector3 worldPos = Vector3.Scale(tree.position, data.size) + terrain.transform.position;

            // Tạo một vật thể tàng hình
            GameObject obstacle = new GameObject("TreeCollider");
            obstacle.transform.position = worldPos;
            obstacle.transform.SetParent(parentObj.transform);

            // Gắn Capsule Collider để NavMesh "nhìn thấy" và né ra
            CapsuleCollider col = obstacle.AddComponent<CapsuleCollider>();
            col.radius = radius;
            col.height = height;
        }

        Debug.Log($"✅ Đã tạo thành công {data.treeInstances.Length} gốc cây ảo! Bạn hãy qua NavMesh Surface bấm Bake lại nhé.");
    }

    [ContextMenu("2. Xóa Gốc Cây Ảo (Sau khi Bake xong)")]
    public void DeleteTreeObstacles()
    {
        Transform oldGroup = transform.Find("Tree_Obstacles_For_NavMesh");
        if (oldGroup != null) DestroyImmediate(oldGroup.gameObject);
        Debug.Log("🗑️ Đã dọn dẹp xong gốc cây ảo cho nhẹ máy!");
    }
}