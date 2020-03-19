using NeoServer.Server.Helpers;
using NeoServer.Server.Map;
using NeoServer.Server.Model.World.Structs;
using NeoServer.Server.World.Map;
using System;

namespace NeoServer.Server.World
{
	public sealed class WorldLoader : IWorldLoader
	{
		private World _world;

		public byte PercentageComplete {
			get {
				if (_world == null)
					return 0;
				else
					return _world.PercentageComplete;
			}
		}

		public bool HasLoaded(int x, int y, byte z) {
			if (_world == null)
				return false;
			else
				return _world.HasLoaded(x, y, z);
		}
		
		

		private readonly OTBMWorldLoader _OTBMWorldLoader;
		public WorldLoader(OTBMWorldLoader OTBMWorldLoader)
		{
			_OTBMWorldLoader = OTBMWorldLoader;
		}

		public void Load()
		{
			var serializedWorldData = ServerResourcesManager.GetMap();
			_world = _OTBMWorldLoader.LoadWorld(serializedWorldData);
			Console.WriteLine($"Tiles loaded in world: {_world.LoadedTilesCount()}");
		}
	}
}
