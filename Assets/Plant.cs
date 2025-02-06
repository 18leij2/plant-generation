using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class Plant : MonoBehaviour
{
    // variables to randomize the base size of trunk and number of branches
    public float[][] trees = new float[4][];
    public float[] trunkHeight = new float[4];
    public float[] trunkRadius = new float[4];
    public int[] numBranches = new int[4];
    public int seed;
    public int growthTime;
    public List<Internode> internodes = new List<Internode>();
    public List<Bud> budList = new List<Bud>();
    public List<Bud> tempBuds = new List<Bud>();
    public float branchProb = 0.6f;
    public Material trunk;
    public Material tree;
    public Material treeColor;
    public Material randomTree;

    private void Start()
    {
        growthTime = 10;
        Random.InitState(seed); 

        for (int i = 0; i < 4; i++)
        {
            trees[i] = new float[3];
            trunkHeight[i] = (int)Mathf.Floor(Random.value * 2) + 1; // trunk and branch height between 1 and 3
            trunkRadius[i] =  (Random.value * 0.3f) + 0.3f; // trunk and branch height between 0.3 and 0.6
            numBranches[i] = (int)Mathf.Floor(Random.value * 2) + 1; // 1-3 branches
            // put them into the trees array
            trees[i][0] = trunkHeight[i];
            trees[i][1] = trunkRadius[i];
            trees[i][2] = numBranches[i];
        }

        generatePlant(gameObject.transform.position);
        // generatePlant(new Vector3(10, 0, 0));
        // generatePlant(new Vector3(20, 0, 0));

        for (int i = 0; i < internodes.Count; i++)
        {
            Mesh treeMesh;
            if (internodes[i].trunk)
            {
                treeMesh = createTreeMesh(internodes[i].node1.position, internodes[i].node2.position, 0.2f);
            } else
            {
                treeMesh = createTreeMesh(internodes[i].node1.position, internodes[i].node2.position, 0.08f);
            }
            

            GameObject treeObject = new GameObject("Tree");
            MeshFilter meshFilter = treeObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = treeObject.AddComponent<MeshRenderer>();

            meshFilter.mesh = treeMesh;

            // this is to differentiate the trunk from any other axis order
            if (internodes[i].trunk)
            {
                meshRenderer.material = trunk;
            }
            else
            {
                // randomized color for the tree branches
                float num = Random.Range(0, 3);
                switch (num)
                {


                    case 0:
                        meshRenderer.material = tree;
                        break;
                    case 1:
                        meshRenderer.material = randomTree;
                        break;
                    case 2:
                        meshRenderer.material = treeColor;
                        break;
                }
            }
        }
    }

    // alternate tool to visualize trunks and branches without having to render meshes
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < internodes.Count; i++)
        {
            Gizmos.DrawLine(internodes[i].node1.position, internodes[i].node2.position);
        }
    }

    public Mesh createTreeMesh(Vector3 node1, Vector3 node2, float rad)
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[24 * 2 + 2]; // 24 points around top and bottom face each, as well as 2 more verts for center points
        int[] triangles = new int[24 * 6 + 24 * 6]; // the number of faces to map to vertices

        Vector3 direction = node2 - node1;
        direction.Normalize();
        
        // find the orientation of the shape to generate
        Vector3 upVector = Vector3.up;
        Vector3 rightVector = Vector3.Cross(direction, upVector).normalized;
        // this checks if the direction we're using is too close to the up vector (which is true for initial apical buds)
        if (rightVector.magnitude < 0.01f)
        {
            // we cross it with right instead since it will be far enough
            rightVector = Vector3.Cross(direction, Vector3.right).normalized;
        }
        
        // find another vector orthogonal to both our original vector and the calculated vector
        Vector3 forward = Vector3.Cross(direction, rightVector).normalized;

        // calculate the vertex positions
        for (int i = 0; i < 24; i++)
        {
            float angle = (i / (float)24) * 2 * Mathf.PI;
            float x = Mathf.Cos(angle) * rad;
            float z = Mathf.Sin(angle) * rad;

            // bottom
            Vector3 rightOffset = rightVector * x;
            Vector3 forwardOffset = forward * z;
            vertices[i] = node1 + rightOffset + forwardOffset;
            // top
            vertices[i + 24] = node2 + rightOffset + forwardOffset;
        }

        // center vertices
        vertices[24 * 2] = node1; // bottom
        vertices[24 * 2 + 1] = node2; // top

        // calculate triangles
        for (int i = 0; i < 24; i++)
        {
            // mod so we don't go out of bounds
            int next = (i + 1) % 24;

            // triangles on the curved side of cylinder shape
            triangles[i * 6] = i; 
            triangles[i * 6 + 1] = next;
            triangles[i * 6 + 2] = i + 24;

            triangles[i * 6 + 3] = next;
            triangles[i * 6 + 4] = next + 24;
            triangles[i * 6 + 5] = i + 24;
        }

        // triangles for the bottom face
        for (int i = 0; i < 24; i++)
        {
            int next = (i + 1) % 24;
            int baseIndex = 24 * 6;

            triangles[baseIndex + i * 3] = i;
            triangles[baseIndex + i * 3 + 1] = next;
            triangles[baseIndex + i * 3 + 2] = 24 * 2;
        }

        for (int i = 0; i < 24; i++)
        {
            int next = (i + 1) % 24;
            int baseIndex = 24 * 6 + 24 * 3;

            triangles[baseIndex + i * 3] = 24 * 2 + 1; 
            triangles[baseIndex + i * 3 + 1] = i + 24; 
            triangles[baseIndex + i * 3 + 2] = next + 24;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    // generate the plant
    public void generatePlant(Vector3 location)
    {
        budList.Clear();
        // internodes.Clear();
        // assume we pass first time step, since branching will over ever happen after trunk grows a bit
        // each node will have 3 buds, but may branch less due to the die and pause probability
        Node rootNode = new Node(location);
        Bud apicalBud = new Bud(rootNode, new Vector3(0, 1, 0), 0.3f, 0.2f, 0.3f, 0, 1, false);
        // rootNode.addBud(apicalBud);
        budList.Add(apicalBud);

        for (int i = 0; i < 30; i++)
        {
            tempBuds.Clear();
            for (int j = 0; j < budList.Count; j++)
            {
                Debug.Log(budList.Count);
                Debug.Log(budList[j].isDead);
                Bud bud = budList[j];
                if (!bud.isDead)
                {
                    // death, pause, and growth probabilities, all of which start either increasing or decreasing as the tree branches out with axis order
                    float deathProbability = Mathf.Clamp(0.2f +((bud.order - 1) * 0.1f), 0f, 1f);

                    float pauseProbability = Mathf.Clamp(0.1f + ((bud.order - 1) * 0.05f), 0f, 1f);

                    float growthProbability = Mathf.Clamp(0.6f - ((bud.order - 1) * 0.05f), 0f, 1f);

                    if (Random.value < deathProbability)
                    {
                        bud.isDead = true;
                    }
                    
                    // limit bud order and depth to stop tree from potentially infinitely growing
                    else if (Random.value > pauseProbability && bud.order <= 3 && bud.depth <= 10)
                    {
                        Internode tempInter = createInternode(bud);
                        createApical(bud, tempInter.node2);
                        if (Random.value < growthProbability)
                        {
                            createSideBranch(bud, bud.node);
                        }
                    }
                }
            }

            // add to the list afterwards to avoid modifying list mid loop
            budList.AddRange(tempBuds);
        }
        
    }

    // create internode helper method that helps to track which nodes to create shapes at
    Internode createInternode(Bud bud)
    {
        Node newNode = new Node(bud.node.position + bud.budDirection);
        Internode newInter = new Internode(bud.node, newNode);
        if (bud.order == 1 && bud.budDirection.normalized == new Vector3(0, 1, 0))
        {
            newInter.isTrunk();
        }
        newInter.setOrder(bud.order);
        internodes.Add(newInter);
        return newInter;
    }

    // creates an apical bud
    void createApical(Bud bud, Node node)
    {
        Bud apical = new Bud(node, bud.budDirection, 0.3f, 0.2f, 0.3f, bud.depth + 1, 1, false);
        bud.isDead = true;
        // node.addBud(apical);
        tempBuds.Add(apical);
    }

    // create a side branch, which adds a new apical bud
    void createSideBranch(Bud bud, Node node)
    {
        bud.budDirection.Normalize();
        
        // deviation angle specifies how far a branch can differ from original apical bud direction
        // as bud order increases the deviation increases
        float deviationAngle = 20f + (bud.order - 1) * 20;

        float angle1 = Random.Range(0f, Mathf.PI * 2); // get random spherical coords and make new direction from that
        float angle2 = Random.Range(0f, Mathf.PI);

        float x = Mathf.Cos(angle1) * Mathf.Sin(angle2);
        float y = Mathf.Sin(angle1) * Mathf.Sin(angle2);
        float z = Mathf.Cos(angle2);

        Vector3 randomDirection = new Vector3(x, y, z).normalized;

        float blendF = Mathf.Deg2Rad * deviationAngle;
        // interpolate between vectors
        Vector3 sideDirection = Vector3.Slerp(bud.budDirection, randomDirection, blendF);

        // make new bud off based on branching direction
        Bud sideApicalBud = new Bud(node, sideDirection, 0.3f, 0.2f, 0.3f, bud.depth + 1, bud.order + 1, false);
        tempBuds.Add(sideApicalBud);

        Debug.Log("Side Direction: " + sideDirection);
        Debug.Log("side apical bud: " + sideApicalBud.isDead);
    }
}
