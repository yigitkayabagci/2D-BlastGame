using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIParticleEffectController : MonoBehaviour
{
    private Image imgComp;
    public Sprite redSprite;
    public Sprite greenSprite;
    public Sprite blueSprite;
    public Sprite yellowSprite;
    public float effectDuration = 0.5f;

    void Awake()
    {
        imgComp = GetComponent<Image>();
    }

    public void SetParticleSprite(Cube.CubeType cubtype)
    {
        Sprite selectedSprite = null;
        switch (cubtype)
        {
            case Cube.CubeType.Red:
                selectedSprite = redSprite;
                break;
            case Cube.CubeType.Green:
                selectedSprite = greenSprite;
                break;
            case Cube.CubeType.Blue:
                selectedSprite = blueSprite;
                break;
            case Cube.CubeType.Yellow:
                selectedSprite = yellowSprite;
                break;
        }
        if (selectedSprite != null)
        {
            imgComp.sprite = selectedSprite;
        }
    }

    public void PlayEffect()
    {
        float randX = Random.Range(-20f, 20f);
        float randY = Random.Range(-50f, -30f);
        Vector3 targetPosition = transform.position + new Vector3(randX, randY, 0);

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMove(targetPosition, effectDuration).SetEase(Ease.OutCubic));
        seq.Join(imgComp.DOFade(0f, effectDuration).SetEase(Ease.Linear));
        seq.Join(transform.DOScale(0f, effectDuration).SetEase(Ease.InQuad));
        seq.OnComplete(() => Destroy(gameObject));
    }
}