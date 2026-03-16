using UnityEngine;
using UnityEngine.UI;

public class PlayerLivesUI : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Heart Images")]
    [SerializeField] private Image[] hearts;

    [Header("Sprites")]
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;

    private void Start()
    {
        if (playerHealth != null)
        {
            playerHealth.OnLivesChanged += UpdateHearts;
            UpdateHearts(playerHealth.CurrentLives, playerHealth.MaxLives);
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnLivesChanged -= UpdateHearts;
        }
    }

    private void UpdateHearts(int currentLives, int maxLives)
    {
        if (hearts == null || hearts.Length == 0) return;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;

            if (i < currentLives)
            {
                hearts[i].sprite = fullHeartSprite;
            }
            else
            {
                hearts[i].sprite = emptyHeartSprite;
            }
        }
    }
}