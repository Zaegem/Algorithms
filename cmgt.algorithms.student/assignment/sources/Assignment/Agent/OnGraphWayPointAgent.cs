using GXPEngine;
using System;
using System.Collections.Generic;

/**
 * Very simple example of a nodegraphagent that walks directly to the node you clicked on,
 * ignoring walls, connections etc.
 */
class OnGraphWayPointAgent : NodeGraphAgent
{
    // List to hold the path of nodes to move towards
    private List<Node> _path = new List<Node>();
    // Index to track the current target in the path
    private Node CurrentNodePosition;
    private int index;

    public OnGraphWayPointAgent(NodeGraph pNodeGraph) : base(pNodeGraph)
    {
        SetOrigin(width / 2, height / 2);

        // Position ourselves on a random node
        if(pNodeGraph.nodes.Count > 0)
        {
            Node startNode = pNodeGraph.nodes[Utils.Random(0, pNodeGraph.nodes.Count)];
            jumpToNode(startNode);
            CurrentNodePosition = startNode;
        }

        // Listen to node clicks
        pNodeGraph.OnNodeLeftClicked += onNodeClickHandler;
    }

    protected virtual void onNodeClickHandler(Node pNode)
    {
        // Find path from current node to the clicked node
        _path = FindPath(CurrentNodePosition, pNode);
        if(_path != null)
        {
            
            index = 0;
            Console.WriteLine("New path set.");
        } else
        {
            Console.WriteLine("No path found to the clicked node.");
        }
    }

    protected override void Update()
    {
        // No path or reached the end of the path? Don't walk
        if (_path == null || index >= _path.Count)
        {
            return;
        }

        // Get the current target node
        Node currentIndexPosition = _path[index];

        // Move towards the current target node, if we reached it, move to the next target
        if(moveTowardsNode(currentIndexPosition))
        {
            // Update CurrentNodePosition to the new node
            CurrentNodePosition = currentIndexPosition;
            index++;
        }
    }

    private List<Node> FindPath(Node startNode, Node endNode)
    {
        // Dictionary to store the path
        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
        List<Node> queue = new List<Node> { startNode };
        cameFrom[startNode] = null;

        while(queue.Count > 0)
        {
            // Remove the first node from the queue, so I dont have to check it again.
            Node current = queue[0];
            queue.RemoveAt(0);

            // Check all connections of the current node
            foreach(Node next in current.connections)
            {
                // If the next node has not been visited yet
                if(!cameFrom.ContainsKey(next))
                {
                    queue.Add(next);
                    cameFrom[next] = current;

                    // If we found the end node, construct the path and return it
                    if(next == endNode)
                    {
                        List<Node> path = new List<Node>();
                        for(Node step = endNode; step != null; step = cameFrom[step])
                        {
                            path.Add(step);
                        }
                        path.Reverse();
                        return path;
                    }
                }
            }
        }

        // If we exit the loop without finding the endNode, return null
        return null;
    }
}
