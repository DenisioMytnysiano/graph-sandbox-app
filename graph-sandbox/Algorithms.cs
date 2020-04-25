﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;

namespace graph_sandbox
{
    static class Algorithms
    {
        private static readonly Color activeVertex = Color.Red;
        private static readonly Color processedVertex = Color.YellowGreen;
        private static readonly Color passiveVertex = Color.White;

        private static Color[] colors = new Color[] {
            Color.Red, Color.Yellow, Color.Blue, Color.Green, Color.Gray, Color.Chocolate,
            Color.Lime, Color.Cyan, Color.Magenta, Color.Maroon, Color.Olive, Color.Purple,
            Color.Teal, Color.Wheat, Color.Indigo
        };

        public static void BFS(DrawingSurface ds, int start)
        {
            if (start > ds.Vertices.Count || start < 1) return;
            var adjList = ds.GetAdjList();
            var que = new Queue<int>();
            bool[] visited = new bool[ds.Vertices.Count];
            var g = ds.CreateGraphics();
            que.Enqueue(start - 1);
            visited[start - 1] = true;

            while(que.Count > 0)
            {
                start = que.Dequeue();
                ds.Vertices[start].ReDraw(g, activeVertex);
                ds.Invalidate();
                Thread.Sleep(700);

                visited[start] = true;
                foreach(var vertex in adjList[start])
                {
                    if (!visited[vertex])
                    {
                        ds.Vertices[vertex].FillColor = processedVertex;
                        ds.Vertices[vertex].ReDraw(g, processedVertex);
                        ds.Invalidate();
                        que.Enqueue(vertex);
                        visited[vertex] = true;
                        Thread.Sleep(700);
                    }
                }
                ds.Vertices[start].ReDraw(g, processedVertex);
            }
            g.Dispose();
            ClearVertices(ds, ds.Vertices);
        }
        public static void DFS(DrawingSurface ds, int start)
        {
            if (start > ds.Vertices.Count || start < 1) return;
            Stack<int> stack = new Stack<int>();
            stack.Push(start-1);
            var adjList = ds.GetAdjList();
            bool[] visited = new bool[ds.Vertices.Count];
            var g = ds.CreateGraphics();
            while (stack.Count>0)
            {
                int vertex = stack.Peek();
                ds.Vertices[vertex].ReDraw(g, activeVertex);
                ds.Invalidate();
                Thread.Sleep(700);
                if (!visited[vertex])
                {
                    visited[vertex] = true;
                    ds.Vertices[vertex].FillColor = processedVertex;
                    ds.Vertices[vertex].ReDraw(g, processedVertex);
                    ds.Invalidate();
                    Thread.Sleep(700);
                }
                bool remove = true;
                foreach (int adjvertex in adjList[vertex])
                {
                    if (!visited[adjvertex])
                    {
                        stack.Push(adjvertex);
                        remove = false;
                        ds.Vertices[vertex].FillColor = processedVertex;
                        ds.Vertices[vertex].ReDraw(g, processedVertex);
                        ds.Invalidate();
                        Thread.Sleep(700);
                        break;
                    }
                }
                if (remove)
                {
                    int v = stack.Pop();
                    ds.Vertices[v].FillColor = processedVertex;
                    ds.Vertices[v].ReDraw(g, processedVertex);
                    ds.Invalidate();
                    Thread.Sleep(700);
                }
            }
            g.Dispose();
            ClearVertices(ds, ds.Vertices);
        }
        public static void Colouring(DrawingSurface ds)
        {
            if (ds.Vertices.Count == 0) return;
            Random rnd = new Random { };
            var g = ds.CreateGraphics();
            var adjList = ds.GetAdjList();
            List<int> nodePowers = ds.GetNodePowers();
            List<int> vertices = Enumerable.Range(0, ds.Vertices.Count).OrderByDescending(u => nodePowers[u]).ToList();
            List<int> colors_list = Enumerable.Repeat(-1, ds.Vertices.Count).ToList();
            colors = colors.OrderBy(x => rnd.Next()).ToArray();
            int colors_used = 1;
            colors_list[vertices[0]] = 0;

            foreach(var vertex in vertices)
            {
                if (vertex == vertices[0]) continue;
                HashSet<int> adjacentColors = new HashSet<int> { };
                foreach(var adjvertices in adjList[vertex])
                {
                    if (colors_list[adjvertices] != -1)
                    {
                        adjacentColors.Add(colors_list[adjvertices]);
                    }
                }
                HashSet<int> possibleColors = colors_list.Where(u=>u!=-1).Except(adjacentColors).ToHashSet();
                if (possibleColors.Count == 0)
                {
                    colors_used += 1;
                    colors_list[vertex] = colors_used;
                }
                else
                {
                    if(colors_list[vertex]==-1)
                    {
                        colors_list[vertex] = possibleColors.Min();
                    }
                }    
            }
            foreach(var vertex in vertices)
            {  
                ds.Vertices[vertex].ReDraw(g, colors[colors_list[vertex]]);
                ds.Invalidate();
                Thread.Sleep(1000);
            }
            Thread.Sleep(5000);
            g.Dispose();
            ClearVertices(ds, ds.Vertices);
        }
        private static int[] TopologicalSort(DrawingSurface ds)
        {
            HashSet<int> visited = new HashSet<int>();
            int[] answer = new int[ds.Vertices.Count];
            Dictionary<int, List<int>> adjList = ds.GetAdjList();
            int currentPlace = ds.Vertices.Count;


            for (int vertex = 0; vertex < ds.Vertices.Count; ++vertex)
            {
                if (!visited.Contains(vertex))
                {
                    DFS(vertex);
                }
            }

            void DFS(int start)
            {
                visited.Add(start);

                foreach (var vertex in adjList[start])
                {
                    if (!visited.Contains(vertex))
                    {
                        DFS(vertex);
                    }
                }
                answer[--currentPlace] = start;
            }
            return answer;
        }

        public static void ConnectedComponents(DrawingSurface ds)
        {
            var g = ds.CreateGraphics();
            int[] sortedVertices = TopologicalSort(ds);
            var OldGraphEdges = new List<Edge>();
            for(var i =0; i < ds.Edges.Count; i++)
            {
                OldGraphEdges.Add(ds.Edges[i]);
            }
            foreach(var edge in ds.Edges)
            {
                var tmp = edge.start;
                edge.setStart(ds.Vertices[edge.end]);
                edge.setEnd(ds.Vertices[tmp]);
            }
            Dictionary<int, List<int>> adjList = ds.GetAdjList();
            HashSet<int> visited = new HashSet<int>();
            List<int> component = new List<int>();
            List<List<int>> componentsList = new List<List<int>>();
            foreach (var vertex in sortedVertices)
            {
                if (!visited.Contains(vertex))
                {
                    DFS(vertex);
                }
                if (component.Count != 0)
                {
                    componentsList.Add(new List<int>(component));
                    component.Clear();
                }
            }

            void DFS(int start)
            {
                visited.Add(start);
                component.Add(start);

                foreach (var vertex in adjList[start])
                {
                    if (!visited.Contains(vertex))
                    {
                        DFS(vertex);
                    }
                }
            }
            foreach (var edge in ds.Edges)
            {
                var tmp = edge.start;
                edge.setStart(ds.Vertices[edge.end]);
                edge.setEnd(ds.Vertices[tmp]);
            }
            Random rnd = new Random();
            foreach (var c in componentsList)
            {
               
                
                Color colorForCurrentComponent = Color.FromArgb(255, rnd.Next(255), rnd.Next(255), rnd.Next(255));
                foreach(var el in c)
                {
                    ds.Vertices[el].ReDraw(g, colorForCurrentComponent);
                }
                for (int i = 0; i < ds.Vertices.Count; i++)
                {
                    for(int j = 0; j < ds.Vertices.Count; j++)
                    {
                        
                        if (i != j)
                        {
                           
                            for(int k = 0; k < ds.Edges.Count; k++)
                            {
                                if(((ds.Edges[k].start == i && ds.Edges[k].end == j) || (ds.Edges[k].start == j && ds.Edges[k].end == i))&&c.Contains(i)&&c.Contains(j))
                                {
                                    
                                    ds.Edges[k].FillColor = colorForCurrentComponent;
                                    ds.Edges[k].Draw(g);
                                }
                            }
                        }
                       
                    }
                }
            }
            
            Thread.Sleep(5000);
            ClearEdges(ds, ds.Edges);
            ClearVertices(ds, ds.Vertices);
            ds.Invalidate();
        }
        
        private static void ClearVertices(DrawingSurface ds,List<Circle> vertices)
        {
            for(var i = 0; i < vertices.Count; ++i)
            {
                vertices[i].FillColor = Color.White;
            }
            ds.Invalidate();
        }
        private static void ClearEdges(DrawingSurface ds, List<Edge> edges)
        {
            for(var i = 0; i < ds.Edges.Count; i++)
            {
                edges[i].FillColor = Color.Gray;
            }
            ds.Invalidate();
        }
    }
}
