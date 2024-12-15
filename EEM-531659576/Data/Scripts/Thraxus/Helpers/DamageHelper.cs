using System;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Extensions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Weapons;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace Eem.Thraxus.Helpers
{
    public static class DamageHelper
    {
        private static readonly List<IMySlimBlock> Slims = new List<IMySlimBlock>();

        /// <summary>
        ///     Determines if damage was done by player.
        ///     <para />
        ///     If it's necessary to determine who did the damage, use overload.
        /// </summary>
        //public static bool IsDoneByPlayer(this MyDamageInformation damage)
        //{
        //	IMyPlayer trash;
        //	return damage.IsDoneByPlayer(out trash);
        //}
        private static bool IsDamagedByPlayerWarhead(IMyWarhead warhead, out IMyPlayer damager)
        {
            damager = null;
            try
            {
                if (warhead.OwnerId == 0)
                {
                    damager = MyAPIGateway.Players.GetPlayerById(((MyCubeBlock)warhead).BuiltBy);
                    //AiSessionCore.DebugWrite("Damage.IsDoneByPlayer", "Attempting to find damager by neutral warhead.");
                    return damager != null;
                }

                damager = MyAPIGateway.Players.GetPlayerById(warhead.OwnerId);
                //AiSessionCore.DebugWrite("Damage.IsDoneByPlayer", "Attempting to find damager by warhead owner.");
                return damager != null;
            }
            catch (Exception)
            {
                //AiSessionCore.LogError("Damage.IsDoneByPlayer", new Exception("Check for neutral warheads crashed", scrap));
                return false;
            }
        }

        private static bool IsDamagedByPlayer(IMyGunBaseUser gun, out IMyPlayer damager)
        {
            damager = null;
            try
            {
                damager = MyAPIGateway.Players.GetPlayerById(gun.OwnerId);
                //AISessionCore.DebugWrite($"GunDamage.IsDamagedByPlayer", $"Getting player from gun. ID: {Gun.OwnerId}, player: {(Damager != null ? Damager.DisplayName : "null")}", false);
                return !damager?.IsBot ?? false;
            }
            catch (Exception)
            {
                //AiSessionCore.LogError("Damage.IsDoneByPlayer", new Exception("Check gun owner crashed", scrap));
                return false;
            }
        }

        private static bool IsDamagedByPlayer(IMyEngineerToolBase tool, out IMyPlayer damager)
        {
            damager = null;
            try
            {
                damager = MyAPIGateway.Players.GetPlayerById(tool.OwnerIdentityId);
                //AISessionCore.DebugWrite($"ToolDamage.IsDamagedByPlayer", $"Getting player from tool. ID: {Tool.OwnerId}, IdentityID: {Tool.OwnerIdentityId}, player: {(Damager != null ? Damager.DisplayName : "null")}", false);
                return damager != null && !damager.IsBot;
            }
            catch (Exception)
            {
                //AiSessionCore.LogError("Damage.IsDoneByPlayer", new Exception("Check gun owner crashed", scrap));
                return false;
            }
        }

        private static bool IsDamagedByPlayerInNeutralGrid(IMyCubeGrid grid, out IMyPlayer damager)
        {
            damager = null;
            try
            {
                damager = grid.FindControllingPlayer();
                if (damager != null) return !damager.IsBot;

                try
                {
                    //List<MyCubeBlock> cubeBlocks = grid.GetBlocks<MyCubeBlock>(x => x.BuiltBy != 0);
                    //List<IMySlimBlock> slims = new List<IMySlimBlock>();
                    //grid.GetBlocks(slims, x => x.BuiltBy != 0);

                    List<IMyTerminalBlock> cubeBlocks = grid.GetFatBlocks<IMyTerminalBlock>().Where(x => !x.IsBuiltByNobody()).ToList();
                    if (cubeBlocks.Count != 0)
                    {
                        long thatCunningAsshat = cubeBlocks[0].SlimBlock.BuiltBy;
                        damager = MyAPIGateway.Players.GetPlayerById(thatCunningAsshat);
                        return damager != null;
                    }

                    Slims.Clear();
                    grid.GetBlocks(Slims, x => x.BuiltBy != 0);
                    if (Slims.Count == 0) return false; // We give up on this one

                    try
                    {
                        damager = MyAPIGateway.Players.GetPlayerById(Slims.First().GetBuiltBy());
                        //if (damager != null) grid.DebugWrite("Damage.IsDoneByPlayer.FindBuilderBySlimBlocks", $"Found damaging player from slim block. Meanie is {damager.DisplayName}");
                        return damager != null;
                    }
                    catch (Exception)
                    {
                        //AiSessionCore.LogError("Damage.IsDoneByPlayer", new Exception("Check grid via SlimBlocks BuiltBy crashed.", scrap));
                        return false;
                    }
                }
                catch (Exception)
                {
                    //AiSessionCore.LogError("Damage.IsDoneByPlayer", new Exception("Check grid via BuiltBy crashed.", scrap));
                    return false;
                }
            }
            catch (Exception)
            {
                //AiSessionCore.LogError("Damage.IsDoneByPlayer", new Exception("Check neutral grid crashed", scrap));
                return false;
            }
        }

        private static bool IsDamagedByPlayerGrid(IMyCubeGrid grid, out IMyPlayer damager)
        {
            damager = null;
            try
            {
                long biggestOwner = grid.BigOwners.FirstOrDefault();
                if (biggestOwner == 0) return false;
                damager = MyAPIGateway.Players.GetPlayerById(biggestOwner);
                return !damager?.IsBot ?? false;
            }
            catch (Exception)
            {
                //AiSessionCore.LogError("Damage.IsDoneByPlayer", new Exception("Check grid via BigOwners crashed", scrap));
                return false;
            }
        }


        /// <summary>
        ///     Determines if damage was done by player.
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="damager">Provides player who did the damage. Null if damager object is not a player.</param>
        public static bool IsDoneByPlayer(this MyDamageInformation damage, out IMyPlayer damager)
        {
            damager = null;

            try
            {
                IMyEntity attackerEntity = MyAPIGateway.Entities.GetEntityById(damage.AttackerId);
                if (damage.IsDeformation || damage.AttackerId == 0 || attackerEntity == null) return false;

                if (attackerEntity is IMyMeteor) return false;
                if (attackerEntity is IMyWarhead) return IsDamagedByPlayerWarhead(attackerEntity as IMyWarhead, out damager);
                if (attackerEntity is IMyEngineerToolBase) return IsDamagedByPlayer(attackerEntity as IMyEngineerToolBase, out damager);
                if (attackerEntity is IMyGunBaseUser) return IsDamagedByPlayer(attackerEntity as IMyGunBaseUser, out damager);

                attackerEntity = attackerEntity.GetTopMostParent();
                var grid = attackerEntity.GetTopMostParent() as IMyCubeGrid;
                if (grid == null || grid.IsPirate())
                {
                    return false;
                }

                //if (!(attackerEntity is IMyCubeGrid)) return false;
                //var grid = attackerEntity as IMyCubeGrid;
                //if (grid.IsPirate()) return false;
                //grid.GetOwnerFaction()

                return grid.IsOwnedByNobody() ? IsDamagedByPlayerInNeutralGrid(grid, out damager) : IsDamagedByPlayerGrid(grid, out damager);
            }
            catch (Exception)
            {
                //AiSessionCore.LogError("Damage.IsDoneByPlayer", new Exception("General crash.", scrap));
                return false;
            }
        }

        public static bool IsMeteor(this MyDamageInformation damage)
        {
            IMyEntity attackerEntity = MyAPIGateway.Entities.GetEntityById(damage.AttackerId);
            return attackerEntity is IMyMeteor;
        }

        public static bool IsThruster(this MyDamageInformation damage)
        {
            IMyEntity attackerEntity = MyAPIGateway.Entities.GetEntityById(damage.AttackerId);
            return attackerEntity is IMyThrust;
        }

        //public static bool IsGrid(this MyDamageInformation damage, out IMyCubeGrid grid)
        //{
        //	grid = MyAPIGateway.Entities.GetEntityById(damage.AttackerId).GetTopMostParent() as IMyCubeGrid;
        //	return grid != null;
        //}

        //public static bool IsGrid(this MyDamageInformation damage)
        //{
        //	IMyCubeGrid grid = MyAPIGateway.Entities.GetEntityById(damage.AttackerId).GetTopMostParent() as IMyCubeGrid;
        //	return grid != null;
        //}
    }
}