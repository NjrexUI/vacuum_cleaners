using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManagerGlobal : MonoBehaviour
{
    private int maxHealth = 15;
    public int currentPool;

    private void Start()
    {
        currentPool = maxHealth;
        DontDestroyOnLoad(gameObject);
    }
    public void TakeDamage(int amount)
    {
        currentPool -= amount;

        if (currentPool <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
        SceneManager.LoadScene("GameOverScreen");
    }

}
