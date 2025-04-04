using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Models;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")] public int width;
    public int height;
    public GameObject cellPrefab;
    public Transform gridParent;

    [Header("Prefabs")] [SerializeField] private GameObject cubePrefab;
    [SerializeField] private GameObject boxPrefab;
    [SerializeField] private GameObject stonePrefab;
    [SerializeField] private GameObject vasePrefab;

    [Header("Rocket Settings")] [SerializeField]
    private GameObject RocketPrefab;

    private Cell[,] grid;
    private bool isProcessing = false;
    
    [Header("Rocket Handling")]
    public static int activeRocketCount = 0;
    private bool isRocketChainInProgress = false;
    private List<Task> pendingRocketTasks = new List<Task>();
    
    public bool gravityLocked = false;
    
    public GameObject particleEffectPrefab;
    public GameObject obstacleParticlePrefab;
    public RectTransform canvasTransform;
    
    
    public static void StartRocketEffect()
    {
        Instance.gravityLocked = true;
        activeRocketCount++;
        Instance.isRocketChainInProgress = true;
    }

    public static void FinishRocketEffect()
    {
        activeRocketCount--;
        if (activeRocketCount <= 0)
        {
            activeRocketCount = 0; 
            Instance.isRocketChainInProgress = false;
            Instance.gravityLocked = false;
            Instance.StartCoroutine(Instance.rocketChain());
        }
    }

    private IEnumerator rocketChain()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.2f);
        if (activeRocketCount <= 0 && pendingRocketTasks.Count == 0)
        {
            yield return new WaitForSeconds(0.1f);
            if (activeRocketCount <= 0 && pendingRocketTasks.Count == 0)
            {
                isRocketChainInProgress = false;
                gravityLocked = false;
                _ = UpdateGridAfterExplosionAsync();
            }
        }
    }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    
        DOTween.Init();
    }
    public void CreateGridFromData(LevelData levelData)
    {
        width = levelData.grid_width;
        height = levelData.grid_height;

        if (grid != null) ClearGrid();

        CreateGrid();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                if (index < levelData.grid.Length)
                {
                    string itemType = levelData.grid[index];
                    CreateItemFromType(x, y, itemType);
                }
            }
        }
        UpdateRocketHints();
    }

    public void ClearGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GridItem item = grid[x, y].GetItem();
                if (item != null)
                {
                    Destroy(item.gameObject);
                    grid[x, y].SetItem(null);
                }

            }
        }
    }

    void CreateGrid()
    {
        grid = new Cell[width, height];

        float cellSize = 65f; 
        float spacing = 0f;
        float startX = -(width * (cellSize + spacing)) / 2f + cellSize / 2;
        float startY = -(height * (cellSize + spacing)) / 2f + cellSize / 2;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject cellObj = Instantiate(cellPrefab, gridParent);
                RectTransform cellRect = cellObj.GetComponent<RectTransform>();
                cellRect.anchoredPosition = new Vector2(
                    startX + (x * (cellSize + spacing)),
                    startY + (y * (cellSize + spacing))
                );
                cellRect.sizeDelta = new Vector2(cellSize, cellSize);
                cellRect.pivot = new Vector2(0.5f, 0.5f);
                cellRect.anchorMin = new Vector2(0.5f, 0.5f);
                cellRect.anchorMax = new Vector2(0.5f, 0.5f);

                Cell cell = cellObj.GetComponent<Cell>();
                cell.SetCoordinates(x, y);
                grid[x, y] = cell;
            }
        }
    }

    private void CreateItemFromType(int x, int y, string itemType)
    {
        GridItem newItem = null;

        switch (itemType)
        {
            case "r": 
                newItem = CreateCubeWithColor(Color.red);
                break;
            case "g":
                newItem = CreateCubeWithColor(Color.green);
                break;
            case "b": 
                newItem = CreateCubeWithColor(Color.blue);
                break;
            case "y": 
                newItem = CreateCubeWithColor(Color.yellow);
                break;
            case "rand":
                newItem = CreateRandomCube();
                break;
            case "vro": 
                newItem = CreateRocketWithDirection(Rocket.Direction.Vertical);
                break;
            case "hro": 
                newItem = CreateRocketWithDirection(Rocket.Direction.Horizontal);
                break;
            case "bo": 
                newItem = CreateObstacle(boxPrefab);
                break;
            case "s": 
                newItem = CreateObstacle(stonePrefab);
                break;
            case "v":
                newItem = CreateObstacle(vasePrefab);
                break;
        }

        if (newItem != null)
        {
            PlaceItemAt(x, y, newItem);
        }
    }



    private Cube CreateRandomCube()
    {
        GameObject cubeObject = Instantiate(cubePrefab);
        Cube cube = cubeObject.GetComponent<Cube>();

        cube.SetRandomType();

        return cube;
    }

    private Cube CreateCubeWithColor(Color color)
    {
        GameObject cubeObj = Instantiate(cubePrefab);
        Cube cube = cubeObj.GetComponent<Cube>();

        if (color == Color.red)
            cube.SetCubeType(Cube.CubeType.Red);
        else if (color == Color.green)
            cube.SetCubeType(Cube.CubeType.Green);
        else if (color == Color.blue)
            cube.SetCubeType(Cube.CubeType.Blue);
        else if (color == Color.yellow)
            cube.SetCubeType(Cube.CubeType.Yellow);

        return cube;
    }

    private Rocket CreateRocketWithDirection(Rocket.Direction direction)
    {
        GameObject rocketObj = Instantiate(RocketPrefab);
        Rocket rocket = rocketObj.GetComponent<Rocket>();
        rocket.SetDirection(direction);
        return rocket;
    }

    private Obstacle CreateObstacle(GameObject obstaclePrefab)
    {
        GameObject obstacleObj = Instantiate(obstaclePrefab);
        return obstacleObj.GetComponent<Obstacle>();
    }

    public GridItem GetItemAt(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return grid[x, y].GetItem();
        }

        return null;
    }

    public List<Cell> GetConnectedCells(Cell startCell)
    {
        List<Cell> conn_cells = new List<Cell>();
        Queue<Cell> q = new Queue<Cell>();
        bool[,] visited = new bool[width, height];

        Cube startCube = startCell.GetItem() as Cube;
        if (startCube == null)
            return conn_cells;

        Cube.CubeType targetType = startCube.GetCubeType();
        q.Enqueue(startCell);
        visited[startCell.x, startCell.y] = true;

        while (q.Count > 0)
        {
            Cell current = q.Dequeue();
            conn_cells.Add(current);

            foreach (Cell neighbor in GetNeighbors(current))
            {
                if (!visited[neighbor.x, neighbor.y] && neighbor.GetItem() != null)
                {
                    Cube neighborCube = neighbor.GetItem() as Cube;
                    if (neighborCube != null && neighborCube.GetCubeType() == targetType)
                    {
                        q.Enqueue(neighbor);
                        visited[neighbor.x, neighbor.y] = true;
                    }
                }
            }
        }

        return conn_cells;
    }

    private List<Cell> GetNeighbors(Cell cell)
    {
        List<Cell> neighbors = new List<Cell>();
        int x = cell.x;
        int y = cell.y;

        if (x > 0)
            neighbors.Add(grid[x - 1, y]);
        if (x < width - 1)
            neighbors.Add(grid[x + 1, y]);
        if (y > 0)
            neighbors.Add(grid[x, y - 1]);
        if (y < height - 1)
            neighbors.Add(grid[x, y + 1]);

        return neighbors;
    }

    public void PlaceItemAt(int x, int y, GridItem item)
    {
        if (grid[x, y] != null)
        {
            grid[x, y].SetItem(item);
            item.Initialize(x, y);
        }
    }

    public bool IsEmptyCell(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return grid[x, y].IsEmpty();
        }

        return false;
    }
    
    private void DamageObstacleNeighbors(List<Cell> explodedCells, int damageAmount = 1)
    {
        HashSet<Obstacle> processedObstacles = new HashSet<Obstacle>();
        foreach (Cell cell in explodedCells)
        {
            List<Cell> neighbors = GetNeighbors(cell);
            foreach (Cell neighbor in neighbors)
            {
                if (cell == null)
                {
                    continue;
                }
                
                GridItem neighborItem = neighbor.GetItem();
                if (neighborItem != null && neighborItem is Obstacle)
                {
                    Obstacle obs = neighborItem as Obstacle;
                    if (!processedObstacles.Contains(obs))
                    {
                        obs.TakeDamage(damageAmount);
                        processedObstacles.Add(obs);
                    }
                }
            }
        }
    }
    
    public Rocket CreateComboRocket()
    {
        GameObject rocketObj = Instantiate(RocketPrefab);
        Rocket rocket = rocketObj.GetComponent<Rocket>();
        return rocket;
    }
    
    
    
    // When Cell Clicked!
    public async void OnCellClicked(Cell clickedCell)
    {
        if (clickedCell == null) { return;}
        if (isProcessing) return;
        if (clickedCell.GetItem() == null) return;

        GridItem clickedItem = clickedCell.GetItem();
        
        
        //when rocket
        if (clickedItem is Rocket)
        {
            List<Cell> neighbors = GetNeighbors(clickedCell);
            int rocketNeighborCount = 0;
            foreach (Cell cell in neighbors)
            {
                if (cell.GetItem() is Rocket)
                {
                    rocketNeighborCount++;
                }
            }
            
            // ROCKET COMBO
            if (rocketNeighborCount >= 1)
            {
                await TriggerRocketCombo(clickedCell);while (activeRocketCount > 0 || pendingRocketTasks.Count > 0 || isRocketChainInProgress) {await Task.Delay(75); }
                
                await Task.Delay(75);
                
                for (int i = 0; i < 3; i++)
                {
                    await UpdateGridAfterExplosionAsync();
                    await Task.Delay(30);
                }
                UpdateRocketHints();
                LevelManager.Instance.CheckLevelObjectives();
                return;
            }
        
            // Normal rocket
            LevelManager.Instance.DecreaseMoveCount();
            pendingRocketTasks.Clear();
            isRocketChainInProgress = true;
            await ((Rocket)clickedItem).Explode();
        
            while (activeRocketCount > 0 || pendingRocketTasks.Count > 0 || isRocketChainInProgress)
            {
                await Task.Delay(35);
            }
        
            await Task.Delay(50);
            for (int i = 0; i < 3; i++)
            {
                await UpdateGridAfterExplosionAsync();
                await Task.Delay(50);
            }
            UpdateRocketHints();
            LevelManager.Instance.CheckLevelObjectives();
            return;
        }
        
        
        //when cube NORMAL 
        List<Cell> connectedCells = GetConnectedCells(clickedCell);
        if (connectedCells.Count < 2) return;

        isProcessing = true;
        LevelManager.Instance.DecreaseMoveCount();
        DamageObstacleNeighbors(connectedCells);
    
        if (connectedCells.Count >= 4)
        {
            await HandleRocketExplosion(clickedCell, connectedCells);
        }
        else
        {
            await ExplodeCells(connectedCells);
        }
    
        await UpdateGridAfterExplosionAsync();
        UpdateRocketHints();
        LevelManager.Instance.CheckLevelObjectives();
        isProcessing = false;
    }
    
    private async Task TriggerRocketCombo(Cell centerCell)
    {
        LevelManager.Instance.DecreaseMoveCount();
        int centerX = centerCell.x;
        int centerY = centerCell.y;
        if(centerCell.GetItem() != null)
        {
            Destroy(centerCell.GetItem().gameObject);
            centerCell.SetItem(null);
        }
        Rocket centerComboRocket = CreateComboRocket();
        PlaceItemAt(centerX, centerY, centerComboRocket);
        Rocket rightRocket = null;
        if (IsWithinBounds(centerX + 1, centerY))
        {
            Cell rightCell = grid[centerX + 1, centerY];
            if(rightCell.GetItem() != null)
            {
                Destroy(rightCell.GetItem().gameObject);
                rightCell.SetItem(null);
            }
            rightRocket = CreateRocketWithDirection(Rocket.Direction.Vertical);
            PlaceItemAt(centerX + 1, centerY, rightRocket);
        }
        
        Rocket leftRocket = null;
        if (IsWithinBounds(centerX - 1, centerY))
        {
            Cell leftCell = grid[centerX - 1, centerY];
            if(leftCell.GetItem() != null)
            {
                Destroy(leftCell.GetItem().gameObject);
                leftCell.SetItem(null);
            }
            leftRocket = CreateRocketWithDirection(Rocket.Direction.Vertical);
            PlaceItemAt(centerX - 1, centerY, leftRocket);
        }
        
        Rocket topRocket = null;
        if (IsWithinBounds(centerX, centerY + 1))
        {
            Cell topCell = grid[centerX, centerY + 1];
            if(topCell.GetItem() != null)
            {
                Destroy(topCell.GetItem().gameObject);
                topCell.SetItem(null);
            }
            topRocket = CreateRocketWithDirection(Rocket.Direction.Horizontal);
            PlaceItemAt(centerX, centerY + 1, topRocket);
        }
        
        Rocket bottomRocket = null;
        if (IsWithinBounds(centerX, centerY - 1))
        {
            Cell bottomCell = grid[centerX, centerY - 1];
            if(bottomCell.GetItem() != null)
            {
                Destroy(bottomCell.GetItem().gameObject);
                bottomCell.SetItem(null);
            }
            bottomRocket = CreateRocketWithDirection(Rocket.Direction.Horizontal);
            PlaceItemAt(centerX, centerY - 1, bottomRocket);
        }
        
        List<Task> explosionTasks = new List<Task>();
        explosionTasks.Add(centerComboRocket.six_pack_combo()); 
        if(rightRocket != null) explosionTasks.Add(rightRocket.Explode());
        if(leftRocket != null) explosionTasks.Add(leftRocket.Explode());
        if(topRocket != null) explosionTasks.Add(topRocket.Explode());
        if(bottomRocket != null) explosionTasks.Add(bottomRocket.Explode());
        
        await Task.WhenAll(explosionTasks);
    }
    
    public void CleanupInactiveCubes()
    {
        Cube[] allCubes = FindObjectsOfType<Cube>();
    
        foreach (Cube cube in allCubes)
        {
            bool isInGrid = false;
        
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (GetItemAt(x, y) == cube)
                    {
                        isInGrid = true;
                        break;
                    }
                }
                if (isInGrid) break;
            }
        
            if (!isInGrid && cube.gameObject != null)
            {
                Destroy(cube.gameObject);
            }
        }
    }

    
    
    // 4 block create rocket
    private async Task HandleRocketExplosion(Cell clickedCell, List<Cell> connectedCells)
    {
        List<Cell> cellsToExplode = new List<Cell>(connectedCells);
        cellsToExplode.Remove(clickedCell);
        await ExplodeCells(cellsToExplode);

        GridItem clickedItem = clickedCell.GetItem();
        if (clickedItem != null)
        {
            Destroy(clickedItem.gameObject);
            clickedCell.SetItem(null);
        }
        
        
        //random way
        int randomVal = Random.Range(0, 2);
        Rocket rocket = CreateRocketWithDirection(randomVal == 1 ? Rocket.Direction.Vertical : Rocket.Direction.Horizontal);
        PlaceItemAt(clickedCell.x, clickedCell.y, rocket);
        rocket.transform.DOScale(1.2f, 0.15f).SetLoops(2, LoopType.Yoyo);
    }
    
    public void UpdateRocketHints()
    {
        bool[,] visited = new bool[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!visited[x, y])
                {
                    Cell cell = grid[x, y];
                    GridItem item = cell.GetItem();
                    if (item is Cube)
                    {
                        List<Cell> group = GetConnectedCells(cell);
                        foreach (Cell c in group)
                        {
                            visited[c.x, c.y] = true;
                        }
                        bool showRocket = group.Count >= 4;
                        foreach (Cell c in group)
                        {
                            Cube cube = c.GetItem() as Cube;
                            if (cube != null)
                            {
                                cube.ShowRocketIcon(showRocket);
                            }
                        }
                    }
                }
            }
        }
    }
    
    

    public async Task UpdateGridAfterExplosionAsync()
    {
        if (isRocketChainInProgress || activeRocketCount > 0) { return; }

        for (int i = 0; i < 3; i++)
        {
            await ApplyGravityAsync();
            await Task.Delay(25);
        }
    
        float cellSize = 65f;
    
        for (int x = 0; x < width; x++)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                if (IsEmptyCell(x, y))
                {
                    Cube newCube = CreateRandomCube();
                    PlaceItemAt(x, y, newCube);
                    Vector3 topCellPos = grid[x, height - 1].transform.position;
                    Vector3 spawnPos = topCellPos + new Vector3(0, cellSize, 0);
                    newCube.transform.position = spawnPos;
                    newCube.transform.DOMove(grid[x, y].transform.position, 0.41f);
                }
                else
                {
                    break;
                }
            }
        }
    
        for (int i = 0; i < 3; i++)
        {
            await ApplyGravityAsync();
            await Task.Delay(25);
        }
    }


    
    public async Task ExplodeCells(List<Cell> cellsToExplode)
    {
        List<Task> explosionTasks = new List<Task>();

        foreach (Cell cell in cellsToExplode)
        {
            GridItem item = cell.GetItem();
            if (item != null)
            {
                if (item is Cube cube)
                {
                    Task cubeDieTask = cube.Die();
                    explosionTasks.Add(cubeDieTask);
                }
                else if (item is Obstacle obstacle)
                {
                    obstacle.TakeDamage(1);
                }
            }
        }

        await Task.WhenAll(explosionTasks);

        foreach (Cell cell in cellsToExplode)
        {
            GridItem item = cell.GetItem();
            if (item != null)
            {
                Destroy(item.gameObject);
                cell.SetItem(null);
            }
        }
    }

        public async Task ApplyGravityAsync()
    {
        if (gravityLocked || isRocketChainInProgress || activeRocketCount > 0) { return; }
        
        bool moved;
        do
        {
            moved = false;
            for (int y = 1; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!IsEmptyCell(x, y))
                    {
                        GridItem item = grid[x, y].GetItem();
                        
                        if (IsEmptyCell(x, y - 1))
                        {
                            if (item != null && item.CanFall())
                            {
                                MoveItemDown(x, y, item);
                                moved = true;
                            }
                        }
                        else if (item is Vase vase)
                        {
                            TryApplyFallDamage(vase, x, y);
                        }
                    }
                }
            }
            if (moved)
            {
                await Task.Delay(75);
            }
        } while (moved);
    }

    
        
    //below check
    private void MoveItemDown(int x, int y, GridItem item)
    {
        grid[x, y - 1].SetItem(item);
        grid[x, y].SetItem(null);
        
        Vector3 targetPos = grid[x, y - 1].transform.position;
        item.transform.DOMove(targetPos, 0.15f);
        
        if (item is Vase vase)
        {
            vase.reset_flag();
            vase.hasFallen = true;
        }
    }

    
    
    //vase fall
    private void TryApplyFallDamage(Vase vase, int x, int y)
    {
        if (vase.fallDamageApplied || !vase.hasFallen)
            return;
        
        GridItem belowItem = grid[x, y - 1].GetItem();
        bool hitGround = (y - 1 == 0) || (belowItem != null && !belowItem.CanFall());
        
        if (hitGround)
        {
            vase.TakeDamage(1);
            vase.fallDamageApplied = true;
        }
    }
        

    public Vector3 GetCellPosition(int x, int y)
    {
        return grid[x, y].transform.position;
    }
    
    public void ClearCell(int x, int y)
    {
        if (IsWithinBounds(x, y))
        {
            grid[x, y].SetItem(null);
        }
    }

    public bool IsWithinBounds(int x, int y)
    {
        return (x >= 0 && x < width && y >= 0 && y < height);
    }
    
    public void RegisterRocketTask(Task rocketTask)
    {
        pendingRocketTasks.Add(rocketTask);
        _ = TrackRocketTaskCompletion(rocketTask);
    }

    private async Task TrackRocketTaskCompletion(Task rocketTask)
    {
        await rocketTask;
        pendingRocketTasks.Remove(rocketTask);
        if (pendingRocketTasks.Count == 0 && activeRocketCount <= 0)
        {
            await Task.Delay(50);
        
            if (pendingRocketTasks.Count == 0 && activeRocketCount <= 0 && !isRocketChainInProgress)
            {
                isRocketChainInProgress = false;
                gravityLocked = false;
                _ = UpdateGridAfterExplosionAsync();
            }
        }
    }
}