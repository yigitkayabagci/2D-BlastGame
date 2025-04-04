using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIObstacleParticleEffectController : MonoBehaviour
{
    private Image imgComp;
    public ObstacleParticleSet[] obstacleSets;
    public float effectDuration = 1.8f;
    public Vector2 xOffsetRange = new Vector2(-50f, 50f);
    public Vector2 yOffsetRange = new Vector2(-100f, -50f);

    void Awake()
    {
        imgComp = GetComponent<Image>();
    }


    public void SetRandomSprite(string obstacleType)
    {
        bool foundSet = false;
        foreach (var set in obstacleSets)
        {
            if (set.obstacleType == obstacleType)
            {
                foundSet = true;
                if (set.sprites != null && set.sprites.Length > 0)
                {
                    int index = Random.Range(0, set.sprites.Length);
                    imgComp.sprite = set.sprites[index];
                }
                else
                {
                    //Debug.LogWarning("No sprites");
                }
                break;
            }
        }
        if (!foundSet)
        {
            //Debug.LogWarning("No obstacle");
        }
    }
    public void PlayEffect()
    {
        float randomX = Random.Range(xOffsetRange.x, xOffsetRange.y);
        float peakY = Random.Range(10f, 20f);
        float targetY = transform.position.y + Random.Range(yOffsetRange.x, yOffsetRange.y);

        Vector3 peak_pos = new Vector3(transform.position.x + randomX, transform.position.y + peakY, transform.position.z);
        Vector3 targetPosition = new Vector3(transform.position.x + randomX, targetY, transform.position.z);

        float riseTime = effectDuration * 0.1f;
        float fallTime = effectDuration * 0.55f;

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMove(peak_pos, riseTime).SetEase(Ease.OutQuad));
        seq.Append(transform.DOMove(targetPosition, fallTime).SetEase(Ease.InQuad));
        seq.Join(GetComponent<Image>().DOFade(0f, fallTime).SetEase(Ease.Linear));
        seq.Join(transform.DOScale(0f, fallTime).SetEase(Ease.InQuad));
        seq.OnComplete(() => Destroy(gameObject));
    }

}


[System.Serializable]
public class ObstacleParticleSet {
    public string obstacleType;
    public Sprite[] sprites;
}