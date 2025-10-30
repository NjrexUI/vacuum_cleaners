using UnityEngine;

public class PaintProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 2f;
    private Vector2 direction;

    public void Initialize(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyBasic>()?.TakeDamage(1);
        }

        Destroy(gameObject);
    }
}
