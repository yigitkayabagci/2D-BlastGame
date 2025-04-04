using UnityEngine;

public abstract class Obstacle : GridItem
{
    public int health;

    public virtual void TakeDamage(int damage)
    {
        if (IsBroken())
        {
            health -= damage;
        }
        if (health <= 0)
        {
            Die();
        }
    }
    
    public override bool CanFall()
    {
        return false;
    }

    protected virtual void Die()
    {
        string obstacleType = "";
        if (this is Box) obstacleType = "Box";
        else if (this is Stone) obstacleType = "Stone";
        else if (this is Vase) obstacleType = "Vase";
    
        GameObject prefab = GridManager.Instance.obstacleParticlePrefab;
        RectTransform canvas = GridManager.Instance.canvasTransform;

        int numberOfParticles = 5;
        for (int i = 0; i < numberOfParticles; i++)
        {
            GameObject obsEffectGO = Instantiate(prefab, canvas);
            UIObstacleParticleEffectController effectInstance = obsEffectGO.GetComponent<UIObstacleParticleEffectController>();
            if (effectInstance != null)
            {
                effectInstance.transform.position = transform.position;
                effectInstance.SetRandomSprite(obstacleType);
                effectInstance.PlayEffect();
            }
        }
        
        LevelManager.Instance.OnObstacleDestroyed(obstacleType);
        Destroy(gameObject);
    }

    public override void RocketDamage()
    {
        TakeDamage(1);
    }
}