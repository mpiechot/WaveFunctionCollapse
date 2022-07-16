#nullable enable

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveFunctionCollapse : MonoBehaviour
{
    [SerializeField] private Tile tile;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private List<Sprite> sprites;

    private Tile[]? grid;
    private bool isUpdating;
    private int currentWidth = 0;
    private int currentHeight = 0;

    private const int BLANK = 0;
    private const int UP = 1;
    private const int RIGHT = 2;
    private const int DOWN = 3;
    private const int LEFT = 4;

    private Dictionary<int, List<List<int>>> rules = new Dictionary<int, List<List<int>>>()
    {
        {BLANK,
            new()
            {
                new() {BLANK, UP},
                new() {BLANK, RIGHT},
                new() {BLANK, DOWN},
                new() {BLANK, LEFT},
            }},
        {UP,
            new()
            {
                new() {RIGHT, DOWN, LEFT},
                new() {UP, DOWN, LEFT},
                new() {BLANK, DOWN},
                new() {UP, DOWN, RIGHT},
            }},
        {RIGHT,
            new()
            {
                new() {RIGHT, DOWN, LEFT},
                new() {UP, DOWN, LEFT},
                new() {UP, RIGHT, LEFT},
                new() {BLANK, LEFT},
            }},
        {DOWN,
            new()
            {
                new() {BLANK, UP},
                new() {UP, LEFT, DOWN},
                new() {UP, RIGHT, LEFT},
                new() {UP, RIGHT, DOWN},
            }},
        {LEFT,
            new()
            {
                new() {LEFT, DOWN, RIGHT},
                new() {BLANK, RIGHT},
                new() {LEFT, UP, RIGHT},
                new() {UP, RIGHT, DOWN},
            }},
    };

    void Start()
    {
        CreateGrid();
    }

    private void CreateGrid()
    {
        currentWidth = width;
        currentHeight = height;

        ClearGrid();

        grid = new Tile[currentHeight * currentWidth];
        for (int i = 0; i < currentHeight; i++)
        {
            for (int j = 0; j < currentWidth; j++)
            {
                grid[i * currentWidth + j] = Instantiate<Tile>(tile, new Vector3(i * tile.transform.localScale.x, j * tile.transform.localScale.y), Quaternion.identity, this.transform);
                grid[i * currentWidth + j].Initialize();
                grid[i * currentWidth + j].name = $"Tile: ({i}, {j})";
            }
        }
    }

    private void ClearGrid()
    {
        if(grid == null)
        {
            return;
        }

        for(int i = grid.Length -1 ; i >= 0; i--)
        {
            Destroy(grid[i].gameObject);
        }

        grid = null;
    }

    private void LateUpdate()
    {
        if (!isUpdating)
        {
            if(currentWidth != width || currentHeight != height)
            {
                CreateGrid();

            }

            Collapse();
            UpdatePossibilities();

            StartCoroutine(UpdateGrid());
        }
    }

    private void Collapse()
    {
        if(grid == null)
        {
            return;
        }
        List<Tile> possibleTiles = new();
        int min = int.MaxValue;
        foreach(Tile tile in grid.Where(tile => !tile.Collapsed))
        {
            if(tile.Possible.Count < min)
            {
                possibleTiles.Clear();
                min = tile.Possible.Count;
            }
            if (tile.Possible.Count == min)
            {
                possibleTiles.Add(tile);
            }
        }

        if(possibleTiles == null || possibleTiles.Count == 0)
        {
            return;
        }

        Tile collapsableTile = possibleTiles[Random.Range(0, possibleTiles.Count)];

        collapsableTile.Possible = new() { collapsableTile.Possible[Random.Range(0,collapsableTile.Possible.Count)] };
        collapsableTile.Collapsed = true;
    }

    private void UpdatePossibilities()
    {
        if(grid == null)
        {
            return;
        }

        for (int i = 0; i < currentHeight; i++)
        {
            for (int j = 0; j < currentWidth; j++)
            {
                int index = i * currentWidth + j;
                if (!grid[index].Collapsed)
                {
                    UpdateTile(i,j);
                }
            }
        }
    }

    private void UpdateTile(int i, int j)
    {
        if (grid == null)
        {
            return;
        }

        List<int> initialOptions = new() { BLANK, UP, RIGHT, DOWN, LEFT };

        int me = i * currentWidth + j;

        int up = i * currentWidth + j + 1;
        if (InRange(i,j+1))
        {
            var possibleUp = grid[up].Possible.SelectMany(option => rules[option][DOWN - 1]).ToList();
            initialOptions = initialOptions.Where(option => possibleUp.Contains(option)).ToList();
        }

        int right = (i + 1) * currentWidth + j;
        if (InRange(i+1,j))
        {
            var possibleRight = grid[right].Possible.SelectMany(option => rules[option][LEFT-1]).ToList();
            initialOptions = initialOptions.Where(option => possibleRight.Contains(option)).ToList();
        }

        int down = i * currentWidth + j - 1;
        if (InRange(i,j-1))
        {
            var possibleDown = grid[down].Possible.SelectMany(option => rules[option][UP-1]).ToList();
            initialOptions = initialOptions.Where(option => possibleDown.Contains(option)).ToList();
        }

        int left = (i - 1) * currentWidth + j;
        if (InRange(i-1,j))
        {
            var possibleLeft = grid[left].Possible.SelectMany(option => rules[option][RIGHT-1]).ToList();
            initialOptions = initialOptions.Where(option => possibleLeft.Contains(option)).ToList();
        }

        grid[me].Possible = initialOptions.Distinct().ToList();
    }

    private IEnumerator UpdateGrid()
    {
        if(grid == null)
        {
            yield break;
        }

        isUpdating = true;
        for (int i = 0; i < currentHeight; i++)
        {
            for (int j = 0; j < currentWidth; j++)
            {
                Tile current = grid[i * currentWidth + j];
                if (current != null && current.Collapsed)
                {
                    current.Renderer.sprite = sprites[current.Possible[0]];
                }
                //yield return 0;              
            }
        }
        isUpdating = false;
    }

    private bool InRange(int x, int y)
    {
        return x >= 0 && x < currentHeight && y >= 0 && y < currentWidth;
    }
}
