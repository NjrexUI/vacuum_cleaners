using UnityEngine;

public class EnemyKiller : MonoBehaviour
{
    [System.Obsolete]
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            foreach (var enemy in FindObjectsOfType<EnemyBasic>())
            {
                enemy.TakeDamage(1);
            }
        }
    }
}
