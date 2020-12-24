﻿using Autofac;
using NeoServer.Data;
using NeoServer.Data.RavenDB;
using NeoServer.Game.World.Spawns;
using NeoServer.Loaders.Items;
using NeoServer.Loaders.Monsters;
using NeoServer.Loaders.Spawns;
using NeoServer.Loaders.World;
using NeoServer.Networking.Listeners;
using NeoServer.Server.Events;
using NeoServer.Server.Jobs.Creatures;
using NeoServer.Server.Jobs.Items;
using NeoServer.Server.Security;
using NeoServer.Server.Standalone.IoC;
using NeoServer.Server.Tasks;
using NeoServer.Server.Contracts.Tasks;
using Serilog.Core;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NeoServer.Server;
using NeoServer.Data.Model;
using NeoServer.Server.Model.Players;

Console.Title = "OpenCoreMMO Server";

var sw = new Stopwatch();
sw.Start();

var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken = cancellationTokenSource.Token;

var container = Container.CompositionRoot();
container.Resolve<Database>().Connect();

var context = container.Resolve<NeoContext>();

var account = new AccountModel
{
    Id = 0,
    AccountName = "Felipe",
    Email = "felipe@teste@gmail.com",
    Password = "teste",
    PremiumTime = 1
};

var player = new PlayerModel
{
    Id = 0,
    CharacterName = "Felipe Muniz",
    Level = 1
};

context.Accounts.Add(account);

context.SaveChanges();

var accounts = context.Accounts.AsQueryable().ToList();

context.Player.Add(player);

context.SaveChanges();

var players = context.Player.AsQueryable().ToList();

var logger = container.Resolve<Logger>();

RSA.LoadPem();

//   ScriptCompiler.Compile();

container.Resolve<ItemTypeLoader>().Load();

container.Resolve<WorldLoader>().Load();

container.Resolve<SpawnLoader>().Load();

container.Resolve<MonsterLoader>().Load();
//new SpellLoader().Load();
container.Resolve<SpawnManager>().StartSpawn();

var listeningTask = StartListening(container, cancellationToken);

var scheduler = container.Resolve<IScheduler>();
var dispatcher = container.Resolve<IDispatcher>();

dispatcher.Start(cancellationToken);
scheduler.Start(cancellationToken);

scheduler.AddEvent(new SchedulerEvent(1000, container.Resolve<GameCreatureJob>().StartCheckingCreatures));
scheduler.AddEvent(new SchedulerEvent(1000, container.Resolve<GameItemJob>().StartCheckingItems));


container.Resolve<EventSubscriber>().AttachEvents();

container.Resolve<Game>().Open();

sw.Stop();
logger.Information($"Server is up! {sw.ElapsedMilliseconds} ms");
logger.Information($"Memory usage: {Math.Round((System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 / 1024f) / 1024f, 2)} MB");

listeningTask.Wait(cancellationToken);


static async Task StartListening(IContainer container, CancellationToken token)
{
    container.Resolve<LoginListener>().BeginListening();
    container.Resolve<GameListener>().BeginListening();

    while (!token.IsCancellationRequested)
    {
        await Task.Delay(TimeSpan.FromSeconds(1), token);
    }
}



