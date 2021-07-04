﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Eem.Thraxus.Common.Enums;
using Eem.Thraxus.Common.Utilities.Tools.Logging;
using Eem.Thraxus.Common.Utilities.Tools.Networking;
using Eem.Thraxus.Factions.DataTypes;
using Eem.Thraxus.Factions.Models;
using Eem.Thraxus.Factions.Utilities;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;

namespace Eem.Thraxus.Factions.BaseClasses
{
	public abstract class RepControl
	{
		public long FromRelationId;
		protected RelationType RelationType;

		public bool IsPirate { get; protected set; }

		public readonly ConcurrentDictionary<long, int> ToFactions = new ConcurrentDictionary<long, int>();
		
		// TODO: Must change the ToFactions list to include rep; probably need a custom class

		protected readonly List<IMyPlayer> Players = new List<IMyPlayer>();

		protected List<IMyPlayer> GetPlayers()
		{
			Players.Clear();
			MyAPIGateway.Players.GetPlayers(Players);
			return Players;
		}
		
		/// <summary>
		/// Adds a new relationship.  If no value is passed to rep, default neutral rep will be used.
		/// </summary>
		/// <param name="id">ID of the new relationship</param>
		/// <param name="rep">Reputation with the new relationship; nullable.</param>
		public void AddNewRelation(long id, int? rep = null)
		{
			//StaticLog.WriteToLog("AddNewRelation", $"Type: {RelationType} - Attempting to add new relation between Id: {id} Rep Requested: {rep.ToString()}", LogType.General);
			if (!MyAPIGateway.Session.Factions.Factions.ContainsKey(id)) return;
			if (rep == null)
				rep = FactionSettings.DefaultNeutralRep;
			ToFactions.TryAdd(id, (int) rep);
			SetReputation(id, (int) rep);
			//StaticLog.WriteToLog("AddNewRelation", $"Type: {RelationType} - Id: {id} |[Rep]| Requested: {rep.ToString()} <> Actual: {GetReputation(id)}", LogType.General);
		}

		public void RemoveRelation(long id)
		{
			if (RelationExists(id))
				ToFactions.Remove(id);
		}

		public bool RelationExists(long id)
		{
			return ToFactions.ContainsKey(id);
		}

		public int RelationCount()
		{
			return ToFactions.Count;
		}

		public int GetReputation(long id)
		{
			return ToFactions.ContainsKey(id) ? ToFactions[id] : -5000;
		}

		public abstract int GetSeReputation(long id);

		protected void SetReputation(long id, int rep)
		{
			//StaticLog.WriteToLog("SetReputation", $"Type: {RelationType} - Setting Rep between {FromRelationId} and {id} to {rep}", LogType.General);
			if (!ToFactions.ContainsKey(id)) return;
			ToFactions[id] = rep;
			SetSeReputation(id, rep);
		}

		protected abstract void SetSeReputation(long id, int rep);

		public void SetAsPirate()
		{
			IsPirate = true;
			foreach (KeyValuePair<long, int> toFaction in ToFactions)
				SetReputation(toFaction.Key, FactionSettings.DefaultNegativeRep);
		}

		public void ResetReputation()
		{
			foreach (KeyValuePair<long, int> toFaction in ToFactions)
				SetSeReputation(toFaction.Key, toFaction.Value);
		}

		public void NoLongerPirate()
		{
			IsPirate = false;
		}

		public void DecayReputation()
		{
			foreach (KeyValuePair<long, int> toFaction in ToFactions)
			{
				int rep = GetReputation(toFaction.Key);
				if (rep > FactionSettings.DefaultNeutralRep)
					SetReputation(toFaction.Key, rep - FactionSettings.RepDecay / 2);
				// TODO: Decide if I want to lock rep if it ever hits -1500 and make the player buy their way back to good favor.  If yes, uncomment the below.
				//if (rep < FactionSettings.DefaultNeutralRep && rep > FactionSettings.DefaultNegativeRep)
				if (rep < FactionSettings.DefaultNeutralRep)
					SetReputation(toFaction.Key, rep + FactionSettings.RepDecay);
				if (rep != FactionSettings.DefaultNeutralRep)
					IsDialogRequired(rep, toFaction.Key);
			}
		}

		public void TriggerWar(long against)
		{
			StaticLog.WriteToLog("TriggerWar", $"Type: {RelationType} - War triggered between {FromRelationId} and {against}...", LogType.General);
			int rep = GetReputation(against);

			if (rep - FactionSettings.AdditionalWarRepPenalty < FactionSettings.DefaultWarRep)
				SetReputation(against, rep - FactionSettings.AdditionalWarRepPenalty);
			else
				SetReputation(against, FactionSettings.DefaultWarRep);
			IsDialogRequired(rep, against); // Dialog may be required.  However, dialog is not given if TriggerWar pushes rep further into the war zone
		}

		private void IsDialogRequired(int oldRep, long against)
		{
			int newRep = GetReputation(against);
			//StaticLog.WriteToLog("IsDialogRequired", $"Old: {oldRep} | New: {newRep}", LogType.General);
			if (oldRep >= FactionSettings.DefaultNeutralRep && newRep < FactionSettings.DefaultNeutralRep)
			{   // Was neutral, is hostile
				DialogRequest(DialogType.WarDeclared, MyAPIGateway.Session.Factions.Factions[against].Tag);
				return;
			}
			  
			if (oldRep < FactionSettings.DefaultNeutralRep && newRep >= FactionSettings.DefaultNeutralRep)
			{   // Was hostile, is neutral
				DialogRequest(DialogType.PeaceAccepted, MyAPIGateway.Session.Factions.Factions[against].Tag);
			}
		}

		private void DialogRequest(DialogType dialog, string sender)
		{
			Func<string> message = Dialogue.RequestDialog(sender, dialog);
			//StaticLog.WriteToLog("DialogRequest", $"Type: {dialog} | Sender: {sender} | Message: {message != null}", LogType.General);
			if (message != null)
				SendMessage(message.Invoke(), sender);
		}

		protected virtual void SendMessage(string message, string sender)
		{
			//StaticLog.WriteToLog("SendMessage", $"From: {sender} | To: {FromRelationId} | Message: {message}", LogType.General);
			Messaging.SendMessageToPlayer($"{message}", sender, FromRelationId, MyFontEnum.DarkBlue);
		}

		public RelationSave GetSaveState()
		{
			HashSet<Relation> relations = new HashSet<Relation>();
			foreach (KeyValuePair<long, int> toFaction in ToFactions)
			{
				relations.Add(new Relation(toFaction.Key, toFaction.Value));
			}
			return new RelationSave(FromRelationId, relations);
		}

		public override string ToString()
		{
			return $"FromId: {FromRelationId} | RelationCounts: {ToFactions.Count}";
		}

		public string ToStringExtended()
		{
			StringBuilder returnString = new StringBuilder();
			returnString.Append("\n");
			foreach (KeyValuePair<long, int> toFaction in ToFactions)
			{
				returnString.Append($"FromId: {FromRelationId} | ToFactionTag: {toFaction} | Reputation: {GetReputation(toFaction.Key)}\n");
			}
			return returnString.ToString();
		}
	}
}