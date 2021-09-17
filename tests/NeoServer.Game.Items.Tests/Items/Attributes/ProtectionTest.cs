﻿using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Items.Items;
using NeoServer.Game.Items.Items.Attributes;
using NeoServer.Game.Tests.Helpers;
using Xunit;

namespace NeoServer.Game.Items.Tests.Items.Attributes
{
    public class ProtectionTest
    {
        [Fact]
        public void DressedIn_When_Has_Damage_Protection_Should_Reduce_Damage()
        {
            var sut = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
            {
                (ItemAttribute.AbsorbPercentFire, 20)
            }, charges: 10);

            var player = PlayerTestDataBuilder.BuildPlayer();
            var enemy = PlayerTestDataBuilder.BuildPlayer();

            sut.DressedIn(player);

            var resultDamage = 0;
            player.OnInjured += (_, _, damage) => { resultDamage = damage.Damage; };

            var damage = new CombatDamage(200, DamageType.Fire);
            player.ReceiveAttack(enemy, damage);

            Assert.Equal(160, resultDamage);
        }

        [Fact]
        public void DressedIn_When_Has_No_Damage_Protection_Should_Not_Reduce_Damage()
        {
            var itemType = new Mock<IItemType>();

            itemType.SetupGet(x => x.Attributes).Returns(new ItemAttributeList());

            itemType.SetupGet(x => x.Attributes.DamageProtection).Returns(new Dictionary<DamageType, sbyte>
            {
                {DamageType.Earth, 20}
            });

            var sut = new BodyDefenseEquipment(itemType.Object, Location.Zero);

            var player = PlayerTestDataBuilder.BuildPlayer();
            var enemy = PlayerTestDataBuilder.BuildPlayer();

            sut.DressedIn(player);

            var resultDamage = 0;
            player.OnInjured += (_, _, damage) => { resultDamage = damage.Damage; };

            var damage = new CombatDamage(200, DamageType.Fire);
            player.ReceiveAttack(enemy, damage);

            Assert.Equal(200, resultDamage);
        }

        [Fact]
        public void DressedIn_When_Has_100_Percent_Damage_Protection_Should_Reduce_Damage_To_0()
        {
            var sut = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
            {
                (ItemAttribute.AbsorbPercentFire, 100)
            }, charges: 10);

            var player = PlayerTestDataBuilder.BuildPlayer();
            var enemy = PlayerTestDataBuilder.BuildPlayer();

            sut.DressedIn(player);

            var resultDamage = 0;
            player.OnInjured += (_, _, damage) => { resultDamage = damage.Damage; };

            var damage = new CombatDamage(200, DamageType.Fire);
            player.ReceiveAttack(enemy, damage);

            Assert.Equal(0, resultDamage);
        }


        [Fact]
        public void DressedIn_100PercentDamageProtection_From_Different_Attack_Should_Not_Reduce_Damage()
        {
            var sut = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
            {
                (ItemAttribute.AbsorbPercentFire, 100)
            }, charges: 10);

            var player = PlayerTestDataBuilder.BuildPlayer();
            var enemy = PlayerTestDataBuilder.BuildPlayer();

            sut.DressedIn(player);

            var resultDamage = 0;
            player.OnInjured += (_, _, damage) => { resultDamage = damage.Damage; };

            var damage = new CombatDamage(200, DamageType.Energy);
            player.ReceiveAttack(enemy, damage);

            resultDamage.Should().BeGreaterThan(0);
        }

        [Fact]
        public void UndressFrom_When_Receive_Damage_Should_Not_Reduce_Damage()
        {
            var sut = ItemTestData.CreateDefenseEquipmentItem(1, attributes: new (ItemAttribute, IConvertible)[]
            {
                (ItemAttribute.AbsorbPercentFire, 100)
            }, charges: 10);

            var player = PlayerTestDataBuilder.BuildPlayer();
            var enemy = PlayerTestDataBuilder.BuildPlayer();

            sut.DressedIn(player);

            var resultDamage = 0;
            player.OnInjured += (_, _, damage) => { resultDamage = damage.Damage; };

            var damage = new CombatDamage(200, DamageType.Fire);
            player.ReceiveAttack(enemy, damage);

            Assert.Equal(0, resultDamage);

            sut.UndressFrom(player);

            player.ReceiveAttack(enemy, damage);

            Assert.Equal(200, resultDamage);
        }
        [Fact]
        public void Decrease_DefendedAttack_DecreaseCharges()
        {
            //arrange
            var defender = PlayerTestDataBuilder.BuildPlayer();
            var attacker = PlayerTestDataBuilder.BuildPlayer();

            var hmm = ItemTestData.CreateAttackRune(1, damageType: DamageType.Energy);

            var sut = ItemTestData.CreateDefenseEquipmentItem(1, charges: 50, attributes: new (ItemAttribute, IConvertible)[]
            {
                (ItemAttribute.AbsorbPercentEnergy,10)
            });

            sut.DressedIn(defender);

            //act
            attacker.Attack(defender, hmm);

            //assert
            sut.Charges.Should().Be(49);
        }

        [Fact]
        public void Decrease_DefendedDifferentAttack_DoNotDecreaseCharges()
        {
            //arrange
            var defender = PlayerTestDataBuilder.BuildPlayer();
            var attacker = PlayerTestDataBuilder.BuildPlayer();

            var hmm = ItemTestData.CreateAttackRune(1, damageType: DamageType.Energy);

            var sut = ItemTestData.CreateDefenseEquipmentItem(1, charges: 50, attributes: new (ItemAttribute, IConvertible)[]
            {
                (ItemAttribute.AbsorbPercentFire,100)
            });

            sut.DressedIn(defender);

            //act
            attacker.Attack(defender, hmm);

            //assert
            sut.Charges.Should().Be(50);
        }
        [Fact]
        public void Protect_InfiniteCharges_Protect()
        {
            //arrange
            var defender = PlayerTestDataBuilder.BuildPlayer();
            var attacker = PlayerTestDataBuilder.BuildPlayer();
            var oldHp = defender.HealthPoints;

            var totalDamage = 0;
            defender.OnInjured += (enemy, victim, damage) =>
            {
                totalDamage = damage.Damage;
            };

            var hmm = ItemTestData.CreateAttackRune(1, damageType: DamageType.Energy, min: 100, max: 100);

            var sut = ItemTestData.CreateDefenseEquipmentItem(1, charges: 0,
                attributes: new (ItemAttribute, IConvertible)[]
                {
                    (ItemAttribute.AbsorbPercentEnergy, 100),
                    (ItemAttribute.Duration, 100)
                });
            sut.DressedIn(defender);

            //act
            attacker.Attack(defender, hmm);

            //assert
            totalDamage.Should().Be(0);
            defender.HealthPoints.Should().Be(oldHp);
        }

        [Fact]
        public void Protect_NoCharges_DoNotProtect()
        {
            //arrange
            var defender = PlayerTestDataBuilder.BuildPlayer();
            var attacker = PlayerTestDataBuilder.BuildPlayer();
            var oldHp = defender.HealthPoints;

            var totalDamage = 0;
            defender.OnInjured += (enemy, victim, damage) =>
            {
                totalDamage = damage.Damage;
            };

            var hmm = ItemTestData.CreateAttackRune(1, damageType: DamageType.Energy, min: 100, max: 100);

            var sut = ItemTestData.CreateDefenseEquipmentItem(1, charges: 1);
            sut.Metadata.Attributes.SetAttribute(ItemAttribute.AbsorbPercentEnergy, 100);
            sut.DressedIn(defender);

            //act
            attacker.Attack(defender, hmm);
            attacker.Attack(defender, hmm);

            //assert
            totalDamage.Should().NotBe(0);
            defender.HealthPoints.Should().BeLessThan(oldHp);
        }
        [Fact]
        public void Protect_1Charge_ProtectFromDamage()
        {
            //arrange
            var defender = PlayerTestDataBuilder.BuildPlayer();
            var attacker = PlayerTestDataBuilder.BuildPlayer();

            var oldHp = defender.HealthPoints;

            var totalDamage = 0;
            defender.OnInjured += (_, _, damage) =>
            {
                totalDamage = damage.Damage;
            };

            var hmm = ItemTestData.CreateAttackRune(1, damageType: DamageType.Energy, min: 100, max: 100);

            var sut = ItemTestData.CreateDefenseEquipmentItem(1, charges: 1, attributes: new (ItemAttribute, IConvertible)[]
            {
                (ItemAttribute.AbsorbPercentEnergy, 100),
                (ItemAttribute.Duration, 100)
            });
            sut.DressedIn(defender);

            //act
            attacker.Attack(defender, hmm);

            //assert
            totalDamage.Should().Be(0);
            defender.HealthPoints.Should().Be(oldHp);
        }

        [Theory]
        [InlineData(-100, 400, 100)]
        [InlineData(-50, 300, 200)]
        [InlineData(-5, 210, 290)]
        public void DressedIn_When_Player_Has_Negative_Damage_Protection_Should_Increase_Damage(sbyte protection, ushort expectedDamage, ushort remainingHp)
        {
            //arrange
            var defender = PlayerTestDataBuilder.BuildPlayer(hp: 500);
            var attacker = PlayerTestDataBuilder.BuildPlayer();

            var totalDamage = 0;
            defender.OnInjured += (enemy, victim, damage) =>
            {
                totalDamage = damage.Damage;
            };

            var hmm = ItemTestData.CreateAttackRune(1, damageType: DamageType.Energy, min: 100, max: 100);

            var sut = ItemTestData.CreateDefenseEquipmentItem(1, charges: 10,
                attributes: new (ItemAttribute, IConvertible)[]
                {
                    (ItemAttribute.AbsorbPercentEnergy, protection),
                });
            sut.DressedIn(defender);

            //act
            var damage = new CombatDamage(200, DamageType.Energy);
            defender.ReceiveAttack(attacker, damage);

            //assert
            totalDamage.Should().Be(expectedDamage);
            defender.HealthPoints.Should().Be(remainingHp);
        }

        [Fact]
        public void DressedIn_ManaDrainDamageProtection_DecreaseDamage()
        {
            //arrange
            var defender = PlayerTestDataBuilder.BuildPlayer(hp: 500, mana: 500);
            var attacker = PlayerTestDataBuilder.BuildPlayer();
            var oldHp = defender.HealthPoints;

            var totalDamage = 0;
            defender.OnInjured += (enemy, victim, damage) =>
            {
                totalDamage = damage.Damage;
            };

            var sut = ItemTestData.CreateDefenseEquipmentItem(1, charges: 10,
                attributes: new (ItemAttribute, IConvertible)[]
                {
                    (ItemAttribute.AbsorbPercentManaDrain, 10),
                });
            sut.DressedIn(defender);

            //act
            var damage = new CombatDamage(200, DamageType.ManaDrain);
            defender.ReceiveAttack(attacker, damage);

            //assert
            totalDamage.Should().Be(180);
            defender.HealthPoints.Should().Be(500);
            defender.Mana.Should().Be(320);

        }
        [Fact]
        public void DressedIn_LifeDrainDamageProtection_DecreaseDamage()
        {
            //arrange
            var defender = PlayerTestDataBuilder.BuildPlayer(hp: 500, mana: 500);
            var attacker = PlayerTestDataBuilder.BuildPlayer();
            var oldHp = defender.HealthPoints;

            var totalDamage = 0;
            defender.OnInjured += (enemy, victim, damage) =>
            {
                totalDamage = damage.Damage;
            };

            var sut = ItemTestData.CreateDefenseEquipmentItem(1, charges: 10,
                attributes: new (ItemAttribute, IConvertible)[]
                {
                    (ItemAttribute.AbsorbPercentLifeDrain, 10),
                });
            sut.DressedIn(defender);

            //act
            var damage = new CombatDamage(200, DamageType.LifeDrain);
            defender.ReceiveAttack(attacker, damage);

            //assert
            totalDamage.Should().Be(180);
            defender.HealthPoints.Should().Be(320);
        }

        [Theory]
        [InlineData(DamageType.Energy, ItemAttribute.AbsorbPercentEnergy)]
        [InlineData(DamageType.Fire, ItemAttribute.AbsorbPercentFire)]
        [InlineData(DamageType.Drown, ItemAttribute.AbsorbPercentDrown)]
        [InlineData(DamageType.Holy, ItemAttribute.AbsorbPercentHoly)]
        [InlineData(DamageType.Ice, ItemAttribute.AbsorbPercentIce)]
        [InlineData(DamageType.ManaDrain, ItemAttribute.AbsorbPercentManaDrain)]
        [InlineData(DamageType.Earth, ItemAttribute.AbsorbPercentPoison)]
        [InlineData(DamageType.Death, ItemAttribute.AbsorbPercentDeath)]
        [InlineData(DamageType.LifeDrain, ItemAttribute.AbsorbPercentLifeDrain)]
        [InlineData(DamageType.Physical, ItemAttribute.AbsorbPercentPhysical)]
        [InlineData(DamageType.Melee, ItemAttribute.AbsorbPercentPhysical)]
        public void DressedIn_DamageProtection_DecreaseDamage(DamageType damageType, ItemAttribute protectionAttribute)
        {
            //arrange
            var defender = PlayerTestDataBuilder.BuildPlayer(hp: 500, mana: 500);
            var attacker = PlayerTestDataBuilder.BuildPlayer();

            var totalDamage = 0;
            defender.OnInjured += (enemy, victim, damage) =>
            {
                totalDamage = damage.Damage;
            };

            var sut = ItemTestData.CreateDefenseEquipmentItem(1, charges: 10,
                attributes: new (ItemAttribute, IConvertible)[]
                {
                    (protectionAttribute, 10),
                });
            sut.DressedIn(defender);

            //act
            var damage = new CombatDamage(200, damageType);
            defender.ReceiveAttack(attacker, damage);

            //assert
            totalDamage.Should().Be(180);
        }

        [Theory]
        [InlineData(DamageType.Energy, ItemAttribute.AbsorbPercentElements)]
        [InlineData(DamageType.Fire, ItemAttribute.AbsorbPercentElements)]
        [InlineData(DamageType.Drown, ItemAttribute.AbsorbPercentElements)]
        [InlineData(DamageType.Holy, ItemAttribute.AbsorbPercentElements)]
        [InlineData(DamageType.Ice, ItemAttribute.AbsorbPercentElements)]
        [InlineData(DamageType.ManaDrain, ItemAttribute.AbsorbPercentElements)]
        [InlineData(DamageType.Earth, ItemAttribute.AbsorbPercentElements)]
        [InlineData(DamageType.Death, ItemAttribute.AbsorbPercentElements)]
        [InlineData(DamageType.LifeDrain, ItemAttribute.AbsorbPercentElements)]
        [InlineData(DamageType.Physical, ItemAttribute.AbsorbPercentElements, 200)]
        [InlineData(DamageType.Melee, ItemAttribute.AbsorbPercentElements, 200)]
        public void DressedIn_ElementalDamageProtection_DecreaseDamage(DamageType damageType, ItemAttribute protectionAttribute, ushort expectedDamage = 180)
        {
            //arrange
            var defender = PlayerTestDataBuilder.BuildPlayer(hp: 500, mana: 500);
            var attacker = PlayerTestDataBuilder.BuildPlayer();

            var totalDamage = 0;
            defender.OnInjured += (enemy, victim, damage) =>
            {
                totalDamage = damage.Damage;
            };

            var sut = ItemTestData.CreateDefenseEquipmentItem(1, charges: 10,
                attributes: new (ItemAttribute, IConvertible)[]
                {
                    (protectionAttribute, 10),
                });
            sut.DressedIn(defender);

            //act
            var damage = new CombatDamage(200, damageType);
            defender.ReceiveAttack(attacker, damage);

            //assert
            totalDamage.Should().Be(expectedDamage);
        }

        [Theory]
        [InlineData(DamageType.Energy, ItemAttribute.AbsorbPercentAll)]
        [InlineData(DamageType.Fire, ItemAttribute.AbsorbPercentAll)]
        [InlineData(DamageType.Drown, ItemAttribute.AbsorbPercentAll)]
        [InlineData(DamageType.Holy, ItemAttribute.AbsorbPercentAll)]
        [InlineData(DamageType.Ice, ItemAttribute.AbsorbPercentAll)]
        [InlineData(DamageType.ManaDrain, ItemAttribute.AbsorbPercentAll)]
        [InlineData(DamageType.Earth, ItemAttribute.AbsorbPercentAll)]
        [InlineData(DamageType.Death, ItemAttribute.AbsorbPercentAll)]
        [InlineData(DamageType.LifeDrain, ItemAttribute.AbsorbPercentAll)]
        [InlineData(DamageType.Physical, ItemAttribute.AbsorbPercentAll)]
        [InlineData(DamageType.Melee, ItemAttribute.AbsorbPercentAll)]
        public void DressedIn_AllDamageProtection_DecreaseDamage(DamageType damageType, ItemAttribute protectionAttribute)
        {
            //arrange
            var defender = PlayerTestDataBuilder.BuildPlayer(hp: 500, mana: 500);
            var attacker = PlayerTestDataBuilder.BuildPlayer();

            var totalDamage = 0;
            defender.OnInjured += (enemy, victim, damage) =>
            {
                totalDamage = damage.Damage;
            };

            var sut = ItemTestData.CreateDefenseEquipmentItem(1, charges: 10,
                attributes: new (ItemAttribute, IConvertible)[]
                {
                    (protectionAttribute, 10),
                });
            sut.DressedIn(defender);

            //act
            var damage = new CombatDamage(200, damageType);
            defender.ReceiveAttack(attacker, damage);

            //assert
            totalDamage.Should().Be(180);
        }


        [Fact]
        public void DressedIn_HasAllAndDeathDamageProtection_DecreaseDamageAccordingly()
        {
            //arrange
            var defender = PlayerTestDataBuilder.BuildPlayer(hp: 500, mana: 500);
            var attacker = PlayerTestDataBuilder.BuildPlayer();

            var totalDamage = 0;
            defender.OnInjured += (enemy, victim, damage) =>
            {
                totalDamage = damage.Damage;
            };

            var sut = ItemTestData.CreateDefenseEquipmentItem(1, charges: 10,
                attributes: new (ItemAttribute, IConvertible)[]
                {
                    (ItemAttribute.AbsorbPercentAll, 10),
                    (ItemAttribute.AbsorbPercentDeath, 50),
                });
            sut.DressedIn(defender);

            //act
            var damage = new CombatDamage(200, DamageType.Death);
            defender.ReceiveAttack(attacker, damage);

            //assert
            totalDamage.Should().Be(100);

            //act
            var fireDamage = new CombatDamage(200, DamageType.Fire);
            defender.ReceiveAttack(attacker, fireDamage);

            //assert
            totalDamage.Should().Be(180);
        }
        [Fact]
        public void DressedIn_HasElementsAndDeathDamageProtection_DecreaseDamageAccordingly()
        {
            //arrange
            var defender = PlayerTestDataBuilder.BuildPlayer(hp: 500, mana: 500);
            var attacker = PlayerTestDataBuilder.BuildPlayer();

            var totalDamage = 0;
            defender.OnInjured += (enemy, victim, damage) =>
            {
                totalDamage = damage.Damage;
            };

            var sut = ItemTestData.CreateDefenseEquipmentItem(1, charges: 10,
                attributes: new (ItemAttribute, IConvertible)[]
                {
                    (ItemAttribute.AbsorbPercentElements, 10),
                    (ItemAttribute.AbsorbPercentDeath, 50),
                });
            sut.DressedIn(defender);

            //act
            var damage = new CombatDamage(200, DamageType.Death);
            defender.ReceiveAttack(attacker, damage);

            //assert
            totalDamage.Should().Be(100);

            //act
            var fireDamage = new CombatDamage(200, DamageType.Fire);
            defender.ReceiveAttack(attacker, fireDamage);

            //assert
            totalDamage.Should().Be(180);

            //act
            var meleeDamage = new CombatDamage(200, DamageType.Melee);
            defender.ReceiveAttack(attacker, meleeDamage);

            //assert
            totalDamage.Should().Be(200);
        }
        [Fact]
        public void ToString_ReturnsLookText()
        {
            //arrange
            var sut = new Protection(new Dictionary<DamageType, sbyte>()
            {
                [DamageType.Energy] = 10,
                [DamageType.Fire] = 20,
                [DamageType.Death] = -25,
                [DamageType.ManaDrain] = 30,
                [DamageType.LifeDrain] = 45,
                [DamageType.Ice] = 50,
                [DamageType.Physical] = -65,
                [DamageType.Drown] = 80,
                [DamageType.Earth] = 100
            });

            //assert
            sut.ToString().Should().Be("protection energy 10%, fire 20%, death -25%, mana drain 30%, life drain 45%, ice 50%, physical -65%, drown 80%, earth 100%");
        }
        [Fact]
        public void ToString_AllProtection_ReturnsLookText()
        {
            //arrange
            var sut = new Protection(new Dictionary<DamageType, sbyte>()
            {
                [DamageType.All] = 10
            });

            //assert
            sut.ToString().Should().Be("protection all 10%");
        }
        [Fact]
        public void ToString_ElementalProtection_ReturnsLookText()
        {
            //arrange
            var sut = new Protection(new Dictionary<DamageType, sbyte>()
            {
                [DamageType.Elemental] = 10
            });

            //assert
            sut.ToString().Should().Be("protection elemental 10%");
        }
        [Fact]
        public void ToString_0Protection_Ignores()
        {
            //arrange
            var sut = new Protection(new Dictionary<DamageType, sbyte>()
            {
                [DamageType.Elemental] = 10,
                [DamageType.Death] = 0
            });

            //assert
            sut.ToString().Should().Be("protection elemental 10%");
        }
    }
}