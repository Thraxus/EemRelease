using System.Collections.Generic;
using Eem.Thraxus.Common.Utilities;

namespace Eem.Thraxus.Factions.Utilities
{
	internal static class FactionSettings
	{
		/// <summary>
		/// Value all hostile relationships start out at
		/// </summary>
		public const int DefaultNegativeRep = -1500;

		/// <summary>
		/// Value all neutral relationships start out at
		/// </summary>
		public const int DefaultNeutralRep = -500;

		/// <summary>
		/// Value all neutral relationships start out at
		/// </summary>
		public const int DefaultWarRep = -550;

		/// <summary>
		/// Value all neutral relationships start out at
		/// </summary>
		public const int AdditionalWarRepPenalty = 20;

		/// <summary>
		/// The amount of rep to change every minute from hostile -> neutral
		/// From above neutral back to neutral should be some fraction of this; perhaps 1/2
		/// If this doesn't work out, then will need to change rep war conditions
		/// Value must be an even number!
		/// </summary>
		public const int RepDecay = 2;

		/// <summary>
		/// Faction War cooldown period
		///		15 minute default cooldown, 2 minute in Debug Mode
		/// </summary>
		public static int FactionNegativeRelationshipCooldown => CommonSettings.DebugMode ? (CommonSettings.TicksPerMinute * 2) : (CommonSettings.TicksPerMinute * 15);
		public const int FactionNegativeRelationshipAssessment = CommonSettings.TicksPerSecond;
		public const int FactionMendingRelationshipAssessment = CommonSettings.TicksPerMinute + 20;  // Don't really want these assessments firing at the same time

		/// <summary>
		/// These factions are considered lawful. When they go hostile towards someone,
		/// they also make the police (SEPD) and army (UCMF) go hostile.
		/// </summary>
		public static List<string> LawfulFactionsTags { get; } = new List<string>
		{
			"UCMF", "SEPD", "CIVL", "ISTG", "MA-I", "EXMC", "KUSS", "HS", "AMPH", "IMDC" };

		/// <summary>
		/// 
		/// </summary>
		public static List<string> AllNpcFactions { get; } = new List<string>
		{
			"SPRT", "CIVL", "UCMF", "SEPD", "ISTG", "AMPH", "KUSS", "HS", "MMEC", "MA-I", "EXMC", "IMDC"
		};

		/// <summary>
		/// 
		/// </summary>
		public static List<string> EnforcementFactionsTags { get; } = new List<string>
		{
			"SEPD", "UCMF"
		};

		/// <summary>
		/// 
		/// </summary>
		public static IEnumerable<string> PlayerFactionExclusionList { get; } = new List<string>
		{
			"Pirate", "Rogue", "Outlaw", "Bandit"
		};

		public static List<string> NpcFirstNames { get; } = new List<string>
		{
			"Rosae", "Davith", "Soaph", "Elrin", "Svjetlana", "Zan", "Riya", "Kasdy", "Betrice", "Jaycobe", "Crayg",
			"Emilyse", "Edan", "Brialeagh", "Stanka", "Asan", "Dragoslav", "Vena", "Flyx", "Svetoslav", "Zaid",
			"Timoth", "Katlina", "Kimly", "Jenzen", "Megn", "Juith", "Cayedn", "Jaenelle", "Jayedn", "Alestra", "Madn",
			"Cayelyn", "Rayelyn", "Naethan", "Jaromil", "Laeila", "Aleigha", "Balee", "Kurson", "Kalina", "Allan",
			"Iskren", "Alexi", "Malax", "Baelleigh", "Harp", "Haelee", "Tijan", "Klatko", "Vojta", "Tasya", "Maslinka",
			"Ljupka", "Aubriena", "Danuella", "Jastin", "Idania", "Xandr", "Koba", "Roemary", "Dlilah", "Tanr",
			"Sobeska", "Zaiyah", "Lubka", "Bogomila", "Roderock", "Dayne", "Pribuska", "Kyel", "Svilena", "Laylah",
			"Tray", "Bobbyx", "Kaence", "Rade", "Gojslav", "Tugomir", "Drahomir", "Aldon", "Gyanna", "Jezzy", "Roseya",
			"Zand", "Saria", "Own", "Adriyel", "Ayana", "Spasena", "Vlade", "Kimbr", "Billix", "Landn", "Ylena",
			"Canning", "Slavka", "Gayge", "Dobroslaw", "Jasemine", "Jaden", "Ayna", "Slavomir", "Milaia", "Koale",
			"Elriot", "Ondrea", "Viliana", "Emex", "Ashir", "Yce", "Lyuboslav", "Makenna", "Senka", "Radacek", "Lilea",
			"Wilm", "Burian", "Randis", "Bentom", "Olver", "Charliza", "Vjera", "Caera", "Yasen", "Roselyna", "Venka",
			"Lana", "Nayla", "Ayaan", "Ryliea", "Nicholya", "Adriaenne", "Armanix", "Jazon", "Sulvan", "Roys", "Liyam",
			"Aebby", "Alextra", "Bogomil", "Kole", "Desree", "Zyre", "Haral", "Aerav", "Doriyan", "Rayely", "Helna",
			"Arman", "Zavyr", "Xavis", "Winson", "Arihan", "Adrihan", "Walkr", "Laera", "Victr", "Dobroniega", "Yan",
			"Maianna", "Leshi", "Niklas", "Rebexa", "Renaya", "Jaelyne", "Catlea", "Zdik", "Sereya", "Barba", "Desmon",
			"Arjun", "Boleslava", "Jaxson", "Thalira", "Leslaw", "Aevangelina", "Kade", "Jaro", "Charlise", "Loriya",
			"Ljubica", "Rober", "Iveanne", "Slavena", "Maikle", "Vladica", "Zdiska", "Berivoj", "Shaene", "Brencis",
			"Karina", "Yavor", "Darilan", "Aellana", "Landan", "Adit", "Jazzly", "Ozren", "Nyala", "Azarea", "Sveta",
			"Jaessa", "Aedyn", "Maecey", "Braeylee", "Julyen", "Vela", "Amelise", "Benjam", "Vierka", "Aibram"
		};

		public static List<string> NpcLastNames { get; } = new List<string>
		{
			"Fusepelt", "Andichanteau", "Aubemont", "Kantorovich", "Lomafort", "Borisov", "Wyverneyes", "Abaleilles",
			"Snowreaver", "Litvinchuk", "Vigny", "Vinet", "Milenkovic", "Lamassac", "Masterflower", "Holyblaze",
			"Boberel", "Deathcaller", "Saintimeur", "Châtissac", "Marblemane", "Calic", "Golitsyn", "Aboret",
			"Hardstalker", "Humblevalor", "Sergeyev", "Rameur", "Grassfire", "Forestrock", "Snowsteel", "Chaykovskiy",
			"Smartwoods", "Lightningeyes", "Vassemeur", "Proksch", "Saurriver", "Albignes", "Clarifort", "Pridemaul",
			"Deathhelm", "Vinogradov", "Châtiffet", "Wolinsk", "Limoze", "Chananas", "Hanak", "Popovic", "Noblearm",
			"Belemond", "Runemane", "Chamidras", "Chamigné", "Mildlight", "Kergatillon", "Truedreamer", "Slivkin",
			"Frostbone", "Greatthorne", "Woodtaker", "Nerevilliers", "Abavau", "Stamenkovikj", "Hardlight",
			"Roughsworn", "Nobleroot", "Chaunteau", "Lomages", "Vichanteau", "Laurelet", "Brichagnon", "Shieldsnout",
			"Nozac", "Burningwalker", "Peaceseeker", "Kavka", "Mistseeker", "Sugné", "Sedlak", "Firemore", "Prokesch",
			"Sendula", "Perlich", "Bricharel", "Morningwhisk", "Keenwoods", "Sublirac", "Vilart", "Raunas", "Dewheart",
			"Balaban", "Ravenpike", "Snowcreek", "Sarrarel", "Yellen", "Rochevès", "Croivès", "Chauvetelet", "Polyakov",
			"Mourningroar", "Rambunac", "Woodensworn", "Chabastel", "Fogshaper", "Fistbranch", "Chauthier", "Crerel",
			"Springhand", "Bougaiffet", "Angestel", "Stojanovska", "Bladekeeper", "Heartgloom", "Vajda", "Bloodwound",
			"Mucibaba", "Lhotsky", "Pinekeeper", "Abitillon", "Spiderarm", "Limolot", "Ragnac", "Chaustel", "Croille",
			"Michalek", "Cloudtoe", "Cressier", "Regalshadow", "Cabarkapa", "Snowchewer", "Twerski", "Voronov",
			"Shieldbane", "Gaibannes", "Roquemont", "Gaiffet", "Lamodieu", "Silentwhirl", "Fuseforce", "Farwood",
			"Bouldershade", "Rochedras", "Smolensky", "Bougairelli", "Graysnout", "Korda", "Lonebraid", "Agueleilles",
			"Chanaron", "Chanagnes", "Barassac", "Hnilo", "Popov", "Grayhair", "Younghorn", "Volinsky", "Boberon",
			"Topolski", "Kergassec", "Humblewhisk", "Longbend", "Whitrage", "Pyredrifter", "Wyvernflow", "Vernissier",
			"Dudar", "Chamiveron", "Carlowitz", "Waterbough", "Commonmight", "Raullane", "Boyko", "Wyvernhair",
			"Kovalevsky", "Astateuil", "Bonnetillon", "Dawnleaf", "Laurenteau", "Aguefelon", "Bonnemoux", "Baragre",
			"Kergallane", "Warvale", "Chanaffet", "Polyak", "Kohout", "Wach", "Dolezal", "Doomsprinter", "Malenkov",
			"Woodgazer", "Janowitz", "Golovin", "Milosevic", "Mourningkiller", "Novak", "Barleycrag", "Rabinowicz",
			"Bizelle", "Bohatec", "Rockstrider", "Snowore", "Chauvelet", "Andimtal", "Bonespirit", "Nerelle",
			"Ostrovsky", "Heavystriker", "Cindercutter", "Grasslance", "Baraffet", "Svehla"
		};
	}
}
