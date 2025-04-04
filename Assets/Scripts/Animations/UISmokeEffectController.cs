using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UISmokeEffectController : MonoBehaviour
{
    [Header("Animation Settings")]
    public float lifetime = 3f;       
    public float startScale = 1.7f;      
    public float targetScale = 4f;       
    public float rotationAmount = 25f;   
    [Header("Color Settings")]
    public Color startColor = new Color(1f, 1f, 1f, 0.9f);
    public Color endColor = new Color(1f, 1f, 1f, 0f);
    private Image imageComponent;
    private RectTransform rectTransform;

    private void Awake()
    {
        imageComponent = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }
    
    private void Start()
    {
        float randomRotation = Random.Range(-rotationAmount, rotationAmount);
        rectTransform.rotation = Quaternion.Euler(0, 0, randomRotation);
        Vector3 randomOffset = new Vector3(
            Random.Range(-0.15f, 0.15f),
            Random.Range(-0.15f, 0.15f),
            0);
        rectTransform.localPosition += randomOffset;
        
        rectTransform.localScale = Vector3.one * startScale;
        
        if (imageComponent != null)
        {
            imageComponent.color = startColor;
            
            imageComponent.DOColor(endColor, lifetime);
        }
        
        float randomScaleFactor = Random.Range(0.95f, 1.15f);
        rectTransform.DOScale(targetScale * randomScaleFactor, lifetime)
            .SetEase(Ease.OutQuad);
        
        Vector3 driftDirection = Random.insideUnitCircle.normalized * 0.4f;
        rectTransform.DOLocalMove(rectTransform.localPosition + driftDirection, lifetime)
            .SetEase(Ease.OutQuad);
        
        Destroy(gameObject, lifetime);
    }
}