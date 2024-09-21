using UnityEngine;
public class CameraFollow : MonoBehaviour
{
    [SerializeField] Vector3 offset;
    Camera cam;
    [SerializeField] Transform player;
    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
    }
    void OnValidate()
    {
        transform.position = player.position + offset;
    }
    private void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, (player.position + offset), 0.8f);
    }
    void OnDrawGizmos()
    {
        transform.position = Vector3.Lerp(transform.position, (player.position + offset), 0.8f);
    }
}