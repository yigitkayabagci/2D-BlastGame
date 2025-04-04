using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;

public class AnimationController : MonoBehaviour
{
    public static AnimationController Instance { get; private set; }
    
    [SerializeField] private float blastScaleFactor = 1.2f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        DOTween.Init();
    }

    public async Task blast_anim(Transform target, float duration = 0.3f)
    {
        if (target == null) return;

        Vector3 originalScale = target.localScale;
        Sequence blastSequence = DOTween.Sequence();
        
        blastSequence.Append(
            target.DOScale(originalScale * blastScaleFactor, duration * 0.3f)
                .SetEase(Ease.OutQuad)
        );
        
        blastSequence.Append(
            target.DOScale(Vector3.zero, duration * 0.7f)
                .SetEase(Ease.InBack, 2.5f)
        );
        
        float randomRotation = Random.Range(-180f, 180f);
        Tween rotateTween = target.DORotate(new Vector3(0, 0, randomRotation), duration, RotateMode.FastBeyond360)
            .SetEase(Ease.InOutQuad);
        
        await Task.WhenAll(
            blastSequence.AsyncWaitForCompletion(),
            rotateTween.AsyncWaitForCompletion()
        );
    }
}