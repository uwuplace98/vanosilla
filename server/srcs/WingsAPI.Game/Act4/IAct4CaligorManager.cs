using System;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Maps;

namespace WingsEmu.Game.Act4
{
    public interface IAct4CaligorManager
    {
        public DateTime CaligorStart { get; set; }
        public DateTime CaligorEnd { get; set; }
        public bool CaligorActive { get; set; }
        public bool CaligorLocked { get; set; }
        public int AngelDamage { get; set; }
        public int DemonDamage { get; set; }

        public bool FernonMapsActive { get; }
        public IMapInstance FernonMap { get; }

        public void TeleportPlayerToCaligorCamp(IPlayerEntity player);
        public void LockCaligorInstance();
        public void EndCaligorInstance(bool CaligorGotKilled);
        public void RefreshCaligorInstance();
        public void InitializeAndStartCaligorInstance();
    }
}