using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystemExample : MonoBehaviour
{
    [SerializeField] float maxHealth;
    [SerializeField] float segmentValue;
    bool isAlive;
    float currentHealth;
    float currentSegmentValue;

    [Header("HACKS")]
    [SerializeField] float damageAmount;
    [SerializeField] float healAmount;
    [SerializeField] float maxHealthAmount;
    [SerializeField] float segmentValueExtra;

    //Events
    public delegate void SetHealth(float health,float maxHealth,float _segmentValue);
    public delegate void HealthChanged(float actualHealth,float previousHealth);
    public static event SetHealth OnSetHealth;
    public static event HealthChanged OnHealthChanged;
    void Start()
    {
        isAlive = true;
        currentHealth = maxHealth;
        currentSegmentValue = segmentValue;
        OnSetHealth?.Invoke(currentHealth,maxHealth,currentSegmentValue);
    }
    /////////////////////////////
    private void Update() {
        if(Input.GetKeyDown(KeyCode.N))
        {
            TakeDamage(damageAmount);
        }
        if(Input.GetKeyDown(KeyCode.M) && CanHeal())
        {
            Heal(healAmount);
        }
        if(Input.GetKeyDown(KeyCode.Z))
        {
            AddMaxHealth(maxHealthAmount);
        }
        if(Input.GetKeyDown(KeyCode.X) && maxHealth - maxHealthAmount > 0)
        {
            RemoveMaxHealth(maxHealthAmount);
        }
        if(Input.GetKeyDown(KeyCode.C))
        {
            ChangeSegmentAmount();
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            Start();
        }
    }
    /////////////////////////////
    public void TakeDamage(float damage)
    {
        if(!isAlive) return;
        
        float previousHealth = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth-damage,0,maxHealth);
        //Update UI
        OnHealthChanged?.Invoke(currentHealth,previousHealth);
        //Comprobación de posible final de partida
        isAlive = currentHealth > 0;
        if(!isAlive)
        {
            //DIE
        }
    }
    public void Heal(float amount)
    {
        if(!isAlive) return;
        //Update UI
        OnHealthChanged?.Invoke(Mathf.Clamp(currentHealth + amount,0,maxHealth),currentHealth);
        //Logic
        currentHealth = Mathf.Clamp(currentHealth + amount,0,maxHealth);
    }
    public void AddMaxHealth(float amount)
    {
        if(!isAlive) return;
        //Logic
        maxHealth += amount;
        currentHealth += amount;
        //Update UI
        OnSetHealth?.Invoke(currentHealth,maxHealth,currentSegmentValue);
    }
    public void RemoveMaxHealth(float amount)
    {
        if(!isAlive) return;
        //Logic
        maxHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth,0,maxHealth);
        //Update UI
        OnSetHealth?.Invoke(currentHealth,maxHealth,currentSegmentValue);
    }
    public void ChangeSegmentAmount()
    {
        currentSegmentValue = currentSegmentValue == segmentValue ? segmentValueExtra : segmentValue;
        OnSetHealth?.Invoke(currentHealth,maxHealth,currentSegmentValue);
    }
    public bool CanHeal()
    {
        return currentHealth < maxHealth;
    }
}
