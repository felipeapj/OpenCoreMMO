﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;

namespace NeoServer.Game.World.Algorithms;

public class AStarTibia
{
    private bool pathCondition(IMap map, Location startPos, Location testPos,
        Location targetPos, ref int bestMatchDist, FindPathParams fpp)
    {
        if (!map.IsInRange(startPos, testPos, targetPos, fpp)) return false;

        var testDist = Math.Max(targetPos.GetSqmDistanceX(testPos), targetPos.GetSqmDistanceY(testPos));
        if (fpp.MaxTargetDist == 1)
        {
            if (testDist < fpp.MinTargetDist || testDist > fpp.MaxTargetDist) return false;

            return true;
        }

        if (testDist <= fpp.MaxTargetDist)
        {
            if (testDist < fpp.MinTargetDist) return false;

            if (testDist == fpp.MaxTargetDist)
            {
                bestMatchDist = 0;
                return true;
            }

            if (testDist > bestMatchDist)
            {
                //not quite what we want, but the best so far
                bestMatchDist = testDist;
                return true;
            }
        }

        return false;
    }

    public bool GetPathMatching(IMap map, ICreature creature, Location targetPos, FindPathParams
        fpp, ITileEnterRule tileEnterRule, out Direction[] directions)
    {
        var pos = creature.Location;

        directions = new Direction[0];

        var endPos = new Location();

        var dirList = new List<Direction>();

        var bestMatch = 0;

        var nodes = new Nodes(pos);

        var allNeighbors = new Tuple<int, int>[8]
        {
            new(-1, 0),
            new(0, 1),
            new(1, 0),
            new(0, -1),
            new(-1, -1),
            new(1, -1),
            new(1, 1),
            new(-1, 1)
        };

        var startPos = pos;
        AStarNode found = null;

        while (fpp.MaxSearchDist != 0 || nodes.ClosedNodes < 100)
        {
            var n = nodes.GetBestNode();
            if (n == null)
            {
                if (found != null) break;
                return false;
            }

            var x = n.X;
            var y = n.Y;

            pos.X = (ushort)x;
            pos.Y = (ushort)y;

            if (pathCondition(map, startPos, pos, targetPos, ref bestMatch, fpp))
            {
                found = n;
                endPos = pos;
                if (bestMatch == 0) break;
            }

            int dirCount;
            Tuple<int, int>[] neighbors;

            if (n.Parent != null)
            {
                var offsetX = n.Parent.X - x;
                var offsetY = n.Parent.Y - y;
                if (offsetY == 0)
                {
                    if (offsetX == -1)
                        neighbors = NeighborsDirection.West;
                    else
                        neighbors = NeighborsDirection.East;
                }
                else if (!fpp.AllowDiagonal || offsetX == 0)
                {
                    if (offsetY == -1)
                        neighbors = NeighborsDirection.North;
                    else
                        neighbors = NeighborsDirection.South;
                }
                else if (offsetY == -1)
                {
                    if (offsetX == -1)
                        neighbors = NeighborsDirection.NorthWest;
                    else
                        neighbors = NeighborsDirection.NorthEast;
                }
                else if (offsetX == -1)
                {
                    neighbors = NeighborsDirection.SouthWest;
                }
                else
                {
                    neighbors = NeighborsDirection.SouthEast;
                }

                dirCount = fpp.AllowDiagonal ? 5 : 3;
            }
            else
            {
                dirCount = 8;
                neighbors = allNeighbors;
            }

            var f = n.F;
            for (var i = 0; i < dirCount; ++i)
            {
                pos.X = (ushort)(x + neighbors[i].Item1);
                pos.Y = (ushort)(y + neighbors[i].Item2);

                if (fpp.MaxSearchDist != 0 && (startPos.GetSqmDistanceX(pos) > fpp.MaxSearchDist ||
                                               startPos.GetSqmDistanceY(pos) > fpp.MaxSearchDist)) continue;
                if (fpp.KeepDistance && !map.IsInRange(startPos, pos, targetPos, fpp)) continue;

                var neighborNode = nodes.GetNodeByPosition(pos);

                var tile = map[pos];

                if (!tileEnterRule.ShouldIgnore(tile, creature)) continue;

                var cost = n.GetMapWalkCost(pos);

                if (tile is IDynamicTile walkableTile) cost += n.GetTileWalkCost(creature, walkableTile);

                var newF = f + cost;

                if (neighborNode != null)
                {
                    if (neighborNode.F <= newF) continue;

                    neighborNode.F = newF;
                    neighborNode.Parent = n;
                    nodes.OpenNode(neighborNode);
                }
                else
                {
                    neighborNode = nodes.CreateOpenNode(n, pos.X, pos.Y, newF);

                    if (neighborNode == null)
                    {
                        if (found != null) break;
                        return false;
                    }
                }
            }

            nodes.CloseNode(n);
        }

        if (found == null) return false;

        int prevx = endPos.X;
        int prevy = endPos.Y;

        found = found.Parent;

        while (found != null)
        {
            pos.X = (ushort)found.X;
            pos.Y = (ushort)found.Y;

            var dx = pos.X - prevx;
            var dy = pos.Y - prevy;

            prevx = pos.X;
            prevy = pos.Y;

            if (dx == 1 && dy == 1)
                dirList.Insert(0, Direction.NorthWest);
            else if (dx == -1 && dy == 1)
                dirList.Insert(0, Direction.NorthEast);
            else if (dx == 1 && dy == -1)
                dirList.Insert(0, Direction.SouthWest);
            else if (dx == -1 && dy == -1)
                dirList.Insert(0, Direction.SouthEast);
            else if (dx == 1)
                dirList.Insert(0, Direction.West);
            else if (dx == -1)
                dirList.Insert(0, Direction.East);
            else if (dy == 1)
                dirList.Insert(0, Direction.North);
            else if (dy == -1) dirList.Insert(0, Direction.South);

            found = found.Parent;
        }

        directions = dirList.ToArray();
        return true;
    }
}

internal class Nodes
{
    private readonly List<AStarNode> nodes = new(512);
    private readonly Dictionary<AStarNode, int> nodesIndexMap = new();
    private readonly Dictionary<AStarPosition, AStarNode> nodesMap = new();
    private readonly bool[] openNodes = new bool[512];
    private readonly AStarNode startNode;
    public int currentNode;

    public Nodes(Location location)
    {
        currentNode = 1;
        ClosedNodes = 0;
        openNodes[0] = true;

        startNode = new AStarNode(location.X, location.Y)
        {
            F = 0
        };

        nodes.Add(startNode);
        nodesIndexMap.Add(startNode, nodes.Count - 1);
        nodesMap.Add(new AStarPosition(location.X, location.Y), nodes[0]);
    }

    public int ClosedNodes { get; private set; }

    public AStarNode GetBestNode()
    {
        if (currentNode == 0) return null;

        var best_node_f = int.MaxValue;
        var best_node = -1;
        for (var i = 0; i < currentNode; i++)
            if (openNodes[i] && nodes[i].F < best_node_f)
            {
                best_node_f = nodes[i].F;
                best_node = i;
            }

        if (best_node >= 0)
        {
            nodes[best_node] = nodes[best_node] ?? new AStarNode();
            return nodes[best_node];
        }

        return null;
    }

    internal void CloseNode(AStarNode node)
    {
        var index = 0;
        var start = 0;
        while (true)
        {
            index = nodesIndexMap[node];
            if (openNodes[index] == false)
                start = ++index;
            else
                break;
        }

        if (index >= 512) return;

        openNodes[index] = false;
        ++ClosedNodes;
    }

    internal AStarNode CreateOpenNode(AStarNode parent, int x, int y, int newF)
    {
        if (currentNode >= 512) return null;

        var retNode = currentNode++;
        openNodes[retNode] = true;

        var node = new AStarNode(x, y)
        {
            F = newF,
            Parent = parent
        };
        nodes.Add(node);

        nodesIndexMap.Add(node, nodes.Count - 1);
        nodesMap.TryAdd(new AStarPosition(node.X, node.Y), node);
        return node;
    }

    internal AStarNode GetNodeByPosition(Location location)
    {
        nodesMap.TryGetValue(new AStarPosition(location.X, location.Y), out var foundNode);
        return foundNode;
    }

    internal void OpenNode(AStarNode node)
    {
        var index = nodesIndexMap[node];

        if (index >= 512) return;

        if (!openNodes[index])
        {
            openNodes[index] = true;
            --ClosedNodes;
        }
    }
}

internal readonly struct AStarPosition : IEquatable<AStarPosition>
{
    public AStarPosition(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X { get; }
    public int Y { get; }

    public bool Equals([AllowNull] AStarPosition other)
    {
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object obj)
    {
        return obj is AStarPosition pos && Equals(pos);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
}

internal class AStarNode : IEquatable<AStarNode>
{
    public AStarNode(int x, int y)
    {
        X = x;
        Y = y;
    }

    public AStarNode()
    {
    }

    public int F { get; set; }
    public int X { get; internal set; }
    public int Y { get; internal set; }
    public AStarNode Parent { get; set; }

    public bool Equals([AllowNull] AStarNode other)
    {
        return Equals(this, other);
    }

    public override bool Equals(object obj)
    {
        return obj is AStarNode node && Equals(this, node);
    }

    public override int GetHashCode()
    {
        return GetHashCode(this);
    }

    public bool Equals([AllowNull] AStarNode x, [AllowNull] AStarNode y)
    {
        return x.X == y.X && x.Y == y.Y;
    }

    public int GetHashCode([DisallowNull] AStarNode obj)
    {
        return HashCode.Combine(obj.X, obj.Y);
    }

    public int GetMapWalkCost(Location neighborPos)
    {
        if (Math.Abs(X - neighborPos.X) == Math.Abs(Y - neighborPos.Y))
            //diagonal movement extra cost
            return 25;
        return 10;
    }

    public int GetTileWalkCost(ICreature creature, IDynamicTile tile)
    {
        var cost = 0;

        if (tile.GetTopVisibleCreature(creature) != null) cost += 10 * 3;

        if (tile.MagicField != null)
        {
            //if(creature.IsImmune() tile.MagicField.Type;
        }

        return cost;
    }
}

internal class NeighborsDirection
{
    public static Tuple<int, int>[] West => new Tuple<int, int>[5]
    {
        new(0, 1),
        new(1, 0),
        new(0, -1),
        new(1, -1),
        new(1, 1)
    };

    public static Tuple<int, int>[] East => new Tuple<int, int>[5]
    {
        new(-1, 0),
        new(0, 1),
        new(0, -1),
        new(-1, -1),
        new(-1, 1)
    };

    public static Tuple<int, int>[] North => new Tuple<int, int>[5]
    {
        new(-1, 0),
        new(0, 1),
        new(1, 0),
        new(1, 1),
        new(-1, 1)
    };

    public static Tuple<int, int>[] South => new Tuple<int, int>[5]
    {
        new(-1, 0),
        new(1, 0),
        new(0, -1),
        new(-1, -1),
        new(1, -1)
    };

    public static Tuple<int, int>[] NorthWest => new Tuple<int, int>[5]
    {
        new(0, 1),
        new(1, 0),
        new(1, -1),
        new(1, 1),
        new(-1, 1)
    };

    public static Tuple<int, int>[] NorthEast => new Tuple<int, int>[5]
    {
        new(-1, 0),
        new(0, 1),
        new(-1, -1),
        new(1, 1),
        new(-1, 1)
    };

    public static Tuple<int, int>[] SouthWest => new Tuple<int, int>[5]
    {
        new(1, 0),
        new(0, -1),
        new(-1, -1),
        new(1, -1),
        new(1, 1)
    };

    public static Tuple<int, int>[] SouthEast => new Tuple<int, int>[5]
    {
        new(-1, 0),
        new(0, -1),
        new(-1, -1),
        new(1, -1),
        new(-1, 1)
    };
}