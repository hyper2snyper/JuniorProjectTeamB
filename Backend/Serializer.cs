using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace JuniorProject.Backend
{
    public abstract class Serializable
    {

        public abstract int fieldCount { get; } //If -1, ignore.
        public struct SField
        {
            public bool isString;
            public byte[] bytes;
            public string str;
        }
        List<SField>? fields = new List<SField>();
        BinaryReader? reader;
        List<string>? stringCache;


        public List<SField> Serialize()
        {
            fields = new List<SField>();
            SerializeFields();
            if (fields.Count == 0)
            {
                Debug.Print($"{this} was passed into Serialization, but there were no fields saved.");
            }
            List<SField> fieldsToReturn = fields;
            fields = null;
            return fieldsToReturn;
        }

        public abstract void SerializeFields();

        public bool OrderCheck()
        {
            if (fields == null)
            {
                Debug.Print($"SerializeField() was called outside of a Serialize() call.");
                return false;
            }
            return true;
        }

        //Serializes all built-in types except string, object, dynamic
        public unsafe void SerializeField<T>(T objectToSerialize) where T : notnull
        {
            if (!OrderCheck()) return;
            if (typeof(T) == typeof(string))
            {
                _SerializeString((string)(object)objectToSerialize);
                return;
            }
            byte[] field = new byte[sizeof(T)];
            byte* p = (byte*)&objectToSerialize;
            for (int i = 0; i < field.Length; i++)
            {
                field[i] = *p++;
            }
            SField f = new SField();
            f.bytes = field;
            f.isString = false;
            fields.Add(f);
        }

        public void _SerializeString(string stringToSerialize)
        {
            SField f = new SField();
            f.str = stringToSerialize;
            f.isString = true;
            fields.Add(f);
        }

        public void SerializeList<T>(List<T> listToSerialize) where T : notnull
        {
            if (!OrderCheck()) return;
            int listCount = listToSerialize.Count;
            SerializeField(listCount); //Prefix List saving with size.
            for (int i = 0; i < listCount; i++)
            {
                SerializeField(listToSerialize[i]);
            }
        }

        public void SerializeNestedList<T>(List<List<T>> listToSerialize) where T : notnull
        {
            if (!OrderCheck()) return;
            int listCount = listToSerialize.Count;
            SerializeField(listCount); //Prefix List saving with size.
            for (int i = 0; i < listCount; i++)
            {
                SerializeList(listToSerialize[i]);
            }
        }

        public void SerializeDictionary<K, V>(Dictionary<K, V> dictionaryToSerialize)
            where K : notnull
            where V : notnull
        {
            if (!OrderCheck()) return;
            int keyCount = dictionaryToSerialize.Count;
            SerializeField(keyCount);
            foreach (KeyValuePair<K, V> pair in dictionaryToSerialize)
            {
                SerializeField(pair.Key);
                SerializeField(pair.Value);
            }
        }

        public void SerializeNestedDictionary<K, V>(Dictionary<K, List<V>> dictionaryToSerialize)
            where K : notnull
            where V : notnull
        {
            if (!OrderCheck()) return;
            int keyCount = dictionaryToSerialize.Count;
            SerializeField(keyCount);
            foreach (KeyValuePair<K, List<V>> pair in dictionaryToSerialize)
            {
                SerializeField(pair.Key);
                SerializeList(pair.Value);
            }
        }

        public void Deserialize(BinaryReader reader, List<string> stringCache)
        {
            this.reader = reader;
            this.stringCache = stringCache;
            DeserializeFields();
            this.reader = null;
            this.stringCache = null;
        }

        public abstract void DeserializeFields();

        public bool DOrderCheck()
        {
            if (fields == null || stringCache == null)
            {
                Debug.Print($"DeserializeField() was called outside of a Deserialize() call.");
                return false;
            }
            return true;
        }

        public T DeserializeField<T>()
        {
            dynamic? v = null;
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean: v = reader.ReadBoolean(); return v;
                case TypeCode.Byte: v = reader.ReadByte(); return v;
                case TypeCode.SByte: v = reader.ReadSByte(); return v;
                case TypeCode.Char: v = reader.ReadChar(); return v;
                case TypeCode.Int16: v = reader.ReadInt16(); return v;
                case TypeCode.UInt16: v = reader.ReadUInt16(); return v;
                case TypeCode.Int32: v = reader.ReadInt32(); return v;
                case TypeCode.UInt32: v = reader.ReadUInt32(); return v;
                case TypeCode.Int64: v = reader.ReadInt64(); return v;
                case TypeCode.UInt64: v = reader.ReadUInt64(); return v;
                case TypeCode.Single: v = reader.ReadSingle(); return v;
                case TypeCode.Double: v = reader.ReadDouble(); return v;
                case TypeCode.Decimal: v = reader.ReadDecimal(); return v;
                case TypeCode.String:
                    {
                        int stringPos = reader.ReadInt32();
                        v = stringCache[stringPos];
                        return v;
                    }
            }
            Debug.Print($"The non-base type [{typeof(T).Name}] was attempted to be deserialized. Only base types and strings should be serialized.");
            return v;
        }


        public List<T> DeserializeList<T>() where T : notnull
        {
            List<T> returnList = new List<T>();
            int listCount = DeserializeField<int>();
            for (int i = 0; i < listCount; i++)
            {
                returnList.Add(DeserializeField<T>());
            }
            return returnList;
        }

        public List<List<T>> DeserializeNestedList<T>() where T : notnull
        {
            List<List<T>> returnList = new List<List<T>>();
            int listCount = DeserializeField<int>();
            for (int i = 0; i < listCount; i++)
            {
                returnList.Add(DeserializeList<T>());
            }
            return returnList;
        }

        public Dictionary<K, V> DeserializeDictionary<K, V>()
            where K : notnull
            where V : notnull
        {
            Dictionary<K, V> returnDictionary = new Dictionary<K, V>();
            int listCount = DeserializeField<int>();
            for (int i = 0; i < listCount; i++)
            {
                returnDictionary.Add(DeserializeField<K>(), DeserializeField<V>());
            }
            return returnDictionary;
        }

        public Dictionary<K, List<V>> DeserializeNestedDictionary<K, V>()
            where K : notnull
            where V : notnull
        {
            Dictionary<K, List<V>> returnDictionary = new Dictionary<K, List<V>>();
            int listCount = DeserializeField<int>();
            for (int i = 0; i < listCount; i++)
            {
                returnDictionary.Add(DeserializeField<K>(), DeserializeList<V>());
            }
            return returnDictionary;
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
            File.WriteAllText(file_location, string.Empty);
            FileStream fileStream = File.OpenWrite(file_location);
            BinaryWriter writer = new BinaryWriter(fileStream, Encoding.ASCII, false);

            Stream buffer = new MemoryStream();
            BinaryWriter bufferWriter = new BinaryWriter(buffer); //For storing object data.

            Dictionary<string, int> stringCache = new Dictionary<string, int>(); //String value -> String Pos
            int stringCacheSize = 0;

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

            bufferWriter.Write(typeCount); //Save type count.
            foreach (KeyValuePair<Type, int> fieldCount in fieldCounts)
            {
                bufferWriter.Write(fieldCount.Key.ToString()); //Write the type.
                bufferWriter.Write(objectList[fieldCount.Key].Count); //Write how many there are.
                bufferWriter.Write(fieldCount.Value); //Write how many fields each object should have.
            }

            foreach (Type type in objectList.Keys)
            {
                foreach (Serializable serializable in objectList[type])
                {
                    List<Serializable.SField> fields = serializable.Serialize();
                    foreach (Serializable.SField field in fields)
                    {
                        if (field.isString)
                        {
                            int stringPos = 0;
                            if (stringCache.ContainsKey(field.str))
                            {
                                stringPos = stringCache[field.str];
                                bufferWriter.Write(stringPos); //We write the position of the string cache.
                                continue;
                            }
                            stringCache.Add(field.str, stringCacheSize); //Add the thing to the cache.
                            bufferWriter.Write(stringCacheSize);
                            stringCacheSize++; //increase cache size.
                            continue;
                        }
                        bufferWriter.Write(field.bytes);
                    }
                }
            }

            writer.Write(stringCacheSize); //Write how big the string cache is.
            foreach (string str in stringCache.Keys)
            {
                writer.Write(str);
            }
            buffer.Seek(0, SeekOrigin.Begin);
            buffer.CopyTo(fileStream);
            fileStream.Close();
        }

        public Dictionary<Type, List<Serializable>> Load()
        {
            objectList = new Dictionary<Type, List<Serializable>>();
            if(!File.Exists(file_location))
            {
                Debug.Print($"Tried to Load() [{file_location}], however, that file does not exist.");
                return new Dictionary<Type, List<Serializable>>();
            }
            FileStream fileStream = File.OpenRead(file_location);
            BinaryReader reader = new BinaryReader(fileStream);

            List<string> stringCache = new List<string>();
            int stringCacheSize = reader.ReadInt32();
            int pos = 0;
            for (int i = 0; i < stringCacheSize; i++)
            {
                stringCache.Add(reader.ReadString());
            }

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
                    serializable.Deserialize(reader, stringCache);
                    objectList[type].Add(serializable);
                }
            }
            return objectList;

        }



    }
}
