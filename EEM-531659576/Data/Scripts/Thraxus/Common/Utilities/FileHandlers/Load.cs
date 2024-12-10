using System.IO;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Common.Utilities.FileHandlers
{
	internal static class Load
	{
		public static T ReadBinaryFileInWorldStorage<T>(string fileName)
		{
			if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(fileName, typeof(T)))
				return default(T);

			using (BinaryReader binaryReader = MyAPIGateway.Utilities.ReadBinaryFileInWorldStorage(fileName, typeof(T)))
			{
				return MyAPIGateway.Utilities.SerializeFromBinary<T>(binaryReader.ReadBytes(binaryReader.ReadInt32()));
			}
		}

		public static T ReadXmlFileInWorldStorage<T>(string fileName)
		{
			if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(fileName, typeof(T)))
				return default(T);

			using (TextReader textReader = MyAPIGateway.Utilities.ReadFileInWorldStorage(fileName, typeof(T)))
			{
				return MyAPIGateway.Utilities.SerializeFromXML<T>(textReader.ReadToEnd());
			}
		}

		public static string ReadFileInWorldStorage<T>(string fileName)
		{
			if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(fileName, typeof(T)))
				return string.Empty;

			using (TextReader textReader = MyAPIGateway.Utilities.ReadFileInWorldStorage(fileName, typeof(T)))
			{
				return textReader.ReadToEnd();
			}
		}
	}
}