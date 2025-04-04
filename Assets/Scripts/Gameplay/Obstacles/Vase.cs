using UnityEngine;
using UnityEngine.UI;

public class Vase : Obstacle
{
    [SerializeField] private Sprite damagedSprite;
    
    public bool fallDamageApplied = false; 
    
    public bool hasFallen = false; 

    protected override void Awake()
    {
        base.Awake();
        health = 2; // Initialize with 2 health
    }

    public override void TakeDamage(int damage)
    {
        int org_hea = health;
        health -= damage;
        if (org_hea == 2 && health == 1 && imgComp != null && damagedSprite != null)
        {
            imgComp.sprite = damagedSprite;
        }
        
        if (health <= 0)
        {
            Die();
        }
    }

    public override bool CanFall()
    {
        return true;
    }

    public void reset_flag()
    {
        fallDamageApplied = false;
    }
}