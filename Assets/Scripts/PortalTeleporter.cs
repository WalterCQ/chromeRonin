using UnityEngine;

public class ScreenWrapTeleport : MonoBehaviour
{
    [Header("设置")]
    [Tooltip("传送到上方的高度 (Y轴坐标)")]
    public float topSpawnY = 10f; // 你需要在编辑器里根据你的地图高度设置这个值

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 检测是否是玩家
        if (collision.CompareTag("Player"))
        {
            // 获取玩家当前的坐标
            Vector3 currentPos = collision.transform.position;

            // 修改位置：保持 X 和 Z 不变，只把 Y 改成上方的高度
            collision.transform.position = new Vector3(currentPos.x, topSpawnY, currentPos.z);
            
            // 可选：如果玩家有刚体，可能需要重置一下速度，防止带着巨大的下坠速度穿过地面
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // 保留水平速度，重置垂直速度为0（或者保持下坠感，看你需求）
                rb.velocity = new Vector2(rb.velocity.x, 0); 
            }
        }
    }
}