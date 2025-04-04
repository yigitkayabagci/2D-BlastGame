using System.Threading.Tasks;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RocketProjectile : MonoBehaviour
{
    public int startX;
    public int startY;
    public Vector2 direction;
    [Header("Movement Settings")]
    public float totalMoveDuration = 0.5f;
    public Ease movementEase = Ease.InOutQuad;
    public float trailEffectMultiplier = 1.5f; 
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject smokePrefab;
    public float smokeinterval = 0.02f;
    public float smokeof_dist = 0.2f;
    public bool addMotionBlur = true;
    
    [SerializeField] private GameObject motionBlurPrefab;
    
    [Header("Screen Effects")]
    public bool addScreenShake = false;
    public float shakeStrength = 0.2f;
    
    private float lastSmokeTime = 0f;
    private List<GameObject> smokeTrail = new List<GameObject>();
    private bool isMoving = false;
    private ParticleSystem rocketParticles;

    private void Awake()
    {
        rocketParticles = GetComponentInChildren<ParticleSystem>();
    }

    private Vector3 GetSmokeOffset()
    {
        Vector3 offset = Vector3.zero;
        
        if (direction == Vector2.up)
            offset = new Vector3(0, -smokeof_dist, 0);
        else if (direction == Vector2.down)
            offset = new Vector3(0, smokeof_dist, 0);
        else if (direction == Vector2.right)
            offset = new Vector3(-smokeof_dist, 0, 0);
        else if (direction == Vector2.left)
            offset = new Vector3(smokeof_dist, 0, 0);
        
        return offset;
    }

    private void Update()
    {
        if (isMoving && smokePrefab != null && Time.time > lastSmokeTime + smokeinterval)
        {
            SpawnSmokeParticle();
            lastSmokeTime = Time.time;
        }
    }

    private void SpawnSmokeParticle()
    {
        if (smokePrefab != null && gameObject != null)
        {
            for (int i = 0; i < (Random.value > 0.7f ? 2 : 1); i++)
            {
                Vector3 randomOffset = new Vector3(
                    Random.Range(-0.08f, 0.08f),
                    Random.Range(-0.08f, 0.08f),
                    0);
                    
                Vector3 smokePosition = transform.position + GetSmokeOffset() + randomOffset;
                GameObject smokeInstance = Instantiate(smokePrefab, smokePosition, Quaternion.identity);
                
                if (transform.parent != null)
                {
                    smokeInstance.transform.SetParent(transform.parent);
                }
                
                float randomScale = Random.Range(0.85f, 1.15f);
                smokeInstance.transform.localScale *= randomScale;
                
                smokeTrail.Add(smokeInstance);
                
                Vector3 smokePos = smokeInstance.transform.position;
                smokeInstance.transform.position = new Vector3(smokePos.x, smokePos.y, smokePos.z + 0.1f);
            }
        }
    }

    public async Task MoveAlongGrid()
    {
        transform.SetAsLastSibling();
        Vector3 startPos = GridManager.Instance.GetCellPosition(startX, startY);
        transform.position = startPos;
        int endX = startX;
        int endY = startY;
        while (true)
        {
            int nextX = endX + (int)direction.x;
            int nextY = endY + (int)direction.y;
            
            if (!GridManager.Instance.IsWithinBounds(nextX, nextY))
                break;
                
            endX = nextX;
            endY = nextY;
        }
        
        Vector3 endPos = GridManager.Instance.GetCellPosition(endX, endY);
        
        if (addMotionBlur && motionBlurPrefab != null)
        {
            GameObject blurEffect = Instantiate(motionBlurPrefab, transform);
            if (direction == Vector2.up || direction == Vector2.down)
                blurEffect.transform.localScale = new Vector3(1, trailEffectMultiplier, 1);
            else
                blurEffect.transform.localScale = new Vector3(trailEffectMultiplier, 1, 1);
        }
        
        if (rocketParticles != null)
        {
            rocketParticles.Play();
        }
        
        List<Vector2Int> cellsToCheck = new List<Vector2Int>();
        int cellCount = Mathf.Max(Mathf.Abs(endX - startX), Mathf.Abs(endY - startY)) + 1;
        
        for (int i = 0; i < cellCount; i++)
        {
            int checkX = startX + (int)(direction.x * i);
            int checkY = startY + (int)(direction.y * i);
            cellsToCheck.Add(new Vector2Int(checkX, checkY));
        }
        
        isMoving = true;
        
        if (addScreenShake && Camera.main != null)
        {
            Camera.main.transform.DOShakePosition(totalMoveDuration, shakeStrength, 10, 90, false);
        }
        
        Sequence moveSequence = DOTween.Sequence();
        Vector3 prepDirection = -new Vector3(direction.x, direction.y, 0) * 0.1f;
        moveSequence.Append(transform.DOMove(startPos + prepDirection, totalMoveDuration * 0.1f).SetEase(Ease.OutQuad));
        moveSequence.Append(transform.DOMove(endPos, totalMoveDuration).SetEase(movementEase));
        
        float startTime = Time.time;
        int lastProcessedCell = 0;
        
        while (Time.time - startTime < totalMoveDuration * 1.1f && this != null)
        {
            float t = (Time.time - startTime) / (totalMoveDuration * 1.1f);
            int cellToProcess = Mathf.Min(Mathf.FloorToInt(t * cellCount), cellCount - 1);
            
            for (int i = lastProcessedCell + 1; i <= cellToProcess; i++)
            {
                if (i >= cellsToCheck.Count) break;
                
                Vector2Int cell = cellsToCheck[i];
                await ProcessCell(cell.x, cell.y);
            }
            
            lastProcessedCell = cellToProcess;
            
            await Task.Yield();
        }
        
        for (int i = lastProcessedCell + 1; i < cellsToCheck.Count; i++)
        {
            Vector2Int cell = cellsToCheck[i];
            await ProcessCell(cell.x, cell.y);
        }
        
        isMoving = false;
        
        if (gameObject != null)
            Destroy(gameObject);
            
        GridManager.FinishRocketEffect();
    }
    
    private async Task ProcessCell(int x, int y)
    {
        GridItem targetItem = GridManager.Instance.GetItemAt(x, y);
        if (targetItem != null)
        {
            if (targetItem is Vase vase)
            {
               vasecase(vase);
            }
            else if (targetItem is Obstacle obs)
            {
                obs.RocketDamage();
            }
            else if (targetItem is Rocket rocket && !rocket.isExplode)
            {
                _ = rocket.Explode();
            }
            else if (targetItem is Cube cube)
            {
                cube.TakeDamage(1);
                CheckAndClearCell(x, y);
            }
            else
            {
                targetItem.Explode();
                CheckAndClearCell(x, y);
            }
            await Task.Delay(10);
        }
    }

    private void CheckAndClearCell(int x, int y)
    {
        GridItem currentItem = GridManager.Instance.GetItemAt(x, y);
        if (currentItem == null || currentItem.gameObject == null)
        {
            GridManager.Instance.ClearCell(x, y);
        }
    }

    private void vasecase(Vase vase)
    {
        if (vase.health > 1)
        {
            vase.health -= 1;
                    
            var imageComp = vase.GetComponent<UnityEngine.UI.Image>();
            if (imageComp != null)
            {
                var fieldInfo = vase.GetType().GetField("damagedSprite", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                            
                if (fieldInfo != null)
                {
                    var damagedSprite = fieldInfo.GetValue(vase) as Sprite;
                    if (damagedSprite != null)
                    {
                        imageComp.sprite = damagedSprite;
                    }
                }
            }
        }
        else
        {
            vase.TakeDamage(1);
        }
    }
}