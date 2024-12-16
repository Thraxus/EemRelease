using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Common.Reporting
{
    public class ExistingFactions
    {
        public StringBuilder Report(StringBuilder sb)
        {
            var identityList = new List<IMyIdentity>();
            MyAPIGateway.Players.GetAllIdentites(identityList);

            var parsedIdentities = new Dictionary<long, IMyIdentity>();
            foreach (IMyIdentity identity in identityList) parsedIdentities.Add(identity.IdentityId, identity);

            sb.AppendLine();
            sb.AppendFormat("{0, -2}Existing Factions", " ");
            sb.AppendLine("__________________________________________________");
            sb.AppendLine();

            sb.AppendFormat("{0, -4}[FactionId][Tag][IsEveryoneNpc] Display Name\n", " ");
            sb.AppendFormat("{0, -6}[MemberId][PlayerId] Display Name\n", " ");
            sb.AppendLine();

            foreach (KeyValuePair<long, IMyFaction> faction in MyAPIGateway.Session.Factions.Factions)
            {
                IMyFaction x = faction.Value;
                sb.AppendFormat("{0, -4}[{1}][{2}][{3}] {4}\n", " ", faction.Key, x.Tag, x.IsEveryoneNpc(), x.Name);
                foreach (KeyValuePair<long, MyFactionMember> member in x.Members)
                {
                    IMyIdentity ident;
                    bool nailedIt = parsedIdentities.TryGetValue(member.Key, out ident);
                    sb.AppendFormat("{0, -6}[{1}][{2}] {3}\n", " ", member.Key, member.Value.PlayerId, nailedIt ? ident.DisplayName : "Unknown");
                }

                sb.AppendLine();
            }

            return sb;
        }
    }
}