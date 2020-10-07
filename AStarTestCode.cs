using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Playground
{
    public readonly int m_kStartPosX = 10;
    public readonly int m_kStartPosY = 28;
    public int m_playgroundSizeX { get; private set; }
    public int m_playgroundSizeY { get; private set; }
    public Tile[,] m_playgroundTiles;
}

public class Tile : MonoBehaviour
{
    public string m_fileName{get; private set;}
    public int m_xPos{get; private set;}
    public int m_yPos{get; private set;}
}

public static class AStar
{
    public Playground m_playGround;

    private class Node
    {
        public Node m_parentNode;
        public Tile m_thisTile;
        public List<Node> m_childrenNode = new List<Node>();
        public double F = 0; // G + H
        public double G = 0; // BeginNode -> ThisNode
        public double H = 0; // ThisNode -> EndNode

        public Node(in Tile kThisTile, in Node kParentNode = null)
        {
            m_thisTile = kThisTile;
            m_parentNode = kParentNode;
        }
    }

    public static List<Tile> GetPathList(in Tile kStartTile, in Tile kEndTile)
    {
        List<Node> openedList = new List<Node>();
        List<Node> closedList = new List<Node>();
        List<Tile> pathList = new List<Tile>();

        Node endNode = new Node(kEndTile);
        Node beginNode = new Node(kStartTile);
        beginNode.H = Math.Round(Vector2.Distance(kStartTile.transform.position, kEndTile.transform.position), 2);
        beginNode.F = beginNode.H;

        openedList.Add(beginNode);

        while (openedList.Count != 0)
        {
            double f = openedList[0].F;
            int index = 0;
            for( int i = 0; i < openedList.Count; i++ ) // Find Cheapest F from openedList => it called SelTile.
            {
                if( openedList[i].F < f )
                {
                    f = openedList[i].F;
                    index = i;
                }
            }

            closedList.Add(openedList[index]);
            openedList.RemoveAt(index);

            var lastNode = closedList[closedList.Count - 1]; // lastNode is SelTile.

            if (lastNode.m_childrenNode.Count == 0)
                FindChildren(ref lastNode); // Find SelTile's children.
            
            foreach(var child in lastNode.m_childrenNode)
            {
                foreach(var node in closedList)
                {
                    if(node.m_thisTile == child.m_thisTile)
                    {
                        goto EndOfChildrenLoop; // This tile is in closed list. skip.
                    }
                }

                foreach (var node in openedList)
                {
                    if (node.m_thisTile == child.m_thisTile)
                    {
                        double dist = Math.Round(Vector2.Distance(lastNode.m_thisTile.transform.position, child.m_thisTile.transform.position), 2);
                        if( (lastNode.G + dist) < child.G )
                        {
                            child.m_parentNode = lastNode;
                            child.G = lastNode.G + dist;
                            child.H = Math.Round(Vector2.Distance(child.m_thisTile.transform.position, kEndTile.transform.position), 2);
                            child.F = child.G + child.H;
                            //SetTileDebugText(child.m_thisTile, child.F, child.G, child.H);
                        }

                        goto EndOfChildrenLoop; // This tile is in opened list. skip.
                    }
                }

                // This tile is not in any list. add it to opened list.
                child.m_parentNode = lastNode;
                child.G = lastNode.G + Math.Round(Vector2.Distance(lastNode.m_thisTile.transform.position, child.m_thisTile.transform.position), 2);
                child.H = Math.Round(Vector2.Distance(child.m_thisTile.transform.position, kEndTile.transform.position), 2);
                child.F = child.G + child.H;
                //SetTileDebugText(child.m_thisTile, child.F, child.G, child.H);
                openedList.Add(child);

EndOfChildrenLoop:
                if (child.m_thisTile == endNode.m_thisTile)
                {
                    endNode.m_parentNode = child.m_parentNode;
                    goto EndOfPathfinding;
                }
            }
        }

        if( pathList.Count == 0 )
            return null;

EndOfPathfinding:
        pathList.Add(endNode.m_thisTile);
        Node next = endNode.m_parentNode;
        while (next != null && next.m_thisTile != beginNode.m_thisTile)
        {
            pathList.Insert(0, next.m_thisTile);
            next = next.m_parentNode;
        }

        return pathList; // END OF FUNCTION.

        

        Node GetNodeOnDirectionIfAvailable(in Tile kTile, in Direction kDirection)
        {
            var selectedTile = Utility.GetPlayGroundTileOnDirection(kTile, kDirection);
            if (selectedTile == null)
                return null;

            foreach(var closeNode in closedList)
            {
                if (closeNode.m_thisTile == selectedTile)
                    return null;
            }

            foreach(var openNode in openedList)
            {
                if (openNode.m_thisTile == selectedTile)
                    return openNode;
            }

            if(IsBlocked(selectedTile) || !IsCanPassThroughNextTile(kTile, kDirection) )
            {
                return null;
            }

            return new Node(selectedTile);
        }

        void FindChildren(ref Node kNode)
        {
            for (int i = 0; i < 8; i++)
            {
                var child = GetNodeOnDirectionIfAvailable(kNode.m_thisTile, (Direction)i);
                if ( child != null && (child.m_parentNode == null || child.m_parentNode.G > kNode.G) )
                {
                    child.m_parentNode = kNode;
                    kNode.m_childrenNode.Add(child);
                }
            }
        }
    }

    private static bool IsAdjacentTile(in Tile kStandardTile, in Tile kTargetTile)
    {
        foreach(Direction dir in Enum.GetValues(typeof(Direction)))
        {
            if (Utility.GetPlayGroundTileOnDirection(kStandardTile, dir) == kTargetTile)
                return true;
        }

        return false;
    }

    private static bool IsBlocked(in Tile kTile)
    {
        if (kTile.GetSpriteRenderer().sprite.name == "BLOCKED")
            return true;

        return false;
    }

    private static bool IsCanPassThroughNextTile(in Tile kStandardTile, in Direction kDir)
    {
        List<Direction> kNeedCheckDirList = new List<Direction> { Direction.UP, Direction.DOWN, Direction.LEFT, Direction.RIGHT };
        foreach (var dir in kNeedCheckDirList)
        {
            if (dir == kDir)
            {
                if (IsBlocked(Utility.GetPlayGroundTileOnDirection(kStandardTile, Utility.GetNextEnumValue(kDir))) ||
                    IsBlocked(Utility.GetPlayGroundTileOnDirection(kStandardTile, Utility.GetPrevEnumValue(kDir))))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        return true;
    }

    private static void SetTileDebugText(Tile tile, double F, double G, double H)
    {
        tile.GetComponentInChildren<Text>().text = $"{tile.transform.name}\nF : {F}\nG : {G}\nH : {H}";
    }
}