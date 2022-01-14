﻿using Moq;
using NeoServer.Data.InMemory.DataStores;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Creatures.Factories;
using NeoServer.Game.Items.Factories;
using NeoServer.Game.World.Services;
using NeoServer.Game.World.Spawns;
using Serilog;
using PathFinder = NeoServer.Game.World.Map.PathFinder;

namespace NeoServer.Game.Tests.Helpers
{
    public static class NpcTestDataBuilder
    {
        public static INpc Build(string name, INpcType npcType)
        {
            var logger = new Mock<ILogger>();
            var itemFactory = new ItemFactory();

            var npcStore = new NpcStore();
            npcStore.Add(name, npcType);

            var coinTypeStore = new CoinTypeStore();

            var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7, addGround: true);
            var pathFinder = new PathFinder(map);
            var mapTool = new MapTool(map, pathFinder);

            var spawnPoint = new SpawnPoint(new Location(105, 105, 7), 60);

            var npcFactory = new NpcFactory(logger.Object, itemFactory, npcStore, coinTypeStore, mapTool);

            var npc = npcFactory.Create(name, spawnPoint);
            
            npc.Location = new Location(105, 105, 7); 
            map.PlaceCreature(npc);

            return npc;
        }
    }
}