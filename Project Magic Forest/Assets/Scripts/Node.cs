using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Node : MonoBehaviour
{
    public Node cameFrom;
    public List<Node> connections;
    public float gCost;
    public float hCost;

    public float fscore()
    {
        return gCost + hCost;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        if(connections.Count > 0)
        {
            for(int i = 0; i < connections.Count; i++)
            {
                Node connection = connections[i];
                {
                    Gizmos.DrawLine(transform.position, connection.transform.position);
                }
            }
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
