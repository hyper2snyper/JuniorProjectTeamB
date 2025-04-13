using JuniorProject.Backend.Helpers;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace JuniorProject.Backend
{
    public abstract class Serializable
    {
        public Serializable() { }

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
            if (objectToSerialize is string str)
            {
                _SerializeString(str);
                return;
            }
            if (objectToSerialize is Serializable ser)
            {
                _SerializeObject(ser);
                return;
            }
            if (objectToSerialize is Vector2Int vi)
            {
                SerializeField(vi.X);
                SerializeField(vi.Y);
                return;
            }
            if (objectToSerialize is Vector2 v)
            {
                SerializeField(v.X);
                SerializeField(v.Y);
                return;
            }
            if (Nullable.GetUnderlyingType(typeof(T)) != null && objectToSerialize == null)
            {
                SerializeField(-1);
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

        void _SerializeString(string stringToSerialize)
        {
            SField f = new SField();
            f.str = stringToSerialize;
            f.isString = true;
            fields.Add(f);
        }
        void _SerializeObject(Serializable obj)
        {
            SerializeField(obj.GetType().ToString());
            fields.AddRange(obj.Serialize());
        }

        public void SerializeField<T>(List<T> listToSerialize, Delegate preSave = null) where T : notnull
        {
            if (!OrderCheck()) return;
            int listCount = listToSerialize.Count;
            SerializeField(listCount); //Prefix List saving with size.
            for (int i = 0; i < listCount; i++)
            {
                preSave?.DynamicInvoke(listToSerialize[i]);
                SerializeField(listToSerialize[i]);
            }
        }

        public void SerializeField<T>(List<List<T>> listToSerialize, Delegate preSave = null) where T : notnull
        {
            if (!OrderCheck()) return;
            int listCount = listToSerialize.Count;
            SerializeField(listCount); //Prefix List saving with size.
            for (int i = 0; i < listCount; i++)
            {
                SerializeField(listToSerialize[i], preSave);
            }
        }

        public void SerializeField<K, V>(Dictionary<K, V> dictionaryToSerialize, Delegate preSave = null)
            where K : notnull
            where V : notnull
        {
            if (!OrderCheck()) return;
            int keyCount = dictionaryToSerialize.Count;
            SerializeField(keyCount);
            foreach (KeyValuePair<K, V> pair in dictionaryToSerialize)
            {
                SerializeField(pair.Key);
                preSave?.DynamicInvoke(pair.Value);
                SerializeField(pair.Value);
            }
        }

        public void SerializeField<K, V>(Dictionary<K, List<V>> dictionaryToSerialize, Delegate preSave = null)
            where K : notnull
            where V : notnull
        {
            if (!OrderCheck()) return;
            SerializeField(dictionaryToSerialize.Count);
            foreach (KeyValuePair<K, List<V>> pair in dictionaryToSerialize)
            {
                SerializeField(pair.Key);
                SerializeField(pair.Value, preSave);
            }
        }

        public void SerializeField<T>(T[] array, Delegate preSave = null) where T : notnull
        {
            if (!OrderCheck()) return;
            SerializeField(array.Length);
            foreach (T item in array)
            {
                preSave?.DynamicInvoke(item);
                SerializeField(item);
            }

        }
        public void SerializeField<T>(T[,] array, Delegate preSave = null) where T : notnull
        {
            if (!OrderCheck()) return;
            int width = array.GetLength(0);
            int length = array.GetLength(1);
            SerializeField(width);
            SerializeField(length);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < length; y++)
                {
                    preSave?.DynamicInvoke(array[x, y]);
                    SerializeField(array[x, y]);
                }
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

        public T DeserializeField<T>(Delegate preLoad = null)
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
            if (Type.GetTypeCode(typeof(T)) == TypeCode.Object)
            {
                if (Nullable.GetUnderlyingType(typeof(T)) != null)
                {
                    int nextChar = reader.PeekChar();
                    if (nextChar == -1)
                    {
                        v = null;
                        return v;
                    }
                }
                if (typeof(T) == typeof(Vector2Int))
                {
                    v = new Vector2Int(reader.ReadInt32(), reader.ReadInt32());
                    return v;
                }
                if (typeof(T) == typeof(Vector2))
                {
                    v = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    return v;
                }
                v = DeserializeObject(preLoad);
                return v;
            }
            Debug.Print($"The non-base type [{typeof(T).Name}] was attempted to be deserialized. Only base types and strings should be serialized.");
            return v;
        }


        public List<T> DeserializeList<T>(Delegate preLoad = null) where T : notnull
        {
            List<T> returnList = new List<T>();
            int listCount = DeserializeField<int>();
            for (int i = 0; i < listCount; i++)
            {
                T field = DeserializeField<T>(preLoad);
                returnList.Add(field);
            }
            return returnList;
        }

        public List<List<T>> DeserializeNestedList<T>(Delegate preLoad = null) where T : notnull
        {
            List<List<T>> returnList = new List<List<T>>();
            int listCount = DeserializeField<int>();
            for (int i = 0; i < listCount; i++)
            {
                returnList.Add(DeserializeList<T>(preLoad));
            }
            return returnList;
        }

        public Dictionary<K, V> DeserializeDictionary<K, V>(Delegate preLoad = null)
            where K : notnull
            where V : notnull
        {
            Dictionary<K, V> returnDictionary = new Dictionary<K, V>();
            int listCount = DeserializeField<int>();
            for (int i = 0; i < listCount; i++)
            {
                K key = DeserializeField<K>();
                V field = DeserializeField<V>(preLoad);
                returnDictionary.Add(key, field);
            }
            return returnDictionary;
        }

        public Dictionary<K, List<V>> DeserializeNestedDictionary<K, V>(Delegate preLoad = null)
            where K : notnull
            where V : notnull
        {
            Dictionary<K, List<V>> returnDictionary = new Dictionary<K, List<V>>();
            int listCount = DeserializeField<int>();
            for (int i = 0; i < listCount; i++)
            {
                returnDictionary.Add(DeserializeField<K>(), DeserializeList<V>(preLoad));
            }
            return returnDictionary;
        }

        public T[] DeserializeArray<T>(Delegate preLoad = null) where T : notnull
        {
            int len = reader.ReadInt32();
            T[] returnArray = new T[len];
            for (int x = 0; x < len; x++)
            {
                T field = DeserializeField<T>(preLoad);
                returnArray[x] = field;
            }
            return returnArray;
        }

        public T[,] Deserialize2DArray<T>(Delegate preLoad = null) where T : notnull
        {
            int width = reader.ReadInt32();
            int length = reader.ReadInt32();
            T[,] returnArray = new T[width, length];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < length; y++)
                {
                    T field = DeserializeField<T>(preLoad);
                    returnArray[x, y] = field;
                }
            }
            return returnArray;
        }

        public Serializable DeserializeObject(Delegate preLoad = null)
        {
            string typeString = DeserializeField<string>();
            Type type = Type.GetType(typeString);
            Serializable objToReturn = (Serializable)Activator.CreateInstance(type);
            preLoad?.DynamicInvoke(objToReturn);
            objToReturn.Deserialize(reader, stringCache);
            return objToReturn;
        }

    }

    public class Serializer
    {
        Dictionary<Type, List<Serializable>> objectList = new Dictionary<Type, List<Serializable>>();
        string file_location;
        public delegate void postAction(ref FileStream f);
        public postAction arbitraryPostWrite;
        public postAction arbitraryPostRead;

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

            if (objectList.Count == 0)
            {
                Debug.Print("Save was called without any objects to be saved.");
                return;
            }
            bufferWriter.Write(objectList.Count); //Save type count.
            foreach (Type t in objectList.Keys)
            {
                bufferWriter.Write(t.ToString());
                bufferWriter.Write(objectList[t].Count);
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
            arbitraryPostWrite?.Invoke(ref fileStream);
            fileStream.Close();
        }

        public Dictionary<Type, List<Serializable>> Load()
        {
            objectList = new Dictionary<Type, List<Serializable>>();
            if (!File.Exists(file_location))
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

            }

            foreach (Type type in objectList.Keys)
            {
                for (int i = 0; i < objectCounts[type]; i++)
                {
                    Serializable serializable = (Serializable)Activator.CreateInstance(type);
                    serializable.Deserialize(reader, stringCache);
                    objectList[type].Add(serializable);
                }
            }
            arbitraryPostRead?.Invoke(ref fileStream);
            return objectList;

        }



    }
}