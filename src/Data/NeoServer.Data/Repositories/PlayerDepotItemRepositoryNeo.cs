﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NeoServer.Data.Contexts;
using NeoServer.Data.Interfaces;
using NeoServer.Data.Model;
using Serilog;

namespace NeoServer.Data.Repositories
{
    public class PlayerDepotItemRepositoryNeo : BaseRepository<PlayerDepotItemModel>,
        IPlayerDepotItemRepositoryNeo
    {
        #region constructors

        public PlayerDepotItemRepositoryNeo(DbContextOptions<NeoContext> contextOptions, ILogger logger) : base(contextOptions,
            logger)
        {
        }

        #endregion

        #region public methods implementation

        public async Task<IEnumerable<PlayerDepotItemModel>> GetByPlayerId(uint id)
        {
            await using var context = NewDbContext;
            return await context.PlayerDepotItems
                .Where(c => c.PlayerId == id)
                .ToListAsync();
        }

        public async Task DeleteAll(uint playerId)
        {
            await using var context = NewDbContext;
            if (!context.Database.IsRelational())
            {
                var items = context.PlayerDepotItems.Where(x => x.PlayerId == playerId);
                foreach (var item in items) await Delete(item);
                return;
            }

            await context.Database.ExecuteSqlRawAsync($"delete from player_depot_items where player_id = {playerId}");
        }

        #endregion
    }
}