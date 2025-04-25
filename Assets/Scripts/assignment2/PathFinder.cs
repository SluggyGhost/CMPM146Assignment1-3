using UnityEngine;
using System.Collections.Generic;

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

    // Stub
    class AStarEntry{
        public GraphNode node;
        public GraphNeighbor neighbor;
        public AStarEntry prev;
        public float heuristic;
        public float cost;
        public float sum;
    }

    public static (List<Vector3>, int) AStar(GraphNode start, GraphNode destination, Vector3 target)
    {
        List<Vector3> path = new List<Vector3>();
        int expanded = 0;

        List<AStarEntry> frontier = new List<AStarEntry>();
        Dictionary<GraphNode, float> visited = new Dictionary<GraphNode, float>();

        AStarEntry startEntry = new AStarEntry{
            node = start,
            neighbor = null,
            prev = null,
            cost = 0,
            heuristic = Vector3.Distance(start.GetCenter(), target),
        };
        startEntry.sum = startEntry.cost + startEntry.heuristic;

        InsertInOrder(frontier, startEntry);
        visited[start] = 0;

        while (frontier.Count > 0) {
            AStarEntry current = frontier[0];
            frontier.RemoveAt(0);
            expanded++;

            if(current.node == destination){
                // Reconstruct path
                AStarEntry trace = current;
                while (trace != null && trace.neighbor != null) {
                    Wall wall = trace.neighbor.GetWall();
                    Vector3 midpoint = (wall.start + wall.end) / 2;
                    path.Insert(0, midpoint);
                    trace = trace.prev;
                }
                path.Add(target);
                return (path, expanded);
            }

            foreach(GraphNeighbor neighbor in current.node.GetNeighbors()){
                GraphNode next = neighbor.GetNode();
                float newCost = current.cost + Vector3.Distance(current.node.GetCenter(), next.GetCenter());

                if (visited.ContainsKey(next) && visited[next] <= newCost){
                    continue;
                }

                visited[next] = newCost;

                float heuristic = Vector3.Distance(next.GetCenter(), target);
                AStarEntry newEntry = new AStarEntry{
                    node = next,
                    neighbor = neighbor,
                    prev = current,
                    cost = newCost,
                    heuristic = heuristic,
                    sum = newCost + heuristic
                };

                InsertInOrder(frontier, newEntry);
            }
        }

        // return path and number of nodes expanded
        return (new List<Vector3>{ target }, expanded);

    }

    // Helper function to keep frontier ordered
    private static void InsertInOrder(List<AStarEntry> frontier, AStarEntry entry){
        int index = frontier.FindIndex(e => entry.sum < e.sum);
        if(index < 0){
            frontier.Add(entry);    // insert at end
        } else {
            frontier.Insert(index,entry);   // insert before larger entry
        }
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