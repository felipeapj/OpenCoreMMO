﻿using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Item;

namespace NeoServer.Extensions.Spells.Attack
{
    public class IceWave : WaveSpell
    {
        protected override string AreaName => "AREA_WAVE4";
        public override DamageType DamageType => DamageType.Ice;
        public override MinMax CalculateDamage(ICombatActor actor) => new(5, 100);
        public override byte Range => 1;
    }
}