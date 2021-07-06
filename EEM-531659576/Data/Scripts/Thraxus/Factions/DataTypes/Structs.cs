using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using ProtoBuf;

namespace Eem.Thraxus.Factions.DataTypes
{
	//[ProtoContract]
	//public struct FactionRelationSaveState
	//{
	//	[ProtoMember(1)]
	//	public readonly long FactionId;

	//	[ProtoMember(2)]
	//	public readonly string FactionTag;

	//	[ProtoMember(3)]
	//	public readonly int Reputation;

	//	public FactionRelationSaveState(long factionId, string factionTag, int reputation)
	//	{
	//		FactionId = factionId;
	//		FactionTag = factionTag;
	//		Reputation = reputation;
	//	}

	//	public override string ToString()
	//	{
	//		return $"Tag: {FactionTag} ID: {FactionId} Rep: {Reputation}";
	//	}
	//}

	public struct PendingWar
	{
		public readonly long IdentityId;
		public readonly long Against;

		public PendingWar(long identityId, long against)
		{
			IdentityId = identityId;
			Against = against;
		}

		public override string ToString()
		{
			return $"{IdentityId} | {Against}";
		}
	}

	[XmlRoot("SaveData", IsNullable = false)]
	[ProtoContract]
	public struct SaveData
	{
		[XmlArray("FactionSaves")] [ProtoMember(1)] public readonly List<RelationSave> FactionSave;
		[XmlArray("IdentitySaves")] [ProtoMember(2)] public readonly List<RelationSave> IdentitySave;

		public SaveData(List<RelationSave> relationSave, List<RelationSave> identitySave)
		{
			FactionSave = relationSave;
			IdentitySave = identitySave;
		}

		public bool IsEmpty => (FactionSave == null && IdentitySave == null);

		public override string ToString()
		{
			return $"[FactionSave.Count] {FactionSave?.Count ?? 0} [IdentitySave.Count] {IdentitySave?.Count ?? 0}";
		}
	}

	[XmlInclude(typeof(RelationSave))]
	[ProtoContract]
	public struct RelationSave
	{
		[XmlAttribute("FromID")]
		[ProtoMember(1)] 
		public long FromId;

		[XmlArray("Relations")] 
		[XmlArrayItem(typeof(Relation), ElementName = "Relation")] 
		[ProtoMember(2)] 
		public List<Relation> ToFactionRelations;
		
		public RelationSave(long fromId, List<Relation> toFactionRelations)
		{
			FromId = fromId;
			ToFactionRelations = toFactionRelations;
		}

		public override string ToString()
		{
			return $"[FromId] {FromId} [Saved Relations] {ToFactionRelations.Count}";
		}

		public string ToStringExtended()
		{
			var sb = new StringBuilder();
			sb.AppendFormat("{0,-4}Faction ID {1,-18} has the following relationships:\n", " ", FromId);
			sb.AppendLine();
			foreach (Relation relation in ToFactionRelations)
			{
				sb.AppendFormat("{0,-8}[Faction] {1,-18} [Rep] {2,-5} \n", " ", relation.FactionId, relation.Rep);
			}
			return sb.ToString();
		}
	}

	[XmlInclude(typeof(Relation))]
	[ProtoContract]
	public struct Relation
	{
		[XmlAttribute("FactionId")] 
		[ProtoMember(1)] 
		public long FactionId;
		
		[XmlAttribute("Rep")] 
		[ProtoMember(2)] 
		public int Rep;

		public Relation(long factionId, int rep)
		{
			FactionId = factionId;
			Rep = rep;
		}

		public override string ToString()
		{
			return $"[FactionId] {FactionId} [Rep] {Rep}";
		}
	}

	//[ProtoContract]
	//public struct FullFactionRelationSave
	//{
	//	[ProtoMember(1)] public readonly List<FactionRelationSave> FactionRelationSaves;

	//	public FullFactionRelationSave(List<FactionRelationSave> factionRelationSaves)
	//	{
	//		FactionRelationSaves = factionRelationSaves;
	//	}

	//	public override string ToString()
	//	{
	//		return $"FullFactionRelationSave Size: {FactionRelationSaves.Count}";
	//	}
	//}

	//[ProtoContract]
	//public struct FactionRelationSave
	//{
	//	[ProtoMember(1)] public readonly long FromFactionId;
	//	[ProtoMember(2)] public readonly long ToFactionId;
	//	[ProtoMember(3)] public readonly int Rep;

	//	public FactionRelationSave(long fromFactionId, long toFactionId, int rep)
	//	{
	//		FromFactionId = fromFactionId;
	//		ToFactionId = toFactionId;
	//		Rep = rep;
	//		ToFactionId = toFactionId;
	//	}

	//	public override string ToString()
	//	{
	//		return $"FromFactionId: {FromFactionId} | ToFactionId: {ToFactionId} | Rep: {Rep}";
	//	}
	//}

	//[ProtoContract]
	//public class IdentityRelationSave
	//{
	//	[ProtoMember(1)] public readonly long FromIdentityId;
	//	[ProtoMember(2)] public readonly HashSet<long> ToFactionIds;

	//	public IdentityRelationSave(long fromIdentity, HashSet<long> toFactions)
	//	{
	//		FromIdentityId = fromIdentity;
	//		ToFactionIds = toFactions;
	//	}

	//	public override string ToString()
	//	{
	//		return $"FromIdentityId: {FromIdentityId} | ToFactionIds Count: {ToFactionIds.Count}";
	//	}
	//}

}
