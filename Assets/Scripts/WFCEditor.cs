using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using System.Linq;

public class WFCEditor : EditorWindow
{
    private Tilemap currentTileMap;
    private List<TileBase> tilesList;
    Tile tile2;

    Vector2Int startPos, length, startWFC, WFCLength;

    BoundsInt bound;

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
        List<List<int>> readGrid = new();
        HashSet<int> possibleTiles = new();

        float endY = length.y + startPos.y;
        float endX = length.x + startPos.x;

        BoundsInt currentBounds = new BoundsInt(new Vector3Int(startPos.x, startPos.y), new Vector3Int(length.x, length.y, 1));
        TileBase[] tiles = currentTileMap.GetTilesBlock(currentBounds);
        tilesList = tiles.ToList().Distinct().ToList();

        foreach(var tile in tilesList)
        {
            Debug.Log(tile.name);
        }

        for(int y = (int)startPos.y; y < endY; y++)
        {
            List<int> tempRow = new List<int>();
            for(int x = (int)startPos.x; x < endX; x++)
            {
                Tile currTile = (Tile)currentTileMap.GetTile(new Vector3Int(x, y));
                
                tempRow.Add(tilesList.IndexOf(currTile));
                possibleTiles.Add(tilesList.IndexOf(currTile));
                
            }
            readGrid.Add(new List<int>(tempRow));
        }

       inOut.ReadInput(readGrid);

       Undo.RecordObject(currentTileMap, "TileMap update using WFC Algorithm");

       DrawTiles(inOut.Excecute(WFCLength.x, WFCLength.y, possibleTiles));

       EditorUtility.SetDirty(currentTileMap);

       Debug.Log(inOut.RulesToString());

    }

    private void DrawTiles(List<List<int>> generated)
    {
        string output = "";
        for(int y = 0; y < generated.Count; y++)
        {
            for(int x = 0; x < generated[y].Count; x++)
            {
                output += generated[y][x] + " ";

                if(generated[y][x] == -1) break;
                
                currentTileMap.SetTile(new Vector3Int(startWFC.x + x, startWFC.y + y), tilesList[generated[y][x]]);
            }
            output += "\n";
        }
        Debug.Log(output);
    }
}
