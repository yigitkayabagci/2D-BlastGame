using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class Rocket : GridItem
{
    public enum Direction { Horizontal, Vertical }

    [Header("Rocket Properties")]
    public Direction rocketDirection;
    
    [Header("Size Settings")]
    [SerializeField] private Vector2 horizontalSize = new Vector2(65f, 50f);
    [SerializeField] private Vector2 verticalSize = new Vector2(50f, 65f);

    [Header("Rocket Part Prefabs")]
    [SerializeField] private GameObject horizontalLeftProjectilePrefab;
    [SerializeField] private GameObject horizontalRightProjectilePrefab;
    [SerializeField] private GameObject verticalUpProjectilePrefab;
    [SerializeField] private GameObject verticalDownProjectilePrefab;

    [Header("Sprites")]
    [SerializeField] private Sprite horizontalRocketSprite;
    [SerializeField] private Sprite verticalRocketSprite;
    
    public bool isExplode = false;
    private RectTransform rectTransform;

    protected override void Awake()
    {
        base.Awake();
        rectTransform = GetComponent<RectTransform>();
        UpdateVisuals();
    }

    public override void Initialize(int x, int y)
    {
        base.Initialize(x, y);
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (imgComp != null)
        {

            if (rocketDirection == Direction.Horizontal)
            {
                imgComp.sprite = horizontalRocketSprite;
            }
            else
            {
                imgComp.sprite = verticalRocketSprite;
            }
        }
        
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = (rocketDirection == Direction.Horizontal) ? horizontalSize : verticalSize;
        }
    }

    public void SetDirection(Direction direction) {
        rocketDirection = direction; 
        UpdateVisuals();
    }

    public override async Task Explode()
    {
        if (isExplode) return;
        isExplode = true;
        GridManager.StartRocketEffect();
        await AnimationController.Instance.blast_anim(transform);
        Task rocketPartsTask = CreateRocketPartsAsync();
        GridManager.Instance.RegisterRocketTask(rocketPartsTask);
        await rocketPartsTask;
        OnExploded();
    }
    private async Task CreateRocketPartsAsync()
    {
        List<Task> all_project_tasks = new List<Task>();

        if (rocketDirection == Direction.Horizontal)
        {
            GameObject leftPart = Instantiate(horizontalLeftProjectilePrefab, transform.position, Quaternion.identity, transform.parent);
            GameObject rightPart = Instantiate(horizontalRightProjectilePrefab, transform.position, Quaternion.identity, transform.parent);

            ApplySizeToProjectile(leftPart, horizontalSize);
            ApplySizeToProjectile(rightPart, horizontalSize);

            var leftProjectile = leftPart.GetComponent<RocketProjectile>();
            leftProjectile.startX = currentX;
            leftProjectile.startY = currentY;
            leftProjectile.direction = Vector2.left;
            all_project_tasks.Add(leftProjectile.MoveAlongGrid());

            var rightProjectile = rightPart.GetComponent<RocketProjectile>();
            rightProjectile.startX = currentX;
            rightProjectile.startY = currentY;
            rightProjectile.direction = Vector2.right;
            all_project_tasks.Add(rightProjectile.MoveAlongGrid());
        }
        else
        {
            GameObject upPart = Instantiate(verticalUpProjectilePrefab, transform.position, Quaternion.identity, transform.parent);
            GameObject downPart = Instantiate(verticalDownProjectilePrefab, transform.position, Quaternion.identity, transform.parent);

            ApplySizeToProjectile(upPart, verticalSize);
            ApplySizeToProjectile(downPart, verticalSize);

            var upProjectile = upPart.GetComponent<RocketProjectile>();
            upProjectile.startX = currentX;
            upProjectile.startY = currentY;
            upProjectile.direction = Vector2.up;
            all_project_tasks.Add(upProjectile.MoveAlongGrid());

            var downProjectile = downPart.GetComponent<RocketProjectile>();
            downProjectile.startX = currentX;
            downProjectile.startY = currentY;
            downProjectile.direction = Vector2.down;
            all_project_tasks.Add(downProjectile.MoveAlongGrid());
        }

        await Task.WhenAll(all_project_tasks);
    }

    private void ApplySizeToProjectile(GameObject projectile, Vector2 size)
    {
        RectTransform projectileRect = projectile.GetComponent<RectTransform>();
        if (projectileRect != null)
        {
            projectileRect.sizeDelta = size;
        }
    }

    public override bool CanFall()
    {
        return true;
    }
    
    
    //for combo rocket 6 
public async Task six_pack_combo()
{
    if(isExplode) return;
    isExplode = true;
    
    Vector3 currentPosition = transform.position;
    Transform currentParent = transform.parent;
    int rocketX = currentX;
    int rocketY = currentY;
    
    GridManager.StartRocketEffect();
    
    // Play blast animation
    await AnimationController.Instance.blast_anim(transform);
    
    if (this == null || gameObject == null)
    {
        GridManager.FinishRocketEffect();
        return;
    }
    
    List<Task> comboProjectileTasks = new List<Task>();
    
    try
    {
        GameObject leftProj = Instantiate(horizontalLeftProjectilePrefab, currentPosition, Quaternion.identity, currentParent);
        ApplySizeToProjectile(leftProj, horizontalSize);
        var leftProjectile = leftProj.GetComponent<RocketProjectile>();
        leftProjectile.startX = rocketX;
        leftProjectile.startY = rocketY;
        leftProjectile.direction = Vector2.left;
        comboProjectileTasks.Add(leftProjectile.MoveAlongGrid());
        
        GameObject rightProj = Instantiate(horizontalRightProjectilePrefab, currentPosition, Quaternion.identity, currentParent);
        ApplySizeToProjectile(rightProj, horizontalSize);
        var rightProjectile = rightProj.GetComponent<RocketProjectile>();
        rightProjectile.startX = rocketX;
        rightProjectile.startY = rocketY;
        rightProjectile.direction = Vector2.right;
        comboProjectileTasks.Add(rightProjectile.MoveAlongGrid());
        
        // Vertical projectiles (up and down)
        GameObject upProj = Instantiate(verticalUpProjectilePrefab, currentPosition, Quaternion.identity, currentParent);
        ApplySizeToProjectile(upProj, verticalSize);
        var upProjectile = upProj.GetComponent<RocketProjectile>();
        upProjectile.startX = rocketX;
        upProjectile.startY = rocketY;
        upProjectile.direction = Vector2.up;
        comboProjectileTasks.Add(upProjectile.MoveAlongGrid());
        
        GameObject downProj = Instantiate(verticalDownProjectilePrefab, currentPosition, Quaternion.identity, currentParent);
        ApplySizeToProjectile(downProj, verticalSize);
        var downProjectile = downProj.GetComponent<RocketProjectile>();
        downProjectile.startX = rocketX;
        downProjectile.startY = rocketY;
        downProjectile.direction = Vector2.down;
        comboProjectileTasks.Add(downProjectile.MoveAlongGrid());
        OnExploded();
        Destroy(gameObject);
        await Task.WhenAll(comboProjectileTasks);
    }
    catch (System.Exception ex)
    {
        if (comboProjectileTasks.Count == 0)
        {
            GridManager.FinishRocketEffect();
        }
    }
}
}