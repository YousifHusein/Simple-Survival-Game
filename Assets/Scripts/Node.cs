using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector3 position;
    public bool isWalkable;
    public Node parent;

    public float gCost;
    public float hCost;
    public float fCost => gCost + hCost;
}
