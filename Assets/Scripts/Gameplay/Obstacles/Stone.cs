using UnityEngine;

public class Stone : Obstacle
{
    protected override void Awake()
    {
        base.Awake();
        health = 1;
    }

    public override bool IsBroken()
    {
        return false;
    }

    public override void RocketDamage()
    {
        health -= 1;
        if (health <= 0)
        {
            Die();
        }
    }
    
    

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
    }
}