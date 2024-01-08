using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GridManager : MonoBehaviour
{
    public int rows = 4;
    public int cols = 4;
    public GameObject cellPrefab;
    public GameObject[] ingredientPrefabs;
    private GameObject[,] grid;
    private GameObject breadPrefab;

    private GameObject selectedIngredient;
    private List<GameObject> ingredientsOnBread = new List<GameObject>();
    private Vector2 breadPosition1, breadPosition2;

    [SerializeField] TextMeshProUGUI currentLevel;
    void Start()
    {
        currentLevel.text = "Current Level: " + GameManager.Instance.currentLevel;
        breadPrefab = ingredientPrefabs[ingredientPrefabs.Length - 1];
        CreateGrid();
        PlaceInitialIngredients();
    }

    void CreateGrid()
    {
        grid = new GameObject[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Vector2 position = new Vector2(i, j);
                GameObject cell = Instantiate(cellPrefab, position, Quaternion.identity);
                grid[i, j] = cell;
            }
        }
    }

    Vector2 GetAdjacentPosition(Vector2 currentPosition)
    {
        List<Vector2> possiblePositions = new List<Vector2>();

        if (currentPosition.x > 0)
            possiblePositions.Add(new Vector2(currentPosition.x - 1, currentPosition.y));

        if (currentPosition.x < cols - 1)
            possiblePositions.Add(new Vector2(currentPosition.x + 1, currentPosition.y));

        if (currentPosition.y < rows - 1)
            possiblePositions.Add(new Vector2(currentPosition.x, currentPosition.y + 1));

        if (currentPosition.y > 0)
            possiblePositions.Add(new Vector2(currentPosition.x, currentPosition.y - 1));

        if (possiblePositions.Count > 0)
            return possiblePositions[Random.Range(0, possiblePositions.Count)];

        return currentPosition;
    }

    void PlaceInitialIngredients()
    {
        breadPosition1 = new Vector2(Random.Range(0, rows), Random.Range(0, cols));
        breadPosition2 = GetAdjacentPosition(breadPosition1);

        Instantiate(breadPrefab, breadPosition1, Quaternion.identity);
        UpdateCellContent(breadPosition1, breadPrefab);
        Instantiate(breadPrefab, breadPosition2, Quaternion.identity);
        UpdateCellContent(breadPosition2, breadPrefab);

        int numberOfIngredients = Random.Range(1, (rows * cols) - 2);

        Vector2 currentPos = breadPosition1;

        for (int i = 0; i < numberOfIngredients; i++)
        {
            GameObject ingredientPrefab = ingredientPrefabs[Random.Range(0, ingredientPrefabs.Length - 1)];

            Vector2 nextPos = GetRandomAdjacentPosition(currentPos);

            GameObject newIngredient = Instantiate(ingredientPrefab, nextPos, Quaternion.identity);
            activeIngredients.Add(newIngredient);

            UpdateCellContent(nextPos, newIngredient);

            currentPos = nextPos;
        }
    }

    Vector2 GetRandomAdjacentPosition(Vector2 currentPosition)
    {
        List<Vector2> possiblePositions = new List<Vector2>();

        if (currentPosition.x > 0 && IsCellEmpty(new Vector2(currentPosition.x - 1, currentPosition.y)))
            possiblePositions.Add(new Vector2(currentPosition.x - 1, currentPosition.y));
        if (currentPosition.x < cols - 1 && IsCellEmpty(new Vector2(currentPosition.x + 1, currentPosition.y)))
            possiblePositions.Add(new Vector2(currentPosition.x + 1, currentPosition.y));
        if (currentPosition.y > 0 && IsCellEmpty(new Vector2(currentPosition.x, currentPosition.y - 1)))
            possiblePositions.Add(new Vector2(currentPosition.x, currentPosition.y - 1));
        if (currentPosition.y < rows - 1 && IsCellEmpty(new Vector2(currentPosition.x, currentPosition.y + 1)))
            possiblePositions.Add(new Vector2(currentPosition.x, currentPosition.y + 1));

        if (possiblePositions.Count > 0)
            return possiblePositions[Random.Range(0, possiblePositions.Count)];
        else
            return currentPosition;
    }

    bool IsCellEmpty(Vector2 cellPosition)
    {
        return !cellContents.ContainsKey(cellPosition) || cellContents[cellPosition].Count == 0;
    }

    private Vector2 touchStartPos;

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
            Vector2 gridAlignedTouchPosition = GetGridAlignedPosition(touchPosition);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPos = gridAlignedTouchPosition;
                    SelectIngredient(touchPosition);
                    break;
                case TouchPhase.Moved:
                    if (selectedIngredient != null && !ingredientMoved)
                    {
                        if (IsValidMove(gridAlignedTouchPosition))
                        {
                            MoveIngredient(gridAlignedTouchPosition);
                            ingredientMoved = true;
                        }
                    }
                    break;
                case TouchPhase.Ended:
                    selectedIngredient = null;
                    ingredientMoved = false;
                    break;
            }
        }
    }

    private bool ingredientMoved = false;

    [SerializeField] private LayerMask ingredientLayerMask;
    void SelectIngredient(Vector2 screenPosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(screenPosition, Vector2.zero, Mathf.Infinity, ingredientLayerMask);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Ingredient"))
            {
                selectedIngredient = hit.collider.gameObject;
            }
            else if (hit.collider.CompareTag("Bread") && levelComplete == true)
            {
                selectedIngredient = hit.collider.gameObject;
                MoveBread(hit.collider.gameObject.transform.position);
                selectedIngredient = null;
            }
        }
    }

    private Dictionary<Vector2, List<GameObject>> cellContents = new Dictionary<Vector2, List<GameObject>>();
    private List<GameObject> activeIngredients = new List<GameObject>();

    void UpdateCellContent(Vector2 position, GameObject ingredient)
    {
        if (!cellContents.ContainsKey(position))
        {
            cellContents[position] = new List<GameObject>();
        }

        if (ingredient == null)
        {
        }
        else
        {
            if (!cellContents[position].Contains(ingredient))
            {
                cellContents[position].Add(ingredient);
            }
        }
    }

    Vector2 GetGridAlignedPosition(Vector2 position)
    {
        int x = Mathf.RoundToInt(position.x);
        int y = Mathf.RoundToInt(position.y);
        return new Vector2(x, y);
    }

    bool IsWithinGridBounds(Vector2 position)
    {
        return position.x >= 0 && position.x < cols && position.y >= 0 && position.y < rows;
    }

    private int highestSortingOrder = 2;
    private bool levelComplete = false;
    private List<IngredientMove> lastMoveData = new List<IngredientMove>();

    private struct IngredientMove
    {
        public Vector2 position;
        public GameObject prefab;
    }
    void MoveIngredient(Vector2 newPosition)
    {
        if (selectedIngredient != null && !levelComplete)
        {
            Vector2 gridAlignedPosition = GetGridAlignedPosition(newPosition);

            if (IsValidMove(gridAlignedPosition))
            {
                hasUndoneLastMove = false;
                RecordLastMove();

                Vector2 originalPosition = selectedIngredient.transform.position;
                List<GameObject> ingredientsToMove = GetAllIngredientsInCell(originalPosition);

                if (cellContents.ContainsKey(originalPosition))
                {
                    cellContents[originalPosition].RemoveAll(item => ingredientsToMove.Contains(item));
                }

                foreach (GameObject ingredient in ingredientsToMove)
                {
                    ingredient.transform.position = gridAlignedPosition;
                    UpdateCellContent(gridAlignedPosition, ingredient);

                    SpriteRenderer spriteRenderer = ingredient.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.sortingOrder = ++highestSortingOrder;
                    }
                }

                if (cellContents[originalPosition].Count == 0)
                {
                    cellContents.Remove(originalPosition);
                }

                CheckForLevelCompletion();
            }
        }
    }

    void RecordLastMove()
    {
        lastMoveData.Clear();
        foreach (var kvp in cellContents)
        {
            Vector2 position = kvp.Key;
            List<GameObject> ingredients = kvp.Value;

            foreach (var ingredient in ingredients)
            {
                IngredientMove move;
                move.position = position;
                move.prefab = ingredient;

                lastMoveData.Add(move);
            }
        }
    }

    public void UndoLastMove()
    {
        if (lastMoveData.Count > 0 && !hasUndoneLastMove)
        {
            ClearCellContents();

            List<IngredientMove> breadMoveData = new List<IngredientMove>();
            List<IngredientMove> otherIngredientsMoveData = new List<IngredientMove>();

            foreach (var move in lastMoveData)
            {
                if (move.prefab != null)
                {
                    if (move.prefab.CompareTag("Bread"))
                    {
                        breadMoveData.Add(move);
                    }
                    else if (move.prefab.CompareTag("Ingredient"))
                    {
                        Collider2D[] colliders = Physics2D.OverlapPointAll(move.position);
                        foreach (var collider in colliders)
                        {
                            GameObject existingIngredient = collider.gameObject;
                            if (existingIngredient.CompareTag("Ingredient"))
                            {
                                activeIngredients.Remove(existingIngredient);
                                Destroy(existingIngredient);
                            }
                        }

                        GameObject newIngredient = Instantiate(move.prefab, move.position, Quaternion.identity);
                        newIngredient.GetComponent<BoxCollider2D>().enabled = true;
                        UpdateCellContent(move.position, newIngredient);
                        activeIngredients.Add(newIngredient);
                    }
                }
            }



            foreach (var move in breadMoveData)
            {
                if (move.prefab != null)
                {
                    move.prefab.transform.position = move.position;
                    UpdateCellContent(move.position, move.prefab);
                }
            }

            lastMoveData.RemoveAt(lastMoveData.Count - 1);

            CheckForLevelCompletion();

            hasUndoneLastMove = true;
        }
    }

    void ClearCellContents()
    {
        foreach (var kvp in cellContents)
        {
            kvp.Value.Clear();
        }
        cellContents.Clear();
    }

    List<GameObject> GetAllIngredientsInCell(Vector2 cellPosition)
    {
        List<GameObject> ingredientsInCell = new List<GameObject>();
        foreach (GameObject ingredient in activeIngredients)
        {
            if (ingredient != null)
            {
                Vector2 ingredientPosition = new Vector2(ingredient.transform.position.x, ingredient.transform.position.y);
                if (ingredientPosition == cellPosition)
                {
                    ingredientsInCell.Add(ingredient);
                }
            }
            else
            {
                ingredientsInCell.Remove(ingredient);
            }
        }
        return ingredientsInCell;
    }

    bool IsValidMove(Vector2 newPosition)
    {
        Vector2 currentPosition = new Vector2(selectedIngredient.transform.position.x, selectedIngredient.transform.position.y);

        float distance = Vector2.Distance(touchStartPos, newPosition);
        bool isAdjacentMove = distance <= 1.0f && distance > 0f;

        bool targetCellHasIngredient = IsCellOccupied(newPosition) && newPosition != currentPosition;

        return isAdjacentMove && IsWithinGridBounds(newPosition) && targetCellHasIngredient;
    }

    bool IsCellOccupied(Vector2 cellPosition)
    {
        return cellContents.ContainsKey(cellPosition) && cellContents[cellPosition].Count > 0;
    }

    void CheckForIngredientOnBread(GameObject ingredient)
    {
        Collider2D breadCollider1 = grid[(int)breadPosition1.x, (int)breadPosition1.y].GetComponent<Collider2D>();
        Collider2D breadCollider2 = grid[(int)breadPosition2.x, (int)breadPosition2.y].GetComponent<Collider2D>();

        if (breadCollider1.OverlapPoint(ingredient.transform.position) ||
            breadCollider2.OverlapPoint(ingredient.transform.position))
        {
            if (!ingredientsOnBread.Contains(ingredient))
            {
                ingredientsOnBread.Add(ingredient);
            }
        }
        else
        {
            if (ingredientsOnBread.Contains(ingredient))
            {
                ingredientsOnBread.Remove(ingredient);
            }
        }

        CheckForLevelCompletion();
    }

    void CheckForLevelCompletion()
    {
        int numberOfIngredients = Random.Range(1, (rows * cols) - 2);

        int totalMovableIngredients = activeIngredients.Count;

        int ingredientsOnBreadCount = 0;

        foreach (GameObject ingredient in activeIngredients)
        {
            Vector2 ingredientPosition = new Vector2(ingredient.transform.position.x, ingredient.transform.position.y);

            if (IsOnBread(ingredientPosition))
            {
                ingredientsOnBreadCount++;
            }
        }

        if (ingredientsOnBreadCount == totalMovableIngredients)
        {
            levelComplete = true;
        }
    }

    void MoveBread(Vector2 newPosition)
    {
        if (selectedIngredient != null && levelComplete)
        {
            Vector2 targetBreadPosition = (new Vector2(selectedIngredient.transform.position.x, selectedIngredient.transform.position.y) == breadPosition1) ? breadPosition2 : breadPosition1;

            if (newPosition == breadPosition1 || newPosition == breadPosition2)
            {
                List<GameObject> ingredientsOnMovingBread = GetAllIngredientsInCell(selectedIngredient.transform.position);

                if (cellContents.ContainsKey(selectedIngredient.transform.position))
                {
                    cellContents[selectedIngredient.transform.position].Clear();
                }

                selectedIngredient.transform.position = targetBreadPosition;
                UpdateCellContent(targetBreadPosition, selectedIngredient);

                foreach (GameObject ingredient in ingredientsOnMovingBread)
                {
                    ingredient.transform.position = targetBreadPosition;
                    UpdateCellContent(targetBreadPosition, ingredient);

                    SpriteRenderer spriteRenderer = ingredient.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.sortingOrder = ++highestSortingOrder;
                    }
                }
                selectedIngredient.GetComponent<SpriteRenderer>().sortingOrder = ++highestSortingOrder;
                Invoke("EnableCanvas", 0.1f);

            }
        }
    }

    [SerializeField] GameObject canvasFinished;
    [SerializeField] TextMeshProUGUI levelText;
    void EnableCanvas()
    {
        if (canvasFinished != null)
        {
            GameManager.Instance.currentLevel++;
            PlayerPrefs.SetInt("CurrentLevel", GameManager.Instance.currentLevel);
            levelText.text = "Finished current level " + GameManager.Instance.currentLevel;
            canvasFinished.SetActive(true);
            Invoke("ResetLevel", 2f);
        }
    }

    public void ResetLevel()
    {
        if (lastMoveData.Count > 0)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private bool hasUndoneLastMove = false;
    bool IsOnBread(Vector2 position)
    {
        return position == breadPosition1 || position == breadPosition2;
    }
}
