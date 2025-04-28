using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class NavMesh : MonoBehaviour
{
    // implement NavMesh generation here:
    //    the outline are Walls in counterclockwise order
    //    iterate over them, and if you find a reflex angle
    //    you have to split the polygon into two
    //    then perform the same operation on both parts
    //    until no more reflex angles are present
    //
    //    when you have a number of polygons, you will have
    //    to convert them into a graph: each polygon is a node
    //    you can find neighbors by finding shared edges between
    //    different polygons (or you can keep track of this while 
    //    you are splitting)

    class Polygon {

        public List<Wall> walls;
        public (Polygon, Polygon) SplitPolygon (int a, int b){
            if (a > b) return SplitPolygon(b, a);
            List<Wall> aWall = walls.getRange(0, a+1);
            List<Wall> bWall = walls.getRange(a+1, b-a);
            Vector3 splitPoint1 = aWalls[aWalls.Count-1].end;
            Vector3 splitPoint2 = bWalls[bWalls.Count-1].end;
            aWalls.Add(new Wall(splitPoint1, splitPoint2));
            bWalls.Add(new Wall(splitPoint2, splitPoint1));
            aWalls.AddRange(walls.GetRange(b+1, walls.Count-b-1));
            return (new Polygon(aWalls), new Polygon(bWalls));
        }
        public Polugon(List<Wall> w) {
            this.walls = w;
        }
    }

    static int findNonConvexCornerIndex(Polygon p) {
        List<Wall> walls = p.walls;
        for (int i = 0; i < walls.Count-1; i++) {
            Wall currentWall = walls[i];
            Wall nextWall = walls[(i+1)%walls.Count];
            if (Vector3.Dot(currentWall.normal, nextWall.direction) < 0) {
                return i;
            }
        }
        return -1;
    }

    static int findNextSplitPoint (Polygon p, int splitPoint) {
        // PSEUDOCODE
        int offset = p.walls.Count/2;
        for (int i = 0; i < p.walls.Count; i++) {
            int currentWallIndex = (i + offset + splitPoint)%p.walls.Count;
            if (Math.Abs(currentWallIndex - splitPoint) < 2) continue;
            Wall newVector = p.walls[currentWallIndex].end - p.walls[splitPoint].end;
            if (Vector3.Dot(p.walls[splitPoint], newVector) < 0) continue;
            foreach(Wall wall in p.walls) {
                if (wall.Crosses(p.walls[currentWallIndex].end - p.walls[splitPoint].end)) {
                    crossed = true;
                }
            }
            if (!crossed) {
                return currentWallIndex;
            }
        }
        return -1;
    }

    public Graph MakeNavMesh(List<Wall> outline)
    {
        // Find the non-convex corner
        // Find the second split point
        // split the polygon
        // build the graph

        List<Polygon> polygons = new List<Polygon>();
        Polygon initPolygon = new Polygon(outline);
        polygons.Add(initPolygon);
        
        bool done = false;
        int i = 0;
        while (!done) {
            if (i >= polygons.Count) break;

            Polygon currentPolygon = polygons[i];
            int nonConvexCornerIndex = -1;
            // Find the non-convex corner
            nonConvexCornerIndex = findNonConvexCornerIndex(currentPolygon);
            if (nonConvexCornerIndex != -1) {
                // Find the 2nd split point
                int nextSplitPoint = findNextSplitPoint(currentPolygon, nonConvexCornerIndex);

                // Split the polygon
                var(a, b) = SplitPolygon();
                polygons.RemoveAt(i);
                polygons.Add(a);
                polygons.Add(b);
            } else {
                i++;
            }
            if (i == polygons.Count) {
                done = true;
            }
        }
        // for (int i = 0; i < polygons.Count; i++) {

        // }


        // build the graph
        List<GraphNode> nodes = new List<GraphNode>();
        int idGenerator = 0;
        foreach (Polygon p in polygons) {
            nodes.Add(new GraphNode(idGenerator, p.walls));
            idGenerator += 1;
        }

        buildNeighbors(nodes);
        Graph g = new Graph();
        g.outline = outline;
        g.all_nodes = nodes;
        return g;
    }

    static void buildNeighbors(List<GraphNode> nodes) {
        foreach (GraphNode a in nodes) {
            foreach (GraphNode b in nodes) {
                if (a.getID() == b.getID()) continue;
                List<Wall> aWalls = a.getPolygon();
                List<Wall> bWalls = b.getPolygon();
                for (int i = 0; i < aWalls.Count; i++) {
                    for (int j = 0; j < bWalls.Count; j++) {
                        if(aWalls[i].Same(bWalls[j])) {
                            a.AddNeighbor(b, i);
                            b.AddNeighbor(a, j);
                        }
                    }
                }
            }
        }
    }

    List<Wall> outline;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EventBus.OnSetMap += SetMap;
    }

    // Update is called once per frame
    void Update()
    {
       

    }

    public void SetMap(List<Wall> outline)
    {
        Graph navmesh = MakeNavMesh(outline);
        if (navmesh != null)
        {
            Debug.Log("got navmesh: " + navmesh.all_nodes.Count);
            EventBus.SetGraph(navmesh);
        }
    }

    


    
}
