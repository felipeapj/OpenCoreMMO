﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types;
using NeoServer.Game.Common.Contracts.Items.Types.Containers;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Items.Items.Containers;
using NeoServer.Game.Items.Items.Weapons;
using NeoServer.Game.Tests.Helpers;
using Xunit;

namespace NeoServer.Game.Creatures.Tests.Players;

public class InventoryTests
{
    public static IEnumerable<object[]> SlotItemsData =>
        new List<object[]>
        {
            new object[] { Slot.Backpack, ItemTestData.CreateBackpack() },
            new object[] { Slot.Ammo, ItemTestData.CreateAmmo(100, 10) },
            new object[] { Slot.Head, ItemTestData.CreateBodyEquipmentItem(100, "head") },
            new object[] { Slot.Left, ItemTestData.CreateWeaponItem(100, "axe") },
            new object[] { Slot.Body, ItemTestData.CreateBodyEquipmentItem(100, "body") },
            new object[] { Slot.Feet, ItemTestData.CreateBodyEquipmentItem(100, "feet") },
            new object[] { Slot.Right, ItemTestData.CreateBodyEquipmentItem(100, "", "shield") },
            new object[] { Slot.Ring, ItemTestData.CreateBodyEquipmentItem(100, "ring") },
            new object[] { Slot.Left, ItemTestData.CreateWeaponItem(100, "sword", true) },
            new object[] { Slot.Necklace, ItemTestData.CreateBodyEquipmentItem(100, "necklace") }
        };

    public static IEnumerable<object[]> BackpackSlotAddItemsData =>
        new List<object[]>
        {
            new object[] { ItemTestData.CreateAmmo(100, 10) },
            new object[] { ItemTestData.CreateBodyEquipmentItem(100, "head") },
            new object[] { ItemTestData.CreateWeaponItem(100, "axe") },
            new object[] { ItemTestData.CreateBodyEquipmentItem(100, "body") },
            new object[] { ItemTestData.CreateBodyEquipmentItem(100, "feet") },
            new object[] { ItemTestData.CreateBodyEquipmentItem(100, "", "shield") },
            new object[] { ItemTestData.CreateBodyEquipmentItem(100, "ring") },
            new object[] { ItemTestData.CreateWeaponItem(100, "sword", true) },
            new object[] { ItemTestData.CreateBodyEquipmentItem(100, "necklace") },
            new object[] { ItemTestData.CreateCumulativeItem(100, 87) }
        };

    public static IEnumerable<object[]> SlotSwapItemsData =>
        new List<object[]>
        {
            new object[] { Slot.Ammo, ItemTestData.CreateAmmo(100, 10), ItemTestData.CreateAmmo(102, 10) },
            new object[]
            {
                Slot.Head, ItemTestData.CreateBodyEquipmentItem(100, "head"),
                ItemTestData.CreateBodyEquipmentItem(102, "head")
            },
            new object[]
                { Slot.Left, ItemTestData.CreateWeaponItem(100, "axe"), ItemTestData.CreateWeaponItem(102, "axe") },
            new object[]
            {
                Slot.Body, ItemTestData.CreateBodyEquipmentItem(100, "body"),
                ItemTestData.CreateBodyEquipmentItem(102, "body")
            },
            new object[]
            {
                Slot.Feet, ItemTestData.CreateBodyEquipmentItem(100, "feet"),
                ItemTestData.CreateBodyEquipmentItem(102, "feet")
            },
            new object[]
            {
                Slot.Right, ItemTestData.CreateBodyEquipmentItem(100, "", "shield"),
                ItemTestData.CreateBodyEquipmentItem(102, "", "shield")
            },
            new object[]
            {
                Slot.Ring, ItemTestData.CreateBodyEquipmentItem(100, "ring"),
                ItemTestData.CreateBodyEquipmentItem(102, "ring")
            },
            new object[]
            {
                Slot.Left, ItemTestData.CreateWeaponItem(100, "sword", true),
                ItemTestData.CreateWeaponItem(102, "sword", true)
            },
            new object[]
            {
                Slot.Necklace, ItemTestData.CreateBodyEquipmentItem(100, "necklace"),
                ItemTestData.CreateBodyEquipmentItem(102, "necklace")
            }
            //new object[] {Slot.Backpack, ItemTestData.CreateBackpack(), ItemTestData.CreateBackpack()},
        };

    public static IEnumerable<object[]> SlotJoinItemsData =>
        new List<object[]>
        {
            new object[]
            {
                Slot.Ammo, ItemTestData.CreateAmmo(100, 10), ItemTestData.CreateAmmo(100, 10),
                ItemTestData.CreateAmmo(100, 20)
            },
            new object[]
            {
                Slot.Ammo, ItemTestData.CreateAmmo(100, 10), ItemTestData.CreateAmmo(100, 90),
                ItemTestData.CreateAmmo(100, 100)
            },
            new object[]
            {
                Slot.Ammo, ItemTestData.CreateAmmo(100, 50), ItemTestData.CreateAmmo(100, 90),
                ItemTestData.CreateAmmo(100, 100)
            },
            new object[]
            {
                Slot.Left, ItemTestData.CreateThrowableDistanceItem(100),
                ItemTestData.CreateThrowableDistanceItem(100, 5), ItemTestData.CreateThrowableDistanceItem(100, 6)
            }
        };

    public static IEnumerable<object[]> WrongSlotItemsData => GenerateWrongSlotItemsData();

    private static List<object[]> GenerateWrongSlotItemsData()
    {
        var result = new List<object[]>();
        foreach (var slot in new List<Slot>
                 {
                     Slot.Head, Slot.Ammo, Slot.Backpack, Slot.Body, Slot.Feet, Slot.Left, Slot.Right, Slot.Ring,
                     Slot.TwoHanded,
                     Slot.Legs, Slot.Necklace
                 })
        {
            if (slot != Slot.Body)
                result.Add(new object[] { slot, ItemTestData.CreateBodyEquipmentItem(100, "body") });

            if (slot != Slot.Ammo)
                result.Add(new object[] { slot, ItemTestData.CreateAmmo(100, 10) });

            if (slot != Slot.Legs)
                result.Add(new object[] { slot, ItemTestData.CreateBodyEquipmentItem(100, "legs") });

            if (slot != Slot.Feet)
                result.Add(new object[] { slot, ItemTestData.CreateBodyEquipmentItem(100, "feet") });

            if (slot != Slot.Right)
                result.Add(new object[] { slot, ItemTestData.CreateBodyEquipmentItem(100, "", "shield") });

            if (slot != Slot.Left)
                result.Add(new object[] { slot, ItemTestData.CreateWeaponItem(100, "axe") });

            if (slot != Slot.Ring)
                result.Add(new object[] { slot, ItemTestData.CreateDefenseEquipmentItem(100) });

            if (slot != Slot.Necklace)
                result.Add(new object[] { slot, ItemTestData.CreateDefenseEquipmentItem(100) });

            if (slot != Slot.Backpack)
                result.Add(new object[] { slot, ItemTestData.CreateBackpack() });

            if (slot != Slot.Head)
                result.Add(new object[] { slot, ItemTestData.CreateBodyEquipmentItem(100, "head") });
        }

        return result;
    }

    [Theory]
    [MemberData(nameof(SlotItemsData))]
    public void AddItemToSlot_AddItem_When_Slot_Is_Empty(Slot slot, IPickupable item)
    {
        var sut = InventoryTestDataBuilder.Build();

        sut.TryAddItemToSlot(slot, item);

        item.Should().Be(sut[slot]);
    }

    [Theory]
    [MemberData(nameof(WrongSlotItemsData))]
    public void AddItemToSlot_AddItem_On_Wrong_Slot_Returns_False(Slot slot, IPickupable item)
    {
        var sut = InventoryTestDataBuilder.Build(inventoryMap: new Dictionary<Slot, Tuple<IPickupable, ushort>>());
        var result = sut.TryAddItemToSlot(slot, item);

        Assert.False(result.Succeeded);
        Assert.Equal(InvalidOperation.CannotDress, result.Error);
    }

    [Fact]
    public void AddItemToSlot_AddTwoHanded_And_Shield_Returns_False()
    {
        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(),
            new Dictionary<Slot, Tuple<IPickupable, ushort>>());

        var twoHanded = ItemTestData.CreateWeaponItem(100, "axe", true);
        var result = sut.TryAddItemToSlot(Slot.Left, twoHanded);

        Assert.Same(twoHanded, sut[Slot.Left]);
        Assert.Null(sut[Slot.Right]);
        Assert.True(result.Succeeded);

        var shield = ItemTestData.CreateBodyEquipmentItem(101, "", "shield");
        Assert.Same(twoHanded, sut[Slot.Left]);

        result = sut.TryAddItemToSlot(Slot.Right, shield);
        Assert.False(result.Succeeded);
        Assert.Equal(InvalidOperation.BothHandsNeedToBeFree, result.Error);

        Assert.Null(sut[Slot.Right]);
    }

    [Fact]
    public void AddItemToSlot_Add_Shield_And_TwoHanded_Returns_False()
    {
        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(),
            new Dictionary<Slot, Tuple<IPickupable, ushort>>());

        var shield = ItemTestData.CreateBodyEquipmentItem(101, "", "shield");

        var result = sut.TryAddItemToSlot(Slot.Right, shield);

        Assert.Same(shield, sut[Slot.Right]);
        Assert.Null(sut[Slot.Left]);
        Assert.True(result.Succeeded);

        var twoHanded = ItemTestData.CreateWeaponItem(100, "axe", true);
        Assert.Same(shield, sut[Slot.Right]);

        result = sut.TryAddItemToSlot(Slot.Left, twoHanded);
        Assert.False(result.Succeeded);
        Assert.Equal(InvalidOperation.BothHandsNeedToBeFree, result.Error);

        Assert.Null(sut[Slot.Left]);
    }

    [Fact]
    public void AddItemToSlot_When_Exceeds_Capacity_Returns_False()
    {
        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(),
            new Dictionary<Slot, Tuple<IPickupable, ushort>>());

        var legs = ItemTestData.CreateBodyEquipmentItem(100, "legs");
        var feet = ItemTestData.CreateBodyEquipmentItem(101, "feet");
        var body = ItemTestData.CreateBodyEquipmentItem(100, "body");

        sut.TryAddItemToSlot(Slot.Legs, legs);
        var result = sut.TryAddItemToSlot(Slot.Feet, feet);

        Assert.True(result.Succeeded);

        result = sut.TryAddItemToSlot(Slot.Body, body);

        Assert.False(result.Succeeded);
        Assert.Equal(InvalidOperation.TooHeavy, result.Error);
    }

    [Fact]
    public void AddItemToBackpack_When_Exceeds_Capacity_Returns_False()
    {
        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(capacity: 130),
            new Dictionary<Slot, Tuple<IPickupable, ushort>>());

        var legs = ItemTestData.CreateBodyEquipmentItem(100, "legs");
        var body = ItemTestData.CreateBodyEquipmentItem(100, "body");

        sut.TryAddItemToSlot(Slot.Legs, legs);
        sut.TryAddItemToSlot(Slot.Body, body);

        var bp = ItemTestData.CreateBackpack();
        var bp2 = ItemTestData.CreateBackpack();

        bp.AddItem(bp2);

        var result = sut.TryAddItemToSlot(Slot.Backpack, bp);

        Assert.True(result.Succeeded);

        //act
        result = sut.TryAddItemToSlot(Slot.Backpack, ItemTestData.CreateAmmo(105, 20));

        //assert
        Assert.False(result.Succeeded);
        Assert.Equal(InvalidOperation.TooHeavy, result.Error);
    }

    [Fact]
    public void AddItemToBackpack_When_Already_Has_Backpack_Add_Item_To_It()
    {
        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(capacity: 200),
            new Dictionary<Slot, Tuple<IPickupable, ushort>>());

        var legs = ItemTestData.CreateBodyEquipmentItem(100, "legs");
        var body = ItemTestData.CreateBodyEquipmentItem(100, "body");

        sut.TryAddItemToSlot(Slot.Legs, legs);
        sut.TryAddItemToSlot(Slot.Body, body);

        var bp = ItemTestData.CreateBackpack();
        var bp2 = ItemTestData.CreateBackpack();

        bp.AddItem(bp2);

        var result = sut.TryAddItemToSlot(Slot.Backpack, bp);

        Assert.True(result.Succeeded);

        result = sut.TryAddItemToSlot(Slot.Backpack, ItemTestData.CreateAmmo(105, 20));

        Assert.Equal(105, (sut[Slot.Backpack] as IPickupableContainer)[0].ClientId);
    }

    [Fact]
    public void TotalWeight_Returns_Total_Weight_Of_Inventory()
    {
        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(capacity: 2000),
            new Dictionary<Slot, Tuple<IPickupable, ushort>>());

        var legs = ItemTestData.CreateBodyEquipmentItem(100, "legs");
        var body = ItemTestData.CreateBodyEquipmentItem(100, "body");
        var feet = ItemTestData.CreateBodyEquipmentItem(100, "feet");
        var head = ItemTestData.CreateBodyEquipmentItem(100, "head");
        var necklace = ItemTestData.CreateBodyEquipmentItem(100, "necklace");
        var ring = ItemTestData.CreateBodyEquipmentItem(100, "ring");
        var shield = ItemTestData.CreateBodyEquipmentItem(100, "shield", "shield");
        var ammo = ItemTestData.CreateAmmo(100, 100);
        var weapon = ItemTestData.CreateWeaponItem(100, "club");

        sut.TryAddItemToSlot(Slot.Legs, legs);
        sut.TryAddItemToSlot(Slot.Body, body);
        sut.TryAddItemToSlot(Slot.Feet, feet);
        sut.TryAddItemToSlot(Slot.Head, head);
        sut.TryAddItemToSlot(Slot.Necklace, necklace);
        sut.TryAddItemToSlot(Slot.Ring, ring);
        sut.TryAddItemToSlot(Slot.Right, shield);
        sut.TryAddItemToSlot(Slot.Left, weapon);
        sut.TryAddItemToSlot(Slot.Ammo, ammo);

        var container = ItemTestData.CreateBackpack();
        sut.TryAddItemToSlot(Slot.Backpack, container);

        container.AddItem(ItemTestData.CreateCumulativeItem(100, 60));

        Assert.Equal(500, sut.TotalWeight);
    }

    [Theory]
    [MemberData(nameof(SlotSwapItemsData))]
    public void AddItemToSlot_AddItem_When_Slot_Has_Item_Swap_Item(Slot slot, IPickupable item, IPickupable newItem)
    {
        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(),
            new Dictionary<Slot, Tuple<IPickupable, ushort>>());

        var result = sut.TryAddItemToSlot(slot, item);

        Assert.Null(result.Value);

        result = sut.TryAddItemToSlot(slot, newItem);

        if (item is PickupableContainer)
        {
            Assert.Same(item, sut[slot]);
            Assert.Null(result.Value);
            return;
        }

        Assert.Same(newItem, sut[slot]);
        Assert.Same(item, result.Value);
    }

    [Theory]
    [MemberData(nameof(SlotJoinItemsData))]
    public void AddItemToSlot_When_Slot_Has_Cumulative_Item_Join_Item(Slot slot, ICumulative item,
        ICumulative newItem, ICumulative resultItem)
    {
        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(capacity: 1000),
            new Dictionary<Slot, Tuple<IPickupable, ushort>>());

        var result = sut.TryAddItemToSlot(slot, item);

        Assert.Null(result.Value);

        sut.TryAddItemToSlot(slot, newItem);

        Assert.Equal(sut[slot], item);
        Assert.Equal((sut[slot] as ICumulative).Amount, resultItem.Amount);
    }

    [Fact]
    public void AddItemToSlot_When_Item_Is_Cumulative_Raises_Event_When_Reduced()
    {
        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(capacity: 1000),
            new Dictionary<Slot, Tuple<IPickupable, ushort>>());
        var item = ItemTestData.CreateAmmo(100, 100) as Ammo;
        var result = sut.TryAddItemToSlot(Slot.Ammo, item);

        var eventRaised = false;
        var itemRemovedEventRaised = false;

        item.OnReduced += (_, _) => eventRaised = true;
        sut.OnItemRemovedFromSlot += (_, _, _, _) =>
            itemRemovedEventRaised = true;

        item.Throw();

        Assert.True(eventRaised);
        Assert.True(itemRemovedEventRaised);
    }

    [Fact]
    public void AddItemToSlot_When_Swap_Item_Is_Cumulative_Raises_Event_When_Reduced()
    {
        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(capacity: 1000),
            new Dictionary<Slot, Tuple<IPickupable, ushort>>());
        var initialItem = ItemTestData.CreateAmmo(101, 1) as Ammo;
        var item = ItemTestData.CreateAmmo(100, 100) as Ammo;

        sut.TryAddItemToSlot(Slot.Ammo, initialItem);
        var result = sut.TryAddItemToSlot(Slot.Ammo, item);

        var eventRaised = false;
        var itemRemovedEventRaised = false;

        item.OnReduced += (_, _) => eventRaised = true;
        sut.OnItemRemovedFromSlot += (_, _, _, _) =>
            itemRemovedEventRaised = true;
        item.Throw();

        Assert.True(eventRaised);
        Assert.True(itemRemovedEventRaised);
    }

    [Fact]
    public void TryAddItemToSlot_SwappedItem_Should_Not_Raise_Event_When_Reduced()
    {
        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(capacity: 1000),
            new Dictionary<Slot, Tuple<IPickupable, ushort>>());
        var initialItem = ItemTestData.CreateAmmo(101, 3) as Ammo;
        var item = ItemTestData.CreateAmmo(100, 100) as Ammo;

        sut.TryAddItemToSlot(Slot.Ammo, initialItem);
        var result = sut.TryAddItemToSlot(Slot.Ammo, item);

        var itemRemovedEventRaised = false;

        var swapped = result.Value as Ammo;

        sut.OnItemRemovedFromSlot += (_, _, _, _) => itemRemovedEventRaised = true;

        swapped.Throw();

        Assert.False(itemRemovedEventRaised);
    }

    [Fact]
    public void
        AddItemToSlot_AddItem_When_Slot_Has_Cumulative_Item_And_Exceeds_Join_Item_And_Returns_Exceeding_Amount()
    {
        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(capacity: 1000),
            new Dictionary<Slot, Tuple<IPickupable, ushort>>());

        var result = sut.TryAddItemToSlot(Slot.Ammo, ItemTestData.CreateAmmo(100, 50));

        Assert.Null(result.Value);

        result = sut.TryAddItemToSlot(Slot.Ammo, ItemTestData.CreateAmmo(100, 80));

        Assert.Equal(30, (result.Value as ICumulative).Amount);
    }

    [Theory]
    [MemberData(nameof(BackpackSlotAddItemsData))]
    public void AddItemToSlot_When_BackpackSlot_Has_Backpack_Add_Item_To_It(IPickupable item)
    {
        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(capacity: 1000),
            new Dictionary<Slot, Tuple<IPickupable, ushort>>());
        var backpack = ItemTestData.CreateBackpack();

        sut.TryAddItemToSlot(Slot.Backpack, backpack);

        var result = sut.TryAddItemToSlot(Slot.Backpack, item);

        Assert.Null(result.Value);

        Assert.Equal(sut[Slot.Backpack], backpack);
        Assert.Equal(1, backpack.SlotsUsed);
    }

    [Theory]
    [MemberData(nameof(SlotItemsData))]
    public void AddItemToSlot_Changes_Item_Location(Slot slot, IPickupable item)
    {
        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(capacity: 1000),
            new Dictionary<Slot, Tuple<IPickupable, ushort>>());

        var result = sut.TryAddItemToSlot(slot, item);

        Assert.Equal(Location.Inventory(slot), item.Location);
    }
    
    [Fact]
    public void CanAddItem_When_Item_Is_Not_Dressable_Returns_Error()
    {
        var item = ItemTestData.CreateRegularItem(1);
        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(capacity: 1000),
            new Dictionary<Slot, Tuple<IPickupable, ushort>>());

        var result = sut.CanAddItem(item.Metadata);

        Assert.False(result.Succeeded);
        Assert.Equal(InvalidOperation.NotEnoughRoom, result.Error);
    }

    [InlineData("head")]
    [InlineData("body")]
    [InlineData("weapon")]
    [InlineData("shield")]
    [InlineData("necklace")]
    [InlineData("ring")]
    [InlineData("backpack")]
    [InlineData("feet")]
    [InlineData("ammo")]
    [Theory]
    public void CanAddItem_When_Slot_Is_Empty_And_Adding_Regular_Item_Returns_Success(string bodyPosition)
    {
        var item = ItemTestData.CreateBodyEquipmentItem(1, bodyPosition);
        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(capacity: 1000),
            new Dictionary<Slot, Tuple<IPickupable, ushort>>());

        var result = sut.CanAddItem(item.Metadata);

        Assert.True(result.Succeeded);
        Assert.Equal((uint)1, result.Value);
    }

    [Fact]
    public void CanAddItem_When_Slot_Is_Not_Empty_And_Adding_Regular_Item_Returns_Not_Enough_Room()
    {
        var bodyItem = ItemTestData.CreateBodyEquipmentItem(1, "body");
        var weapon = ItemTestData.CreateBodyEquipmentItem(3, "weapon");

        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(capacity: 1000),
            new Dictionary<Slot, Tuple<IPickupable, ushort>>
            {
                { Slot.Body, new Tuple<IPickupable, ushort>(bodyItem, 1) },
                { Slot.Left, new Tuple<IPickupable, ushort>(weapon, 3) }
            });

        var bodyItemToAdd = ItemTestData.CreateBodyEquipmentItem(1, "body");
        var weaponToAdd = ItemTestData.CreateBodyEquipmentItem(2, "weapon");

        var result1 = sut.CanAddItem(bodyItemToAdd.Metadata);
        var result2 = sut.CanAddItem(weaponToAdd.Metadata);

        Assert.False(result1.Succeeded);
        Assert.Equal(InvalidOperation.NotEnoughRoom, result1.Error);

        Assert.False(result2.Succeeded);
        Assert.Equal(InvalidOperation.NotEnoughRoom, result2.Error);
    }

    [InlineData("head")]
    [InlineData("body")]
    [InlineData("weapon")]
    [InlineData("shield")]
    [InlineData("necklace")]
    [InlineData("ring")]
    [InlineData("backpack")]
    [InlineData("feet")]
    [InlineData("ammo")]
    [Theory]
    public void CanAddItem_When_Slot_Is_Empty_And_Adding_Cumulative_Item_Returns_Success(string bodyPosition)
    {
        var item = ItemTestData.CreateCumulativeItem(1, 100, bodyPosition);
        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(capacity: 1000),
            new Dictionary<Slot, Tuple<IPickupable, ushort>>());

        var result = sut.CanAddItem(item.Metadata);

        Assert.True(result.Succeeded);
        Assert.Equal((uint)100, result.Value);
    }

    [InlineData(40, 60)]
    [InlineData(70, 30)]
    [InlineData(99, 1)]
    [InlineData(1, 99)]
    [InlineData(5, 95)]
    [InlineData(50, 50)]
    [Theory]
    public void CanAddItem_When_Slot_Has_Same_Cumulative_Item_And_Adding_Cumulative_Item_Returns_Success(byte amount,
        uint expected)
    {
        var item = ItemTestData.CreateAmmo(1, 100);
        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(capacity: 1000),
            new Dictionary<Slot, Tuple<IPickupable, ushort>>
            {
                { Slot.Ammo, new Tuple<IPickupable, ushort>(ItemTestData.CreateAmmo(1, amount), 1) }
            });

        var result = sut.CanAddItem(item.Metadata);

        Assert.True(result.Succeeded);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void CanAddItem_When_Slot_Has_Different_Cumulative_Item_And_Adding_Cumulative_Item_Returns_Not_Enough_Room()
    {
        var item = ItemTestData.CreateAmmo(2, 100);
        var sut = InventoryTestDataBuilder.Build(PlayerTestDataBuilder.Build(capacity: 1000),
            new Dictionary<Slot, Tuple<IPickupable, ushort>>
            {
                { Slot.Ammo, new Tuple<IPickupable, ushort>(ItemTestData.CreateAmmo(1, 50), 1) }
            });

        var result = sut.CanAddItem(item.Metadata);

        Assert.False(result.Succeeded);
        Assert.Equal(InvalidOperation.NotEnoughRoom, result.Error);
    }

    [Fact]
    public void CanAddItem_When_PlayerHasNoRequiredLevel_ReturnsFalse()
    {
        //arrange
        var skills = PlayerTestDataBuilder.GenerateSkills(10);
        var sut = PlayerTestDataBuilder.Build(skills: skills);

        var bodyItemToAdd = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
        {
            (ItemAttribute.MinimumLevel, 1000)
        });

        //act
        var actual = sut.Inventory.AddItem(bodyItemToAdd, (byte)Slot.Body);

        //assert
        actual.Error.Should().Be(InvalidOperation.CannotDress);
    }

    [Fact]
    public void DressingItems_PlayerHasAllItems_ReturnAllExceptBag()
    {
        //arrange
        var inventory = PlayerTestDataBuilder.GenerateInventory();
        var player = PlayerTestDataBuilder.Build(inventoryMap: inventory, capacity: 500_000);
        var expected = inventory.Where(x => x.Key != Slot.Backpack).Select(x => (IItem)x.Value.Item1);

        //assert
        player.Inventory.DressingItems.Should().HaveSameCount(expected);
        player.Inventory.DressingItems.Should().ContainSingle(x => x == inventory[Slot.Head].Item1);
        player.Inventory.DressingItems.Should().ContainSingle(x => x == inventory[Slot.Necklace].Item1);
        player.Inventory.DressingItems.Should().ContainSingle(x => x == inventory[Slot.Ring].Item1);
        player.Inventory.DressingItems.Should().ContainSingle(x => x == inventory[Slot.Body].Item1);
        player.Inventory.DressingItems.Should().ContainSingle(x => x == inventory[Slot.Left].Item1);
        player.Inventory.DressingItems.Should().ContainSingle(x => x == inventory[Slot.Ammo].Item1);
        player.Inventory.DressingItems.Should().ContainSingle(x => x == inventory[Slot.Legs].Item1);
        player.Inventory.DressingItems.Should().ContainSingle(x => x == inventory[Slot.Ring].Item1);
        player.Inventory.DressingItems.Should().ContainSingle(x => x == inventory[Slot.Feet].Item1);
    }
}