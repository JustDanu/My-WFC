using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WFCInOut
{
    private List<int> possibleTiles;
    private Dictionary<int, int> weights;
    private HashSet<(int, int, (int, int))> inputRules;
    private HashSet<int> lowestAllowedOptions;

    private Stack<(List<List<int>>, List<int>, int)> collapseHistory;
    public List<List<int>> Excecute(int rows, int cols, HashSet<int> possibilites, List<Vector2Int> orderAdded)
    {
        possibleTiles = possibilites.ToList();

        List<List<int>> generated = new List<List<int>>();

        for(int y = 0; y < rows; y++)
        {
            generated.Add(new List<int>());
            for(int x = 0; x < rows; x++)
            {
                generated[y].Add(-1);
            }
        }

        System.Random rand = new System.Random();

        int randomRow = 0;
        int randomCol = 0;

        Vector2Int current = Vector2Int.zero;;

        for(int i = 0; i < 1; i++)
        {
            randomRow = rand.Next(rows);
            randomCol = rand.Next(cols);

            int randomChoiceIndex = rand.Next(possibleTiles.Count);

            generated[randomRow][randomCol] = possibleTiles[randomChoiceIndex];

            current = new Vector2Int(randomCol, randomRow);
            orderAdded.Add(current);
        }

        collapseHistory = new();

        int safetyTimeout = 0;
        while(generated.Any(anyList => anyList.Contains(-1)))
        {
            current = GetLowestEntropy(generated);
            orderAdded.Add(current);

            int collapsedTile = CollapseTile(rand, generated);

            generated[current.y][current.x] = collapsedTile;

            safetyTimeout++;
            if(safetyTimeout > 99999)
            {
                Debug.LogError("Loop exceded 99999 excecutions.");
                break;
            }
        }

        return generated; 
    }

    private int CollapseTile(System.Random rand, List<List<int>> currentGenerated)
    {
        /*
        var chances = new List<(int tile, int chance)>();
        double sumWeight = 0;

        string allowedOptionsString = "";
        foreach(int c in lowestAllowedOptions)
        {
            allowedOptionsString += c + " ";
            sumWeight += weights[c];
        }
        Debug.Log(allowedOptionsString);

        foreach(int c in lowestAllowedOptions)
        {
            int calculated = (int)((weights[c]/sumWeight)*100);
            //Debug.Log("Character: " + c + " Chance: " + calculated + " Weight: " + weights[c] + " Sum Weights: " + sumWeight);
            chances.Add((c, calculated));
        }

        int roll = rand.Next(1, 101);
        int selectedTile = -1;

        foreach(var option in chances)
        {
            roll -= option.chance;
            if(roll <= 0)
            {
                selectedTile = option.tile;
                break;
            }
        }
        */

        string allowedOptionsString = "";
        foreach(int c in lowestAllowedOptions)
        {
            allowedOptionsString += c + " ";
        }
        //Debug.Log(allowedOptionsString);
        if(lowestAllowedOptions.Count == 0)
        {
            for(int i = 0; i < 4; i++)
            {
                rewindMap(collapseHistory, currentGenerated);
            }
            
            return -1;
        }

        var lowestList = lowestAllowedOptions.ToList();

        int selectedTile = lowestList[rand.Next(lowestList.Count)];
        //Debug.Log(selectedTile);

        collapseHistory.Push((currentGenerated, lowestList, selectedTile));

        return selectedTile;
    }

    private void rewindMap(Stack<(List<List<int>>, List<int>, int)> collapseHistory, List<List<int>> currentGenerated)
    {
        currentGenerated = collapseHistory.Pop().Item1;
    }

    private Vector2Int GetLowestEntropy(List<List<int>> generated)
    {
        double currentLowest = possibleTiles.Count;
        Vector2Int current = Vector2Int.zero;

        for(int y = 0; y < generated.Count; y++)
        {
            for(int x = 0; x < generated[y].Count; x++)
            {
                if(generated[y][x] == -1)
                {
                    List<List<int>> allowedOptions = new List<List<int>>();

                    for(int i = -1; i <= 1; i++)
                    {
                        for(int j = -1; j <= 1; j++)
                        {
                            if(x+j < 0 || y+i < 0 || (j == 0 && i == 0) || x+j >= generated[y].Count ||  y+i >= generated.Count) continue;

                            List<int> tileAllowed = new List<int>();

                            int nValue = generated[y+i][x+j];

                            if(nValue != -1)
                            {
                                // Get opposite direction
                                (int, int) oppDirection = (-j, -i);

                                foreach(var rule in inputRules)
                                {
                                    if(rule.Item1 == nValue && rule.Item3 == oppDirection)
                                    {
                                        tileAllowed.Add(rule.Item2);
                                    }
                                }
                                if(tileAllowed.Count > 0) allowedOptions.Add(tileAllowed);
                            }
                        }
                    }

                    List<int> commonElements = new();
                    if(allowedOptions.Count > 0)
                    {
                        commonElements = allowedOptions.Any() ? allowedOptions.Skip(1).Aggregate(new HashSet<int>(allowedOptions.First()),
                                        (common, nextList) => { common.IntersectWith(nextList); return common; }).ToList() : new List<int>();
                    }

                    double currentEntropy = commonElements.Count;

                    if(currentLowest > currentEntropy && commonElements.Count != 0)
                    {
                        currentLowest = currentEntropy;
                        current = new Vector2Int(x, y);
                        lowestAllowedOptions = new HashSet<int>(commonElements);
                    }
                }
            }
        }

        return current;
    }

    private double CalculateEntropy(List<int> allowedOptions)
    {
        double sumWeight = 0;
        double sumLogWeight = 0;

        foreach(int c in allowedOptions)
        {
            sumWeight += weights[c];
            sumLogWeight += weights[c] * Math.Log(weights[c]);
        }

        double entropy = Math.Log(sumWeight) - (sumLogWeight / sumWeight);

        return entropy;
    }

    public void ReadInput(List<List<int>> gridTiles)
    {
        inputRules = new HashSet<(int, int, (int, int))>();
        weights = new Dictionary<int, int>();

        for(int y = 0; y < gridTiles.Count; y++)
        {
            for(int x = 0; x < gridTiles[y].Count; x++)
            {
                if (gridTiles[y][x] == -1) continue;

                (int, int, (int, int)) rule = new();

                rule.Item1 = gridTiles[y][x];

                if(weights.ContainsKey(gridTiles[y][x]))
                {
                    weights[gridTiles[y][x]]++;
                }
                else weights.Add(gridTiles[y][x], 1);

                for(int i = -1; i <= 1; i++)
                {
                    for(int j = -1; j <= 1; j++)
                    {
                        //Debug.Log("Looking at: " + gridTiles[y][x]);
                        //Debug.Log("Looking at position: " + "(" + j + ", " + i + ")");

                        if(x+j < 0 || y+i < 0 || (j == 0 && i == 0) || x+j >= gridTiles[y].Count ||  y+i >= gridTiles.Count) continue; // Fuckin return call

                        if(gridTiles[y+i][x+j] == -1) continue;

                        //Debug.Log("Tile looking at if exists: " + gridTiles[y+i][x+j]);

                        rule.Item2 = gridTiles[y+i][x+j];
                        rule.Item3 = (j, i);

                        inputRules.Add(rule);
                    }
                }
            }
        }
    }

    public void WriteTiles()
    {
        
    }

    public string RulesToString()
    {
        string ruleString = "";

        foreach(var ints in inputRules)
        {
            ruleString += ints.Item1 + " ";
            ruleString += ints.Item2 + " ";
            ruleString += ints.Item3;
            ruleString += "\n";
        }
        return ruleString;
    }
}