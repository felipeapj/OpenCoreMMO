﻿using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Item;

namespace NeoServer.Game.Common.Effects.Parsers;

public static class DamageEffectParser
{
    public static EffectT Parse(DamageType damageType)
    {
        return damageType switch
        {
            DamageType.Fire => EffectT.Flame,
            DamageType.FireField => EffectT.Flame,
            DamageType.Energy => EffectT.DamageEnergy,
            DamageType.Melee => EffectT.XBlood,
            DamageType.Physical => EffectT.XGray,
            DamageType.Earth => EffectT.RingsGreen,
            DamageType.LifeDrain => EffectT.GlitterRed,
            DamageType.ManaDrain => EffectT.GlitterRed,
            DamageType.Death => EffectT.BubbleBlack,
            DamageType.Holy => EffectT.HolyDamage,
            DamageType.Ice => EffectT.IceAttack,
            _ => EffectT.None
        };
    }

    public static EffectT Parse(DamageType damageType, ICreature creature)
    {
        if (damageType is DamageType.Melee && creature is IMonster monster)
        {
            return monster.Metadata.Race switch
            {
                Race.Venom => EffectT.XPoison,
                Race.Undead=>EffectT.XGray,
                Race.Energy=>EffectT.RingsBlue,
                _ => EffectT.XBlood
            };
        }

        return Parse(damageType);
    }
}