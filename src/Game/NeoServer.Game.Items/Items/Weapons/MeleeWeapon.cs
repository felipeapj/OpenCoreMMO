﻿using System;
using NeoServer.Game.Combat.Attacks;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types.Body;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Parsers;
using NeoServer.Game.Items.Bases;

namespace NeoServer.Game.Items.Items.Weapons;

public class MeleeWeapon : Equipment, IWeaponItem
{
    public MeleeWeapon(IItemType itemType, Location location) : base(itemType, location)
    {
        //AllowedVocations  todo
    }

    protected override string PartialInspectionText
    {
        get
        {
            var defense = Metadata.Attributes.GetAttribute<byte>(ItemAttribute.Defense);
            var extraDefense = Metadata.Attributes.GetAttribute<sbyte>(ItemAttribute.ExtraDefense);

            var extraDefenseText = extraDefense > 0 ? $" +{extraDefense}" :
                extraDefense < 0 ? $" -{extraDefense}" : string.Empty;

            var elementalDamageText = ElementalDamage is not null && ElementalDamage.Item2 > 0
                ? $" + {ElementalDamage.Item2} {DamageTypeParser.Parse(ElementalDamage.Item1)},"
                : ",";

            return $"Atk: {Attack}{elementalDamageText} Def: {defense}{extraDefenseText}";
        }
    }

    public override bool CanBeDressed(IPlayer player)
    {
        if (Guard.IsNullOrEmpty(Vocations)) return true;

        foreach (var vocation in Vocations)
            if (vocation == player.VocationType)
                return true;

        return false;
    }

    public ushort Attack => Metadata.Attributes.GetAttribute<ushort>(ItemAttribute.Attack);

    public Tuple<DamageType, byte> ElementalDamage => Metadata.Attributes.GetWeaponElementDamage();

    public sbyte ExtraDefense => Metadata.Attributes.GetAttribute<sbyte>(ItemAttribute.ExtraDefense);

    public bool Use(ICombatActor actor, ICombatActor enemy, out CombatAttackResult combatResult)
    {
        combatResult = new CombatAttackResult(DamageType.Melee);

        if (actor is not IPlayer player) return false;

        var result = false;

        if (Attack > 0)
        {
            var maxDamage = player.CalculateAttackPower(0.085f, Attack);
            var combat = new CombatAttackValue(actor.MinimumAttackPower,
                player.CalculateAttackPower(0.085f, Attack), DamageType.Melee);
            if (MeleeCombatAttack.CalculateAttack(actor, enemy, combat, out var damage))
            {
                enemy.ReceiveAttack(actor, damage);
                result = true;
            }
        }

        if (ElementalDamage is null) return result;

        {
            var combat = new CombatAttackValue(actor.MinimumAttackPower,
                player.CalculateAttackPower(0.085f, ElementalDamage.Item2), ElementalDamage.Item1);

            if (MeleeCombatAttack.CalculateAttack(actor, enemy, combat, out var damage))
            {
                enemy.ReceiveAttack(actor, damage);
                result = true;
            }
        }

        return result;
    }

    public static bool IsApplicable(IItemType type)
    {
        return type.WeaponType switch
        {
            WeaponType.Axe => true,
            WeaponType.Club => true,
            WeaponType.Sword => true,
            _ => false
        };
    }
}