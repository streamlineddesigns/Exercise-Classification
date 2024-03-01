using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public static class DataSaveManager
{
    public static void Serialize<T>(T obj, string filePath)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(filePath, FileMode.Create);

        try
        {
            formatter.Serialize(stream, obj);
        }
        catch (SerializationException e)
        {
            //Debug.LogError("Serialization failed! " + e.Message);
        }
        finally
        {
            stream.Close();
        }
    }

    public static T Deserialize<T>(string filePath) where T : new()
    {
        if (!File.Exists(filePath))
        {
            //Debug.Log("Serialization file not found at " + filePath);
            return new T();
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(filePath, FileMode.Open);
        T obj = default(T);

        try
        {
            obj = (T)formatter.Deserialize(stream);
        }
        catch (SerializationException e)
        {
            //Debug.LogError("Deserialization failed! " + e.Message);
        }
        finally
        {
            stream.Close();
        }

        return obj;
    }
}