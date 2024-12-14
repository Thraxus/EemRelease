using System.Text;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Common.Reporting
{
    public class GameSettings
    {
        public StringBuilder Report(StringBuilder sb)
        {
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendFormat("{0, -2}Game Settings", " ");
            sb.AppendLine("__________________________________________________");
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Adaptive Sim Enabled:{1}", " ", MyAPIGateway.Session.SessionSettings.AdaptiveSimulationQuality);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Cargo Ships Enabled:{1}", " ", MyAPIGateway.Session.SessionSettings.CargoShipsEnabled);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Stop Grids Period (minutes):{1}", " ", MyAPIGateway.Session.SessionSettings.StopGridsPeriodMin);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Encounters Enabled:{1}", " ", MyAPIGateway.Session.SessionSettings.EnableEncounters);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Encounter Density:{1}", " ", MyAPIGateway.Session.SessionSettings.EncounterDensity);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Economy Enabled:{1}", " ", MyAPIGateway.Session.SessionSettings.EnableEconomy);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Economy Ticks (seconds):{1}", " ", MyAPIGateway.Session.SessionSettings.EconomyTickInSeconds);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Bounty Contracts Enabled:{1}", " ", MyAPIGateway.Session.SessionSettings.EnableBountyContracts);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Drones Enabled:{1}", " ", MyAPIGateway.Session.SessionSettings.EnableDrones);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Scripts Enabled:{1}", " ", MyAPIGateway.Session.SessionSettings.EnableIngameScripts);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Asteroid Density:{1}", " ", MyAPIGateway.Session.SessionSettings.ProceduralDensity);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Weather Enabled:{1}", " ", MyAPIGateway.Session.SessionSettings.WeatherSystem);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Online Mode:{1}", " ", MyAPIGateway.Session.SessionSettings.OnlineMode);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Game Mode:{1}", " ", MyAPIGateway.Session.SessionSettings.GameMode);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Spiders Enabled:{1}", " ", MyAPIGateway.Session.SessionSettings.EnableSpiders);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Wolves Enabled:{1}", " ", MyAPIGateway.Session.SessionSettings.EnableWolfs);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Sync Distance:{1}", " ", MyAPIGateway.Session.SessionSettings.SyncDistance);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}View Distance:{1}", " ", MyAPIGateway.Session.SessionSettings.ViewDistance);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Player Inventory Size Multiplier:{1}", " ", MyAPIGateway.Session.SessionSettings.InventorySizeMultiplier);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Grid Inventory Size Multiplier:{1}", " ", MyAPIGateway.Session.SessionSettings.BlocksInventorySizeMultiplier);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Block Limits Enabled:{1}", " ", MyAPIGateway.Session.SessionSettings.BlockLimitsEnabled);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Global Encounter PCU:{1}", " ", MyAPIGateway.Session.SessionSettings.GlobalEncounterPCU);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Total Pirate PCU:{1}", " ", MyAPIGateway.Session.SessionSettings.PiratePCU);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Total Player PCU:{1}", " ", MyAPIGateway.Session.SessionSettings.TotalPCU);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Enable Planetary Encounters:{1}", " ", MyAPIGateway.Session.SessionSettings.EnablePlanetaryEncounters);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Planetary Encounter Desired Spawn Range:{1}", " ", MyAPIGateway.Session.SessionSettings.PlanetaryEncounterDesiredSpawnRange);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Planetary Encounter Area Lockdown Range:{1}", " ", MyAPIGateway.Session.SessionSettings.PlanetaryEncounterAreaLockdownRange);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Planetary Encounter Despawn Timeout:{1}", " ", MyAPIGateway.Session.SessionSettings.PlanetaryEncounterDespawnTimeout);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Planetary Encounter Existing Structure Range:{1}", " ", MyAPIGateway.Session.SessionSettings.PlanetaryEncounterExistingStructuresRange);
            sb.AppendLine();
            sb.AppendFormat("{0, -4}Planetary Encounter Presence Range:{1}", " ", MyAPIGateway.Session.SessionSettings.PlanetaryEncounterPresenceRange);
            sb.AppendLine();

            return sb;
        }
    }
}