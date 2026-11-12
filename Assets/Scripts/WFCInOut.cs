using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WFCInOut
{
    private List<int> possibleTiles;
    private Dictionary<int, int> weights;
    private HashSet<(int, int, (int, int))> inputRules;
    private HashSet<int> lowestAllowedOptions;
    public List<List<int>> Excecute(int rows, int cols, HashSet<int> possibilites)
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

        int randomRow = rand.Next(rows);
        int randomCol = rand.Next(cols);

        int randomChoiceIndex = rand.Next(2);

        generated[randomRow][randomCol] = possibleTiles[randomChoiceIndex];

        Vector2Int current = new Vector2Int(randomCol, randomRow);

        int safetyTimeout = 0;
        while(generated.Any(anyList => anyList.Contains(-1)))
        {
            current = GetLowestEntropy(generated);

            generated[current.y][current.x] = CollapseTile(rand);

            safetyTimeout++;
            if(safetyTimeout > 99999)
            {
                Debug.LogError("Loop exceded 99999 excecutions.");
                break;
            }
        }

        return generated; 
    }

    private int CollapseTile(System.Random rand)
    {
        var chances = new List<(int tile, int chance)>();
        double sumWeight = 0;

        foreach(int c in lowestAllowedOptions)
        {
            sumWeight += weights[c];
        }

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

        return selectedTile;
    }

    private Vector2Int GetLowestEntropy(List<List<int>> generated)
    {
        double currentLowest = double.MaxValue;
        Vector2Int current = Vector2Int.zero;

        for(int y = 0; y < generated.Count; y++)
        {
            for(int x = 0; x < generated[y].Count; x++)
            {
                if(generated[y][x] == -1)
                {
                    HashSet<int> allowedOptions = new HashSet<int>();

                    for(int i = -1; i <= 1; i++)
                    {
                        for(int j = -1; j <= 1; j++)
                        {
                            if(x+j < 0 || y+i < 0 || (j == 0 && i == 0) || x+j >= generated[y].Count ||  y+i >= generated.Count) continue;

                            int nValue = generated[y+i][x+j];

                            if(nValue != -1)
                            {
                                // Get opposite direction
                                (int, int) oppDirection = (-j, -i);

                                foreach(var rule in inputRules)
                                {
                                    if(rule.Item1 == nValue && rule.Item3 == oppDirection)
                                    {
                                        allowedOptions.Add(rule.Item2);
                                    }
                                }
                            }
                        }
                    }

                    double currentEntropy = CalculateEntropy(allowedOptions);

                    if(currentLowest > currentEntropy)
                    {
                        currentLowest = currentEntropy;
                        current = new Vector2Int(x, y);
                        lowestAllowedOptions = new HashSet<int>(allowedOptions);
                    }
                }
            }
        }

        return current;
    }

    private double CalculateEntropy(HashSet<int> allowedOptions)
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

#nullable enable
public class ListComparer<T> : IEqualityComparer<List<T>>
{
    public bool Equals(List<T>? x, List<T>? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x == null || y == null) return false;
        
        // SequenceEqual checks if items match and are in the same sequence
        return x.SequenceEqual(y);
    }

    public int GetHashCode(List<T> obj)
    {
        if (obj == null) return 0;
        
        // Combine hashes of all elements inside the list to create a unique hash code
        HashCode hash = new HashCode();
        foreach (var item in obj)
        {
            hash.Add(item);
        }
        return hash.ToHashCode();
    }
}