using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;

public class WFCEditor : EditorWindow
{
    private Tilemap currentTileMap;
    Tile tile1, tile2, tile3, tile4;

    Vector2Int startPos, length, startWFC, WFCLength;

    private WFCInOut inOut;

    [MenuItem("WFC/OpenWFC")]
    public static void ShowWindow()
    {
        GetWindow<WFCEditor>("OpenWFC");
    }

    void OnGUI()
    {
        GUILayout.Label("WFC Settings");

        currentTileMap = EditorGUILayout.ObjectField("Tilemap", currentTileMap, typeof(Tilemap), true) as Tilemap;

        tile1 = EditorGUILayout.ObjectField("Tile1", tile1, typeof(Tile), false) as Tile;
        tile2 = EditorGUILayout.ObjectField("Tile2", tile2, typeof(Tile), false) as Tile;
        tile3 = EditorGUILayout.ObjectField("Ttile3", tile3, typeof(Tile), false) as Tile;
        tile4 = EditorGUILayout.ObjectField("Ttile4", tile4, typeof(Tile), false) as Tile;

        startPos = EditorGUILayout.Vector2IntField("Start Position", startPos);
        length = EditorGUILayout.Vector2IntField("Length", length);

        startWFC = EditorGUILayout.Vector2IntField("WFC Position", startWFC);
        WFCLength = EditorGUILayout.Vector2IntField("WFC Length", WFCLength);

        EditorGUILayout.Space();
        inOut = new WFCInOut();

        if (GUILayout.Button("Start WFC"))
        {
            ExcecuteWFC();
        }
    }

    void ExcecuteWFC()
    {
        List<List<char>> readGrid = new();
        float endY = length.y + startPos.y;
        float endX = length.x + startPos.x;

        for(int y = (int)startPos.y; y < endY; y++)
        {
            List<char> tempRow = new List<char>();
            for(int x = (int)startPos.x; x < endX; x++)
            {
                Tile currTile = (Tile)currentTileMap.GetTile(new Vector3Int(x, y));

                if(currTile == tile1)
                {
                    tempRow.Add('g');
                }

                if(currTile == tile2)
                {
                    tempRow.Add('s');
                }

                if(currTile == tile3)
                {
                    tempRow.Add('c');
                }

                if(currTile == tile4)
                {
                    tempRow.Add('f');
                }
            }
            readGrid.Add(new List<char>(tempRow));
        }

       inOut.ReadInput(readGrid);

       Undo.RecordObject(currentTileMap, "TileMap update using WFC Algorithm");

       DrawTiles(inOut.Excecute(WFCLength.x, WFCLength.y));

       EditorUtility.SetDirty(currentTileMap);

       //Debug.Log(inOut.RulesToString());

    }

    private void DrawTiles(List<List<char>> generated)
    {
        string output = "";
        for(int y = 0; y < generated.Count; y++)
        {
            for(int x = 0; x < generated[y].Count; x++)
            {
                output += generated[y][x] + " ";
                switch(generated[y][x])
                {
                    case 'g':
                        currentTileMap.SetTile(new Vector3Int(startWFC.x + x, startWFC.y + y), tile1);
                        break;
                    case 's':
                        currentTileMap.SetTile(new Vector3Int(startWFC.x + x, startWFC.y + y), tile2);
                        break;
                    case 'c':
                        currentTileMap.SetTile(new Vector3Int(startWFC.x + x, startWFC.y + y), tile3);
                        break;
                    case 'f':
                        currentTileMap.SetTile(new Vector3Int(startWFC.x + x, startWFC.y + y), tile4);
                        break;
                }
            }
            output += "\n";
        }
        Debug.Log(output);
    }
}
