using System;
using Eem.Thraxus.Enums;

namespace Eem.Thraxus.Models
{
    public class BotConfig
    {
        public readonly BotType BotType;
        public readonly FactionType Faction;
        public readonly GridPresetType Preset;

        public readonly bool AmbushMode;
        public readonly bool DelayedAi;
        public readonly bool FleeOnlyWhenDamaged;

        public readonly float ActivationDistance;
        public readonly float FleeSpeedCap;
        public readonly float FleeTriggerDistance;
        public readonly float SeekDistance;

        public readonly string PresetString;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Prefab Config:\t{Faction}\t{BotType}\t{Preset}\t" +
                   $"{DelayedAi}\t{SeekDistance}\t" +
                   $"{AmbushMode}\t{ActivationDistance}\t{FleeOnlyWhenDamaged}\t" +
                   $"{FleeTriggerDistance}\t{FleeSpeedCap}";
        }

        public string ToStringVerbose()
        {
            return $"Prefab Config - |Faction: {Faction} |Type: {BotType} |Preset: {Preset} " +
                   $"|DelayedAi: {DelayedAi} " +
                   $"|SeekDistance: {SeekDistance} |AmbushMode: {AmbushMode} |ActivationDistance: {ActivationDistance} " +
                   $"|FleeOnlyWhenDamaged: {FleeOnlyWhenDamaged} |FleeTriggerDistance: {FleeTriggerDistance} " +
                   $"|FleeSpeedCap: {FleeSpeedCap}";
        }

        private BotType GetBotType(string type)
        {
            var botType = BotType.Invalid;
            switch (type)
            {
                case "Fighter":
                    botType = BotType.Fighter;
                    break;
                case "Freighter":
                    botType = BotType.Freighter;
                    break;
                case "Carrier":
                    botType = BotType.Carrier;
                    break;
                case "Station":
                    botType = BotType.Station;
                    break;
            }

            return botType;
        }

        private T GetValue<T>(string str) where T : struct, IConvertible
        {
            try
            {
                if (typeof(T) == typeof(int))
                {
                    return (T)(object)int.Parse(str);
                }
                if (typeof(T) == typeof(bool))
                {
                    return (T)(object)bool.Parse(str);
                }
                if (typeof(T) == typeof(float))
                {
                    return (T)(object)float.Parse(str);
                }

                // For other numeric types
                return (T)Convert.ChangeType(str, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }

        private FactionType GetFationType(string str)
        {
            var type = FactionType.Other;
            switch (str.ToLower())
            {
                case "amph":
                    type = FactionType.AMPH;
                    break;
                case "civl":
                    type = FactionType.CIVL;
                    break;
                case "exmc":
                    type = FactionType.EXMC;
                    break;
                case "hs":
                    type = FactionType.HS;
                    break;
                case "imdc":
                    type = FactionType.IMDC;
                    break;
                case "imdf":
                    type = FactionType.IMDF;
                    break;
                case "istg":
                    type = FactionType.ISTG;
                    break;
                case "kuss":
                    type = FactionType.KUSS;
                    break;
                case "ma-i":
                    type = FactionType.MAHI;
                    break;
                case "mmec":
                    type = FactionType.MMEC;
                    break;
                case "sepd":
                    type = FactionType.SEPD;
                    break;
                case "sprt":
                    type = FactionType.SPRT;
                    break;
            }

            return type;
        }

        private GridPresetType GetGridPresetType(string str)
        {
            var type = GridPresetType.Unknown;

            switch (str.ToLower())
            {
                case "eemdefaultatmo":
                    type = GridPresetType.AtmosphereDefault;
                    break;
                case "eematmolarge":
                    type = GridPresetType.AtmosphereLarge;
                    break;
                case "eematmogrinder":
                    type = GridPresetType.AtmosphereGrinder;
                    break;
                case "eematmolongrange":
                    type = GridPresetType.AtmosphereLongRange;
                    break;
                case "eematmopolice":
                    type = GridPresetType.AtmospherePolice;
                    break;
                case "eematmosmall":
                    type = GridPresetType.AtmosphereSmall;
                    break;
                case "eemgrinder":
                case "eemspacegrinder":
                    type = GridPresetType.SpaceGrinder;
                    break;
                case "eemdefaultlarge":
                case "eemspacelarge":
                    type = GridPresetType.SpaceLarge;
                    break;
                case "eemlongrange":
                case "eemspacelongrange":
                    type = GridPresetType.SpaceLongRange;
                    break;
                case "eempolice":
                case "eemspacepolice":
                    type = GridPresetType.SpacePolice;
                    break;
                case "eemdefaultsmall":
                case "eemspacedefaultsmall":
                    type = GridPresetType.SpaceSmall;
                    break;
            }

            return type;
        }

        public BotConfig(string config)
        {
            foreach (string cfg in config.Trim().Replace("\r\n", "\n").Split('\n'))
            {
                if (cfg == null) continue;
                string[] x = cfg.Trim().Replace("\r\n", "\n").Split(':');
                if (x.Length < 2) continue;
                switch (x[0].ToLower())
                {
                    case "type":
                        BotType = GetBotType(x[1]);
                        break;
                    case "preset":
                        Preset = GetGridPresetType(x[1]);
                        PresetString = x[1];
                        break;
                    case "seekdistance":
                        SeekDistance = GetValue<float>(x[1]);
                        break;
                    case "faction":
                        Faction = GetFationType(x[1]);
                        break;
                    case "fleeonlywhendamaged":
                        FleeOnlyWhenDamaged = GetValue<bool>(x[1]);
                        break;
                    case "fleetriggerdistance":
                        FleeTriggerDistance = GetValue<float>(x[1]);
                        break;
                    case "fleespeedcap":
                        FleeSpeedCap = GetValue<float>(x[1]);
                        break;
                    case "ambushmode":
                        AmbushMode = GetValue<bool>(x[1]);
                        break;
                    case "delayedai":
                        DelayedAi = GetValue<bool>(x[1]);
                        break;
                    case "activationdistance":
                        ActivationDistance = GetValue<float>(x[1]);
                        break;
                }
            }

            if (Faction == FactionType.Other)
            {
                Faction = FactionType.SPRT;
            }

            if (FleeSpeedCap < 100)
            {
                FleeSpeedCap = 300f;
            }

            if (FleeTriggerDistance < 500)
            {
                FleeTriggerDistance = 1000f;
            }
        }
    }
}