﻿using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Items.Events;
using NeoServer.Game.Items.Factories;
using NeoServer.Game.Items.Factories.AttributeFactory;
using NeoServer.Game.World.Services;

namespace NeoServer.Game.Tests.Server;

public static class ItemFactoryTestBuilder
{
    public static IItemFactory Build(params IItemType[] itemTypes)
    {
        var itemTypeStore = ItemTypeStoreTestBuilder.Build(itemTypes);

        return new ItemFactory()
        {
            WeaponFactory = new WeaponFactory(new ChargeableFactory(), itemTypeStore),
            ItemTypeStore = itemTypeStore
        };
    }
    public static IItemFactory Build(IItemTypeStore itemTypeStore, IMap map =null)
    {
        var chargeableFactory = new ChargeableFactory();
        
        var itemFactory = new ItemFactory()
        {
            WeaponFactory = new WeaponFactory(chargeableFactory, itemTypeStore),
            DefenseEquipmentFactory = new DefenseEquipmentFactory(itemTypeStore, chargeableFactory),
            ItemTypeStore = itemTypeStore
        };
        
        var itemTransformedEventHandler =  new ItemTransformedEventHandler(map, new MapService(map), itemFactory);
        itemFactory.ItemEventSubscribers = new []
        {
            new ItemEventSubscriber(null,null, itemTransformedEventHandler)
        };

        return itemFactory;
    }
}