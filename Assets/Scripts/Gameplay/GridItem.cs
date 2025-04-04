using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public abstract class GridItem : MonoBehaviour


{
    public int currentX;
    public int currentY;

    
    [SerializeField] protected Image imgComp;
    protected virtual void Awake()
    {
        if (imgComp == null)
            imgComp = GetComponent<Image>();
    }
    
    public virtual void Initialize(int x, int y)
    {
        currentX = x;
        currentY = y;
    }
    
    public virtual async Task Explode()
    {
        await AnimationController.Instance.blast_anim(transform);
        OnExploded();
    }
    
    protected virtual void OnExploded()
    {
        if (this != null && gameObject != null)
        {
            Destroy(gameObject);
        }
    }
    
    public virtual bool CanFall()
    {
        return true;
    }

    public virtual bool IsBroken()
    {
        return true;
    }
    
    public virtual void RocketDamage() {}
    public virtual void OnFall()
    {
    }
}