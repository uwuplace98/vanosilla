using System.Collections.Generic;
using WingsEmu.Game.Ship;
using WingsEmu.Game.Ship.Configuration;

namespace WingsEmu.Plugins.BasicImplementations.Ship
{
    public class ShipConfigurationProvider : IShipConfigurationProvider
    {
        private readonly ShipConfiguration _shipConfiguration;

        public ShipConfigurationProvider(ShipConfiguration shipConfiguration)
        {
            _shipConfiguration = shipConfiguration;
        }

        public IReadOnlyList<WingsEmu.Game.Ship.Configuration.Ship> GetShips() => _shipConfiguration;
    }
}