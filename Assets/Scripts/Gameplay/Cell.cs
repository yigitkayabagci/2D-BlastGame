using UnityEngine;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour, IPointerClickHandler
{
    public int x;
    public int y;
    private GridItem currentItem;
    
    [SerializeField] private SpriteRenderer cellRenderer;

    private void Awake()
    {
        if (cellRenderer == null)
            cellRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetCoordinates(int x, int y)
    {
        this.x = x;
        this.y = y;
        name = $"Cell_{x}_{y}";
    }

    public void SetItem(GridItem item)
    {
        currentItem = item;
        if (item != null)
        {
            item.transform.SetParent(transform);
            item.transform.localPosition = Vector3.zero;
            item.transform.localScale = Vector3.one;
        
            RectTransform itemRect = item.GetComponent<RectTransform>();
            if (itemRect != null)
            {
                itemRect.anchorMin = new Vector2(0.5f, 0.5f);
                itemRect.anchorMax = new Vector2(0.5f, 0.5f);
                itemRect.pivot = new Vector2(0.5f, 0.5f);
                RectTransform cellRect = GetComponent<RectTransform>();
                itemRect.sizeDelta = cellRect.sizeDelta;
            }
        
            item.Initialize(x, y);
        }
    }

    public GridItem GetItem()
    {
        return currentItem;
    }

    public bool IsEmpty()
    {
        return currentItem == null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GridManager gridManager = FindAnyObjectByType<GridManager>();
        if (gridManager != null)
        {
            gridManager.OnCellClicked(this);
        }
    }
}