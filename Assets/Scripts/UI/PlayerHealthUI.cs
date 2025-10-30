using TMPro;
using UnityEngine;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI healthText;  // Assign in Inspector
    private PlayerManagerGlobal player;         // Reference to player script that has the variable

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManagerGlobal>();
    }

    void Update()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManagerGlobal>();
        if (player != null && healthText != null)
        {
            healthText.text = $"{player.currentPool}";
        }
    }
}
