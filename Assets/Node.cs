using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Node
{
    public Vector3 position;
    public List<Bud> buds;
    

    public Node(Vector3 pos)
    {
        position = pos;
    }

    public void addBud(Bud bud)
    {
        buds.Add(bud);
    }
}

public class Internode
{
    public Node node1;
    public Node node2;
    public bool trunk = false;
    public int order = 0;

    public Internode(Node one, Node two)
    {
        node1 = one;
        node2 = two;
    }

    public void isTrunk()
    {
        trunk = true;
    }

    public void setOrder(int inOrder)
    {
        order = inOrder;
    }
}

public class Bud
{
    public Node node;
    public Vector3 budDirection;
    public float dieProb;
    public float pauseProb;
    public float growProb;
    public int depth;
    public int order; // axis order
    public bool isDead;

    public Bud(Node inNode, Vector3 dir, float die, float pause, float grow, int inDepth, int orderIn, bool dead)
    {
        node = inNode;
        budDirection = dir;
        dieProb = die;
        pauseProb = pause;
        growProb = grow;
        depth = inDepth;
        order = orderIn;
        isDead = dead;
    }
}

public class Apical
{
    public Node node;
    public Vector3 budDirection;
    public float dieProb;
    public float pauseProb;
    public float growProb;
    public float branchProb;
    public bool isTrunk; // we'll want to know if it's trunk to grow differently and make sure at least 1 bud always goes up
    public int order; // axis order
    public bool isDead;

    public Apical(Node inNode, Vector3 dir, float die, float pause, float grow, float branch, bool trunk, int orderIn, bool dead)
    {
        node = inNode;
        budDirection = dir;
        dieProb = die;
        pauseProb = pause;
        growProb = grow;
        branchProb = branch;
        isTrunk = trunk;
        order = orderIn;
        isDead = dead;
    }
}
