using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;

    private CharacterHealthSystem healthSystem;

    public void Bind(CharacterHealthSystem newHealthSystem)
    {
        healthSystem = newHealthSystem;
        healthSystem.HealthChanged += HandleHealthChanged;

        //UI 구독 전에 Initialize가 끝났으므로 현재 값 직접 표시
        HandleHealthChanged(healthSystem.CurrentHealth, healthSystem.MaxHealth);
    }

    public void Unbind()
    {
        if (healthSystem != null)
        {
            healthSystem.HealthChanged -= HandleHealthChanged;
        }

        healthSystem = null;
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        healthText.text =$"{currentHealth:0} / {maxHealth:0}";
    }
}