﻿using System;
using System.Linq;
using System.Reflection;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Effects;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Location;
using NeoServer.Loaders.Interfaces;

namespace NeoServer.Loaders.Effects;

public class AreaTypeLoader : IStartupLoader
{
    private readonly IAreaEffectStore _areaEffectStore;

    public AreaTypeLoader(IAreaEffectStore areaEffectStore)
    {
        _areaEffectStore = areaEffectStore;
    }

    public void Load()
    {
        var types = AppDomain.CurrentDomain.GetAssemblies();
        var fields = types.SelectMany(x => x.GetTypes()).SelectMany(x => x.GetFields())
            .Where(prop => prop.IsDefined(typeof(AreaEffectAttribute), false));

        foreach (var field in fields)
        {
            var attr = field.GetCustomAttribute<AreaEffectAttribute>();
            if (attr is null) continue; 

            if (attr.HasRotation)
            {
                var areas = Rotate(attr.Name,(byte[,])field.GetValue(null));
                _areaEffectStore.Add(attr.Name, field, areas);
                continue;
            }
            
            _areaEffectStore.Add(attr.Name, field);
        }
    }
    
    private static byte[][,] Rotate(string name, byte[,] area)
    {
        var areas = new byte[4][,];
        
        areas[(byte)Direction.West] = area;
        areas[(byte)Direction.North] = area.Rotate();
        areas[(byte)Direction.East] = areas[(byte)Direction.North].Rotate();
        areas[(byte)Direction.South] =  areas[(byte)Direction.East].Rotate();

        return areas;
    }
}