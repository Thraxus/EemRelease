using Eem.Thraxus.Enums;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Extensions
{
    internal static class EnumExtensions
    {
        public static string FactionTypeToString(this FactionType type)
        {
            switch (type)
            {
                case FactionType.AMPH:
                    return "AMPH";
                case FactionType.CIVL:
                    return "CIVL";
                case FactionType.EXMC:
                    return "EXMC";
                case FactionType.HS:
                    return "HS";
                case FactionType.IMDC:
                    return "IMDC";
                case FactionType.IMDF:
                    return "IMDF";
                case FactionType.ISTG:
                    return "ISTG";
                case FactionType.KUSS:
                    return "KUSS";
                case FactionType.MAHI:
                    return "MA-I";
                case FactionType.MMEC:
                    return "MMEC";
                case FactionType.SEPD:
                    return "SEPD";
                case FactionType.SPRT:
                    return "SPRT";
                case FactionType.Other:
                    return "SPRT";
                default:
                    return "SPRT";
            }
        }

        public static IMyFaction FactionTypeToFaction(this FactionType type)
        {
            return MyAPIGateway.Session.Factions.TryGetFactionByTag(type.FactionTypeToString());
        }
    }
}