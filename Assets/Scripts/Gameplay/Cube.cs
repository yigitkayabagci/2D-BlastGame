using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading.Tasks; // Task kullanımı için

public class Cube : GridItem
{
    public enum CubeType { Red, Green, Blue, Yellow }

    [System.Serializable]
    public class CubeSprites
    {
        public Sprite normalSprite;
        public Sprite rocketSprite;
    }

    [Header("Cube Properties")]
    [SerializeField] private CubeType cubeType;
    public int health = 1;

    [Header("Sprites")]
    [SerializeField] private CubeSprites redCube;
    [SerializeField] private CubeSprites greenCube;
    [SerializeField] private CubeSprites blueCube;
    [SerializeField] private CubeSprites yellowCube;

    private bool isRocket = false;

    protected override void Awake()
    {
        base.Awake();
        UpdateVisuals();
    }

    public override void Initialize(int x, int y)
    {
        base.Initialize(x, y);
        UpdateVisuals();
    }

    public CubeType GetCubeType()
    {
        return cubeType;
    }

    public void SetCubeType(CubeType type)
    {
        cubeType = type;
        UpdateVisuals();
    }

    public void SetRandomType()
    {
        cubeType = (CubeType)Random.Range(0, System.Enum.GetValues(typeof(CubeType)).Length);
        UpdateVisuals();
    }

    public void ShowRocketIcon(bool show)
    {
        isRocket = show;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (imgComp != null)
        {
            imgComp.sprite = isRocket ? GetSpritesForCurrentType().rocketSprite : GetSpritesForCurrentType().normalSprite;
        }
    }

    private CubeSprites GetSpritesForCurrentType()
    {
        switch (cubeType)
        {
            case CubeType.Red:
                return redCube;
            case CubeType.Green:
                return greenCube;
            case CubeType.Blue:
                return blueCube;
            case CubeType.Yellow:
                return yellowCube;
            default:
                return redCube;
        }
    }
    
    public async void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            await Die();
        }
    }

    public override void RocketDamage()
    {
        TakeDamage(1);
    }

    public override bool CanFall()
    {
        return true;
    }

    public override void OnFall()
    {
    }

    public async Task Die()
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject effectPrefab = GridManager.Instance.particleEffectPrefab;
            RectTransform canvasTransform = GridManager.Instance.canvasTransform;
        
            UIParticleEffectController effectInstance = Instantiate(effectPrefab, canvasTransform)
                .GetComponent<UIParticleEffectController>();
            effectInstance.transform.position = transform.position;
            effectInstance.SetParticleSprite(cubeType);
            effectInstance.PlayEffect();
        }
    
        if(this != null)
        {
            await transform.DOScale(0f, 0.15f).AsyncWaitForCompletion();
        }
    
        if(this != null && gameObject != null)
        {
            Destroy(gameObject);
        }
    }

}
