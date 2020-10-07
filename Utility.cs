using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public enum Direction
{
    UP = 0,
    RIGHTUP = 1,
    RIGHT = 2,
    RIGHTDOWN = 3,
    DOWN = 4,
    LEFTDOWN = 5,
    LEFT = 6,
    LEFTUP = 7
}

public static class Utility
{
    public static Tile GetPlayGroundTileOnDirection(in Tile kStandardTile, in Direction kDirection)
    {
        int[] directionArr;
        switch (kDirection)
        {
            case Direction.UP:          directionArr = new int[] { -1, -1 }; break;
            case Direction.RIGHTUP:     directionArr = new int[] { 0, -1 }; break;
            case Direction.RIGHT:       directionArr = new int[] { 1, -1 }; break;
            case Direction.RIGHTDOWN:   directionArr = new int[] { 1, 0 }; break;
            case Direction.DOWN:        directionArr = new int[] { 1, 1 }; break;
            case Direction.LEFTDOWN:    directionArr = new int[] { 0, 1 }; break;
            case Direction.LEFT:        directionArr = new int[] { -1, 1 }; break;
            case Direction.LEFTUP:      directionArr = new int[] { -1, 0 }; break;
            default:                    directionArr = new int[] { 0, 0 }; break;
        }

        var newCoord = ConvertGlobalCoordToPlaygroundCoord(kStandardTile.m_xPos + directionArr[0], kStandardTile.m_yPos + directionArr[1]);
        var newX = newCoord[0];
        var newY = newCoord[1];

        if (newX < 0 || AStar.m_playGround.m_playgroundSizeX <= newX || 
            newY < 0 || AStar.m_playGround.m_playgroundSizeY <= newY)
            return null;

        return AStar.m_playGround.m_playgroundTiles[newX,newY];
    }

    public static Direction GetDirectionToTile(in Tile kFromTile, in Tile kToTile)
    {
        int xDiff = kToTile.m_xPos - kFromTile.m_xPos;
        int yDiff = kToTile.m_yPos - kFromTile.m_yPos;

        int checker = (xDiff * 10) + yDiff;

        switch (checker)
        {
            case (-10 -1) : return Direction.UP;
            case (0   -1) : return Direction.RIGHTUP;
            case (10  -1) : return Direction.RIGHT;
            case (10  +0) : return Direction.RIGHTDOWN;
            case (10  +1) : return Direction.DOWN;
            case (0   +1) : return Direction.LEFTDOWN;
            case (-10 +1) : return Direction.LEFT;
            case (-10 +0) : return Direction.LEFTUP;
        }

        throw new ArgumentException($"Tile [{kFromTile.m_xPos},{kFromTile.m_yPos}] and [{kToTile.m_xPos},{kToTile.m_yPos}] are not adjacent.");
    }

    public static int[] ConvertGlobalCoordToPlaygroundCoord(int kX, int kY)
    {
        return new int[] { kX - AStar.m_playGround.m_kStartPosX, kY - AStar.m_playGround.m_kStartPosY };
    }

    public static T GetNextEnumValue<T>(this T src) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException($"Argument {typeof(T).FullName} is not an Enum");

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(Arr, src) + 1;
        return (Arr.Length == j) ? Arr[0] : Arr[j];
    }

    public static T GetPrevEnumValue<T>(this T src) where T : struct
    {
        if (!typeof(T).IsEnum) throw new ArgumentException($"Argument {typeof(T).FullName} is not an Enum");

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(Arr, src) - 1;
        return (j < 0) ? Arr[Arr.Length - 1] : Arr[j];
    }
}