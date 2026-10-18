using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WFCInOut
{
    // Down, Up, Left, Right
    private int[] dRow = {-1, 1, 0, 0};
    private int[] dCol = {0, 0, -1, 1};

    private List<char> possibleTiles;
    private Dictionary<char, int> weights;
    private HashSet<List<char>> inputRules;
    private HashSet<char> lowestAllowedOptions;
    public List<List<char>> Excecute(int rows, int cols)
    {
        possibleTiles = new List<char>{'g','c','s'};

        List<List<char>> generated = new List<List<char>>();

        for(int y = 0; y < rows; y++)
        {
            generated.Add(new List<char>());
            for(int x = 0; x < rows; x++)
            {
                generated[y].Add('e');
            }
        }

        System.Random rand = new System.Random();

        int randomRow = rand.Next(rows);
        int randomCol = rand.Next(cols);

        int randomChoiceIndex = rand.Next(2);

        generated[randomRow][randomCol] = possibleTiles[randomChoiceIndex];

        Vector2Int current = new Vector2Int(randomCol, randomRow);

        int safetyTimeout = 0;
        while(generated.Any(anyList => anyList.Contains('e')))
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

    private char CollapseTile(System.Random rand)
    {
        var chances = new List<(char tile, int chance)>();
        int sumWeight = 0;

        foreach(char c in lowestAllowedOptions)
        {
            sumWeight += weights[c];
        }

        foreach(char c in lowestAllowedOptions)
        {
            chances.Add((c, sumWeight/weights[c]));
        }

        int roll = rand.Next(1, 101);
        char selectedTile = 'e';

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

    private Vector2Int GetLowestEntropy(List<List<char>> generated)
    {
        double currentLowest = double.MaxValue;
        Vector2Int current = Vector2Int.zero;

        for(int y = 0; y < generated.Count; y++)
        {
            for(int x = 0; x < generated[y].Count; x++)
            {
                if(generated[y][x] == 'e')
                {
                    HashSet<char> allowedOptions = new HashSet<char>();

                    // Loop through the 4 directions and populate the allowedOptions list.
                    for(int i = 0; i < 4; i++)
                    {
                        int nRow = y + dRow[i];
                        int nCol = x + dCol[i];

                        if(nRow >= 0 && nRow < generated.Count && nCol >= 0 && nCol < generated[nRow].Count)
                        {
                            char nValue = generated[nRow][nCol];

                            if(nValue != 'e')
                            {
                                char oppDirection = 'e';
                                // Get opposite direction
                                switch(i)
                                {
                                    case 0:
                                        oppDirection = 'u';
                                        break;
                                    case 1:
                                        oppDirection = 'd';
                                        break;
                                    case 2:
                                        oppDirection = 'r';
                                        break;
                                    case 3:
                                        oppDirection = 'l';
                                        break;
                                }

                                foreach(List<char> rule in inputRules)
                                {
                                    if(rule[0] == nValue && rule[2] == oppDirection)
                                    {
                                        allowedOptions.Add(rule[1]);
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
                        lowestAllowedOptions = new HashSet<char>(allowedOptions);
                    }
                }
            }
        }

        return current;
    }

    private double CalculateEntropy(HashSet<char> allowedOptions)
    {
        double sumWeight = 0;
        double sumLogWeight = 0;

        foreach(char c in allowedOptions)
        {
            sumWeight += weights[c];
            sumLogWeight += weights[c] * Math.Log(weights[c]);
        }

        double entropy = Math.Log(sumWeight) - (sumLogWeight / sumWeight);

        return entropy;
    }

    public void ReadInput(List<List<char>> gridTiles)
    {
        inputRules = new HashSet<List<char>>(new ListComparer<char>());
        weights = new Dictionary<char, int>();
        
        for(int y = 0; y < gridTiles.Count; y++)
        {
            for(int x = 0; x < gridTiles[y].Count; x++)
            {
                List<char> rule = new List<char>();
                rule.Add(gridTiles[y][x]);

                if(weights.ContainsKey(gridTiles[y][x]))
                {
                    weights[gridTiles[y][x]]++;
                }
                else weights.Add(gridTiles[y][x], 1);

                if (y-1 >= 0)
                {
                    rule.Add(gridTiles[y-1][x]);
                    rule.Add('d');

                    inputRules.Add(new List<char>(rule));
                    rule = new List<char>();
                    rule.Add(gridTiles[y][x]);
                }

                if (x-1 >= 0)
                {
                    rule.Add(gridTiles[y][x-1]);
                    rule.Add('l');

                    inputRules.Add(new List<char>(rule));
                    rule = new List<char>();
                    rule.Add(gridTiles[y][x]);
                }

                if (y+1 < gridTiles.Count)
                {
                    rule.Add(gridTiles[y+1][x]);
                    rule.Add('u');

                    inputRules.Add(new List<char>(rule));
                    rule = new List<char>();
                    rule.Add(gridTiles[y][x]);
                }

                if (x+1 <  gridTiles[y].Count)
                {
                    rule.Add(gridTiles[y][x+1]);
                    rule.Add('r');

                    inputRules.Add(new List<char>(rule));
                    rule = new List<char>();
                    rule.Add(gridTiles[y][x]);
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

        foreach(List<char> chars in inputRules)
        {
            foreach(char c in chars)
            {
                ruleString += c + " ";
            }
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