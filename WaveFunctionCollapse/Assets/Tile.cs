using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool Collapsed { get; set; } = false;

    public List<int> Possible { get; set; } = new(){ 0, 1, 2, 3, 4};

    public SpriteRenderer Renderer { get; private set; }

    public void Initialize()
    {
        Renderer = GetComponent<SpriteRenderer>();
    }
}
