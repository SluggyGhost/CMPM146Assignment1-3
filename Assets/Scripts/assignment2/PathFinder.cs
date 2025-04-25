using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PathFinder : MonoBehaviour
{
    // Assignment 2: Implement AStar
    //
    // DO NOT CHANGE THIS SIGNATURE (parameter types + return type)
    // AStar will be given the start node, destination node and the target position, and should return 
    // a path as a list of positions the agent has to traverse to reach its destination, as well as the
    // number of nodes that were expanded to find this path
    // The last entry of the path will be the target position, and you can also use it to calculate the heuristic
    // value of nodes you add to your search frontier; the number of expanded nodes tells us if your search was
    // efficient
    //
    // Take a look at StandaloneTests.cs for some test cases
    private class Frontier {
        private List<(GraphNode node, float priority)> elements = new();

        public int Count => elements.Count;

        public void Enqueue(GraphNode node, float priority){
            elements.Add((node, priority));
        }

        public GraphNode Dequeue() {
            int bestIndex = 0;
            for(int i = 1; i < elements.Count; i++){
                if(elements[i].priority < elements[bestIndex].priority){
                    bestIndex = i;
                }
            }
            GraphNode bestItem = elements[bestIndex].node;
            elements.RemoveAt(bestIndex);
            return bestItem;
        }

        public bool Contains(GraphNode node){
            return elements.Any(e => e.node == node);
        }

        public void UpdatePriority(GraphNode node, float new Priority) {
            for (int i = 0; i < elements.Count; i++) {
                if(elements[i].node == node) {
                    elements[i] = (node, newPriority);
                    return;
                }
            }
        }
    }
    
    public static (List<Vector3>, int) AStar(GraphNode start, GraphNode destination, Vector3 target){
        var frontier = new Frontier();
        var cameFrom = new Dictionary<GraphNode, GraphNode>();
        var gScore = new Dictionary<GraphNode, float>();
        var fScore = new Dictionary<GraphNode, float>();
        var expandedNodes = 0;

        foreach (var node in start.graph.all_nodes)
        {
            gScore[node] = float.PositiveInfinity;
            fScore[node] = float.PositiveInfinity;
        }

        gScore[start] = 0f;
        fScore[start] = Heuristic(start, target);
        frontier.Enqueue(start, fScore[start]);

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            expandedNodes++;

            if (current == destination)
                return (ReconstructPath(cameFrom, current, target), expandedNodes);

            foreach (var neighbor in current.neighbors)
            {
                float tentativeG = gScore[current] + Vector3.Distance(current.GetPosition(), neighbor.GetPosition());

                if (tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, target);

                    if (!frontier.Contains(neighbor))
                        frontier.Enqueue(neighbor, fScore[neighbor]);
                    else
                        frontier.UpdatePriority(neighbor, fScore[neighbor]);
                }
            }
        }

        return (new List<Vector3>(), expandedNodes); // No path found
    }

    private static float Heuristic(GraphNode node, Vector3 target)
    {
        return Vector3.Distance(node.GetPosition(), target);
    }

    private static List<Vector3> ReconstructPath(Dictionary<GraphNode, GraphNode> cameFrom, GraphNode current, Vector3 target)
    {
        var path = new List<Vector3> { target };
        while (cameFrom.ContainsKey(current))
        {
            path.Insert(0, current.GetPosition());
            current = cameFrom[current];
        }
        return path;
    }

    public Graph graph;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EventBus.OnTarget += PathFind;
        EventBus.OnSetGraph += SetGraph;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetGraph(Graph g)
    {
        graph = g;
    }

    // entry point
    public void PathFind(Vector3 target)
    {
        if (graph == null) return;

        // find start and destination nodes in graph
        GraphNode start = null;
        GraphNode destination = null;
        foreach (var n in graph.all_nodes)
        {
            if (Util.PointInPolygon(transform.position, n.GetPolygon()))
            {
                start = n;
            }
            if (Util.PointInPolygon(target, n.GetPolygon()))
            {
                destination = n;
            }
        }
        if (destination != null)
        {
            // only find path if destination is inside graph
            EventBus.ShowTarget(target);
            (List<Vector3> path, int expanded) = PathFinder.AStar(start, destination, target);

            Debug.Log("found path of length " + path.Count + " expanded " + expanded + " nodes, out of: " + graph.all_nodes.Count);
            EventBus.SetPath(path);
        }
    }
}
