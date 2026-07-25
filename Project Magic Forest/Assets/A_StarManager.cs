using UnityEngine;
using System.Collections;
using System.Collections.Generic;   

public class A_StarManager : MonoBehaviour
{
  public static A_StarManager instance;

  private void Awake()
    {
        instance = this;
    }

    
    public List<Node> GeneratePath(Node start, Node end)
    {
        List<Node> openSet = new List<Node>();
        foreach(Node n in FindObjectsOfType<Node>())
        {
            n.gCost = float.MaxValue;
        }
        
        start.gCost = 0; 
        start.hCost = Vector2.Distance(start.transform.position, end.transform.position);
        openSet.Add(start);

        while(openSet.Count > 0)
        {
            int LowestF = default;
            for(int i = 1; i < openSet.Count; i++)
            {
                if(openSet[i].fscore() < openSet[LowestF].fscore())
                {
                    LowestF = i;
                }
            }
        Node currentNode = openSet[LowestF];
        openSet.Remove(currentNode);

        if(currentNode == end)
            {
                List<Node> path = new List<Node>();

                path.Insert(0, end);

                while(currentNode.cameFrom != start)
                {
                    currentNode = currentNode.cameFrom;
                    path.Add(currentNode);
                }

                path.Reverse();
                return path;
            }
            foreach(Node connectedNode in currentNode.connections)
            {
                float heldGCost = currentNode.gCost + Vector2.Distance(currentNode.transform.position, connectedNode.transform.position);

                if(heldGCost < connectedNode.gCost)
                {
                    connectedNode.cameFrom = currentNode;
                    connectedNode.gCost = heldGCost;
                    connectedNode.hCost = Vector2.Distance(connectedNode.transform.position, end.transform.position);
                    
                    if(!openSet.Contains(connectedNode))
                    {
                        openSet.Add(connectedNode);
                    }
                }


            }
        }

            
        
        return null;
        
        
    }
}
