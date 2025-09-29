using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltimateObject : MonoBehaviour
{
    public int maxHealth = 500;
    public int currentHealth;

    private Collider col;
    private Renderer rend;

    void Awake()
    {
        currentHealth = maxHealth;
        col = GetComponent<Collider>();
        rend = GetComponent<Renderer>();
    }
    
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}
