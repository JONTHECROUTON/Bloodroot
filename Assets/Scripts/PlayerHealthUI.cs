using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Image[] heartImages;

    void Start()
    {
        if (playerHealth == null) { Debug.LogError("PlayerHealthUI: playerHealth not assigned!"); return; }
        UpdateHearts(playerHealth.GetMaxHealth());
    }

    public void UpdateHearts(int currentHealth)
    {
        for (int i = 0; i < heartImages.Length; i++)
            if (heartImages[i] != null)
                heartImages[i].enabled = i < currentHealth;
    }
}
