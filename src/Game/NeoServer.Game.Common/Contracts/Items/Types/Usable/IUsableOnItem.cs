﻿using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World.Tiles;

namespace NeoServer.Game.Common.Contracts.Items.Types.Usable;

public delegate void UseOnTile(ICreature usedBy, IDynamicTile tile, IUsableOnTile item);

public interface IUsableOnItem : IUsableOn
{
    /// <summary>
    ///     Useable by creatures on items (ground, weapon, stairs..)
    /// </summary>
    /// <param name="usedBy">player whose item is being used</param>
    /// <param name="item">item which will receive action</param>
    public bool Use(ICreature usedBy, IItem item);
}

public interface IUsableOnTile : IUsableOn
{
    event UseOnTile OnUsedOnTile;

    /// <summary>
    /// Usable by creatures on items (ground, weapon, stairs..)
    /// </summary>
    /// <param name="usedBy">player whose item is being used</param>
    /// <param name="tile">tile which will receive action</param>
    public bool Use(ICreature usedBy, ITile tile);
}

public interface IUsableAttackOnTile : IUsableOn
{
    /// <summary>
    /// Usable by creatures on items (ground, weapon, stairs..)
    /// </summary>
    /// <param name="usedBy">player whose item is being used</param>
    /// <param name="tile">tile which will receive action</param>
    /// <param name="combat"></param>
    public bool Use(ICreature usedBy, ITile tile, out CombatAttackResult combat);
}