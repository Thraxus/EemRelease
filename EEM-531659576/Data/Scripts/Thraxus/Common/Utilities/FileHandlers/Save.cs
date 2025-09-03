using System;
using System.IO;
using Sandbox.ModAPI;

namespace Eem.Thraxus.Common.Utilities.FileHandlers
{
    public static class Save
    {
        public static void WriteBinaryFileToWorldStorage<T>(string fileName, T data)
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(fileName, typeof(T)))
                MyAPIGateway.Utilities.DeleteFileInWorldStorage(fileName, typeof(T));

            using (BinaryWriter binaryWriter = MyAPIGateway.Utilities.WriteBinaryFileInWorldStorage(fileName, typeof(T)))
            {
                if (binaryWriter == null)
                    return;
                byte[] binary = MyAPIGateway.Utilities.SerializeToBinary(data);
                binaryWriter.Write(binary);
            }
        }

        public static void WriteXmlFileToWorldStorage<T>(string fileName, T data)
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(fileName, typeof(T)))
                MyAPIGateway.Utilities.DeleteFileInWorldStorage(fileName, typeof(T));

            using (TextWriter textWriter = MyAPIGateway.Utilities.WriteFileInWorldStorage(fileName, typeof(T)))
            {
                if (textWriter == null)
                    return;
                string text = MyAPIGateway.Utilities.SerializeToXML(data);
                textWriter.Write(text);
            }
        }

        public static void WriteFileToWorldStorage<T>(string fileName, T data)
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(fileName, typeof(T)))
                MyAPIGateway.Utilities.DeleteFileInWorldStorage(fileName, typeof(T));

            using (TextWriter textWriter = MyAPIGateway.Utilities.WriteFileInWorldStorage(fileName, typeof(T)))
            {
                if (textWriter == null)
                    return;
                textWriter.Write(data);
            }
        }

        public static void WriteToSandbox(Type T)
        {
        }
    }
}