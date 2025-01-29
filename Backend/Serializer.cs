using System.IO;
using System.Runtime.InteropServices;
using System.Text;

/*
 * 
 * Todo: Complex types like lists and such. Right now it only will save the basic types and strings.
 * 
 * 
 */


namespace JuniorProject.Backend
{
    public abstract class Serializable
    {

        public abstract int fieldCount { get; } //If -1, ignore.

        public unsafe byte[] SerializeField<T>(T objectToSerialize) where T : unmanaged
        {

            byte[] field = new byte[sizeof(T)];
            byte* p = (byte*)&objectToSerialize;
            for (int i = 0; i < field.Length; i++)
            {
                field[i] = *p++;
            }
            return field;
        }

        public unsafe byte[] SerializeField(string stringToSerialize)
        {
            byte[] field = new byte[stringToSerialize.Length + 1];
            for (int i = 1; i < stringToSerialize.Length + 1; i++)
            {
                field[i] = (byte)stringToSerialize[i - 1];
            }
            field[0] = (byte)stringToSerialize.Length; //This is for correct string formatting for BinaryReader.ReadString.
            return field;
        }

        public abstract void SerializeFields(List<byte[]> serializedFields);
        public abstract void Deserialize(BinaryReader reader);

        public List<byte[]> Serialize()
        {
            List<byte[]> fields = new List<byte[]>();
            SerializeFields(fields);
            if (fields.Count == 0)
            {
                Debug.Print($"{this} was passed into Serialization, but there were no fields saved.");
            }
            return fields;
        }
    }

    class Serializer
    {
        Dictionary<Type, List<Serializable>> objectList = new Dictionary<Type, List<Serializable>>();
        string file_location;


        public Serializer(string file_location)
        {
            this.file_location = file_location;
        }


        public void SaveObject(Serializable objectToSerialize)
        {
            Type objectType = objectToSerialize.GetType();
            if (!objectList.ContainsKey(objectType))
            {
                objectList[objectType] = new List<Serializable>();
            }
            objectList[objectType].Add(objectToSerialize);
        }

        public void Save()
        {
            FileStream fileStream = File.OpenWrite(file_location);
            fileStream.Flush(true);
            BinaryWriter writer = new BinaryWriter(fileStream, Encoding.ASCII, false);

            int typeCount = objectList.Count;
            Dictionary<Type, int> fieldCounts = new Dictionary<Type, int>();

            foreach (Type type in objectList.Keys)
            {
                fieldCounts[type] = objectList[type][0].fieldCount;
            }

            if (typeCount == 0)
            {
                Debug.Print("Save was called without any objects to be saved.");
                return;
            }

            writer.Write(typeCount); //Save type count.
            foreach (KeyValuePair<Type, int> fieldCount in fieldCounts)
            {
                writer.Write(fieldCount.Key.ToString()); //Write the type.
                writer.Write(objectList[fieldCount.Key].Count); //Write how many there are.
                writer.Write(fieldCount.Value); //Write how many fields each object should have.
            }

            foreach (Type type in objectList.Keys)
            {
                foreach (Serializable serializable in objectList[type])
                {
                    List<byte[]> fields = serializable.Serialize();
                    foreach (byte[] field in fields)
                    {
                        writer.Write(field);
                    }
                }
            }
            fileStream.Close();
        }

        public Dictionary<Type, List<Serializable>> Load()
        {
            objectList = new Dictionary<Type, List<Serializable>>();
            FileStream fileStream = File.OpenRead(file_location);
            BinaryReader reader = new BinaryReader(fileStream);

            int typeCount = reader.ReadInt32(); //Load type count;
            if (typeCount == 0)
            {
                Debug.Print($"{file_location} is an empty file, nothing loaded.");
                return new Dictionary<Type, List<Serializable>>();
            }

            Dictionary<Type, int> objectCounts = new Dictionary<Type, int>();
            Dictionary<Type, int> fieldCounts = new Dictionary<Type, int>();

            for (int i = 0; i < typeCount; i++)
            {
                string typeString = reader.ReadString(); //Load type.
                Type? type = Type.GetType(typeString);
                if (type == null)
                {
                    Debug.Print($"{typeString} was read in as a type from {file_location}, however, that type does not exist.");
                    return new Dictionary<Type, List<Serializable>>();
                }
                objectList.Add(type, new List<Serializable>());
                objectCounts.Add(type, reader.ReadInt32()); //Load the object count.
                fieldCounts.Add(type, reader.ReadInt32()); //load the object field count.
            }

            foreach (Type type in objectList.Keys)
            {
                for (int i = 0; i < objectCounts[type]; i++)
                {
                    Serializable serializable = (Serializable)Activator.CreateInstance(type);
                    if (serializable.fieldCount != fieldCounts[type] && (serializable.fieldCount != -1 && fieldCounts[type] != -1))
                    {
                        Debug.Print($"The type [{type.Name}] tried to be loaded from {file_location} with {serializable.fieldCount} fields, " +
                            $"however, the file has {fieldCounts[type]} fields.");
                        return new Dictionary<Type, List<Serializable>>();
                    }
                    serializable.Deserialize(reader);
                    objectList[type].Add(serializable);
                }
            }
            return objectList;

        }



    }
}
