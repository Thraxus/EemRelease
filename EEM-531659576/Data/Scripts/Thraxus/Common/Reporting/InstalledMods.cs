using System.Text;
using Eem.Thraxus.Common.Extensions;
using Sandbox.ModAPI;
using VRage.Game;

namespace Eem.Thraxus.Common.Reporting
{
    public class InstalledMods
    {
        public StringBuilder Report(StringBuilder sb)
        {
            sb.AppendLine();
            sb.AppendFormat("{0, -2}Installed Mods", " ");
            sb.AppendLine("__________________________________________________");
            sb.AppendLine();

            sb.AppendFormat("{0, -4}[ModId][IsDependency] Mod Name\n", " ");
            foreach (MyObjectBuilder_Checkpoint.ModItem mod in MyAPIGateway.Session.Mods)
                sb.AppendFormat("{0, -4}[{1:D18}][{2}] {3}\n", " ", mod.PublishedFileId, mod.IsDependency.ToSingleChar(), mod.FriendlyName);

            return sb;
        }
    }
}