using System;
using System.Collections.Generic;
using System.Linq;
using Eem.Thraxus.Helpers;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Extensions
{
    public static class GridExtenstions
    {
        private static readonly List<IMyCubeGrid> ReusableGridCollection = new List<IMyCubeGrid>();

        /// <summary>
        ///     Returns world speed cap, in m/s.
        /// </summary>
        public static float GetSpeedCap(this IMyShipController shipController)
        {
            switch (shipController.CubeGrid.GridSizeEnum)
            {
                case MyCubeSize.Small:
                    return MyDefinitionManager.Static.EnvironmentDefinition.SmallShipMaxSpeed;
                case MyCubeSize.Large:
                    return MyDefinitionManager.Static.EnvironmentDefinition.LargeShipMaxSpeed;
                default:
                    return 100;
            }
        }

        /// <summary>
        ///     Returns world speed cap ratio to default cap of 100 m/s.
        /// </summary>
        //public static float GetSpeedCapRatioToDefault(this IMyShipController ShipController)
        //{
        //	return ShipController.GetSpeedCap() / 100;
        //}
        public static IMyPlayer FindControllingPlayer(this IMyCubeGrid grid, bool write = true)
        {
            try
            {
                IMyPlayer player = null;
                IMyGridTerminalSystem term = grid.GetTerminalSystem();
                List<IMyShipController> shipControllers = term.GetBlocksOfType<IMyShipController>(x => x.IsUnderControl);
                if (shipControllers.Count == 0)
                {
                    shipControllers = term.GetBlocksOfType<IMyShipController>(x => x.GetBuiltBy() != 0);
                    if (shipControllers.Count > 0)
                    {
                        IMyShipController mainController = shipControllers.FirstOrDefault(x => x.IsMainCockpit()) ?? shipControllers.First();
                        long id = mainController.GetBuiltBy();
                        player = MyAPIGateway.Players.GetPlayerById(id);
                        if (write && player != null) grid.DebugWrite("Grid.FindControllingPlayer", $"Found cockpit built by player {player.DisplayName}.");
                        return player;
                    }

                    if (write) grid.DebugWrite("Grid.FindControllingPlayer", "No builder player was found.");
                    return null;
                }

                player = MyAPIGateway.Players.GetPlayerById(shipControllers.First().ControllerInfo.ControllingIdentityId);
                if (write && player != null) grid.DebugWrite("Grid.FindControllingPlayer", $"Found player in control: {player.DisplayName}");
                return player;
            }
            catch (Exception scrap)
            {
                grid.LogError("Grid.FindControllingPlayer", scrap);
                return null;
            }
        }

        //public static bool HasCockpit(this IMyCubeGrid Grid)
        //{
        //	List<IMySlimBlock> blocks = new List<IMySlimBlock>();
        //	Grid.GetBlocks(blocks, x => x is IMyCockpit);
        //	return blocks.Count > 0;
        //}

        //public static bool HasRemote(this IMyCubeGrid Grid)
        //{
        //	List<IMySlimBlock> blocks = new List<IMySlimBlock>();
        //	Grid.GetBlocks(blocks, x => x is IMyRemoteControl);
        //	return blocks.Count > 0;
        //}

        //public static bool HasShipController(this IMyCubeGrid Grid)
        //{
        //	List<IMySlimBlock> blocks = new List<IMySlimBlock>();
        //	Grid.GetBlocks(blocks, x => x is IMyShipController);
        //	return blocks.Count > 0;
        //}

        public static IMyFaction GetOwnerFaction(this IMyCubeGrid grid, bool recalculateOwners = false)
        {
            try
            {
                if (recalculateOwners)
                    (grid as MyCubeGrid).RecalculateOwners();

                IMyFaction factionFromBigowners = null;
                IMyFaction faction = null;
                if (grid.BigOwners.Count > 0 && grid.BigOwners[0] != 0)
                {
                    long ownerId = grid.BigOwners[0];
                    factionFromBigowners = GeneralExtensions.FindOwnerFactionById(ownerId);
                }
                else
                {
                    grid.LogError("Grid.GetOwnerFaction", new Exception("Cannot get owner faction via BigOwners.", new Exception("BigOwners is empty.")));
                }

                IMyGridTerminalSystem term = grid.GetTerminalSystem();
                var allTermBlocks = new List<IMyTerminalBlock>();
                term.GetBlocks(allTermBlocks);

                if (allTermBlocks.Count == 0)
                {
                    grid.DebugWrite("Grid.GetOwnerFaction", "Terminal system is empty!");
                    return null;
                }

                IGrouping<string, IMyTerminalBlock> biggestOwnerGroup = allTermBlocks.GroupBy(x => x.GetOwnerFactionTag()).OrderByDescending(gp => gp.Count()).FirstOrDefault();
                if (biggestOwnerGroup != null)
                {
                    string factionTag = biggestOwnerGroup.Key;
                    faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(factionTag);
                    if (faction != null)
                        grid.DebugWrite("Grid.GetOwnerFaction", $"Found owner faction {factionTag} via terminal system");
                    return faction ?? factionFromBigowners;
                }

                grid.DebugWrite("Grid.GetOwnerFaction", "CANNOT GET FACTION TAGS FROM TERMINAL SYSTEM!");
                List<IMyShipController> controllers = grid.GetFatBlocks<IMyShipController>().ToList(); // .GetBlocks<IMyShipController>();
                //List<IMyShipController> controllers = grid.GetBlocks<IMyShipController>();
                if (!controllers.Any())
                {
                    faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(allTermBlocks.First().GetOwnerFactionTag());
                    if (faction != null)
                    {
                        grid.DebugWrite("Grid.GetOwnerFaction", $"Found owner faction {faction.Tag} via first terminal block");
                        return faction ?? factionFromBigowners;
                    }

                    grid.DebugWrite("Grid.GetOwnerFaction", "Unable to owner faction via first terminal block!");
                    return faction ?? factionFromBigowners;
                }

                List<IMyShipController> mainControllers;

                if (controllers.Any(x => x.IsMainCockpit(), out mainControllers))
                {
                    faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(mainControllers[0].GetOwnerFactionTag());
                    if (faction != null)
                    {
                        grid.DebugWrite("Grid.GetOwnerFaction", $"Found owner faction {faction.Tag} via main cockpit");
                        return faction ?? factionFromBigowners;
                    }
                } // Controls falls down if faction was not found by main cockpit

                faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(controllers[0].GetOwnerFactionTag());
                if (faction != null)
                {
                    grid.DebugWrite("Grid.GetOwnerFaction", $"Found owner faction {faction.Tag} via cockpit");
                    return faction ?? factionFromBigowners;
                }

                grid.DebugWrite("Grid.GetOwnerFaction", "Unable to owner faction via cockpit!");
                faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(allTermBlocks.First().GetOwnerFactionTag());
                if (faction != null)
                {
                    grid.DebugWrite("Grid.GetOwnerFaction", $"Found owner faction {faction.Tag} via first terminal block");
                    return faction ?? factionFromBigowners;
                }

                grid.DebugWrite("Grid.GetOwnerFaction", "Unable to owner faction via first terminal block!");
                return faction ?? factionFromBigowners;
            }
            catch (Exception scrap)
            {
                grid.LogError("Faction.GetOwnerFaction", scrap);
                return null;
            }
        }

        //public static List<T> GetBlocks<T>(this IMyCubeGrid grid, Func<T, bool> selector = null) where T : class, IMyEntity
        //{
        //    List<IMySlimBlock> slimBlocks = new List<IMySlimBlock>();
        //    List<T> blocks = new List<T>();
        //    grid.GetBlocks(slimBlocks, x => x is T);
        //    foreach (var slimBlock in blocks)
        //    {
        //        T block = slimBlock as T;
        //        // Not the most efficient method, but GetBlocks only allows IMySlimBlock selector
        //        if (selector == null || selector(block))
        //            blocks.Add(block);
        //    }
        //    return blocks;
        //}

        //public static List<IMySlimBlock> GetBlocks(this IMyCubeGrid grid, Func<IMySlimBlock, bool> selector = null, int blockLimit = 0)
        //{
        //    List<IMySlimBlock> blocks = new List<IMySlimBlock>();
        //    int i = 0;
        //    Func<IMySlimBlock, bool> collector = selector;
        //    if (blockLimit > 0)
        //    {
        //        collector = (block) =>
        //        {
        //            if (i >= blockLimit) return false;
        //            i++;
        //            return selector == null || selector(block);
        //        };
        //    }

        //    if (collector == null)
        //        grid.GetBlocks(blocks);
        //    else
        //        grid.GetBlocks(blocks, collector);
        //    return blocks;
        //}

        /// <summary>
        ///     Remember, this is only for server-side.
        /// </summary>
        public static void ChangeOwnershipSmart(this IMyCubeGrid grid, long newOwnerId, MyOwnershipShareModeEnum shareMode)
        {
            if (!MyAPIGateway.Session.IsServer) return;
            try
            {
                List<IMyCubeGrid> subgrids = grid.GetAllSubgrids();
                grid.ChangeGridOwnership(newOwnerId, shareMode);
                foreach (IMyCubeGrid subgrid in subgrids)
                    try
                    {
                        subgrid.ChangeGridOwnership(newOwnerId, shareMode);
                        try
                        {
                            foreach (IMyProgrammableBlock pb in subgrid.GetTerminalSystem().GetBlocksOfType<IMyProgrammableBlock>())
                                try
                                {
                                    //if (!string.IsNullOrEmpty(pb.ProgramData)) continue;
                                    //InGameMessaging.ShowLocalNotification($"PB's recompiling... {subgrid.CustomName}");
                                    pb.Recompile();
                                }
                                catch (Exception)
                                {
                                    //InGameMessaging.ShowLocalNotification($"Recompiling this pb threw and error: {e.TargetSite} {e} ");
                                    //	MyAPIGateway.Utilities.InvokeOnGameThread(() => { pb.Recompile(); });
                                }
                        }
                        catch (Exception)
                        {
                            //InGameMessaging.ShowLocalNotification($"PB's recompile threw: {e} ");
                        }
                    }
                    catch (Exception e)
                    {
                        grid.LogError("ChangeOwnershipSmart.ChangeSubgridOwnership", e);
                    }
            }
            catch (Exception e)
            {
                grid.LogError("ChangeOwnershipSmart", e);
            }
        }

        public static List<IMyCubeGrid> GetAllSubgrids(this IMyCubeGrid grid)
        {
            try
            {
                ReusableGridCollection.Clear();
                MyAPIGateway.GridGroups.GetGroup(grid, GridLinkTypeEnum.Mechanical, ReusableGridCollection);
                //return MyAPIGateway.GridGroups.GetGroup(Grid, GridLinkTypeEnum.Logical);
            }
            catch (Exception e)
            {
                grid.LogError("GetAllSubgrids", e);
                return new List<IMyCubeGrid>();
            }

            return new List<IMyCubeGrid>();
        }
    }
}