using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

public class Data : MonoBehaviour
{
   public static void SaveProfile(PlayerData t_profile)
   {
        try
        {
            string path = Application.persistentDataPath + "/profile.dt"; //Leads to a path that always exist

            if (File.Exists(path))
                File.Delete(path);

            FileStream file = File.Create(path);

            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, t_profile);
            file.Close();
        }catch(Exception e)
        {
            Debug.Log("SAVE ERROR " + e);
        }
        
   }

    public static PlayerData LoadProfile()
    {
        PlayerData t_profile = new PlayerData();
        try
        {
            string path = Application.persistentDataPath + "/profile.dt";
            if (File.Exists(path))
            {
                FileStream file = File.Open(path, FileMode.Open);
                BinaryFormatter bf = new BinaryFormatter();
                t_profile = (PlayerData)bf.Deserialize(file);
            }
        }catch(Exception e)
        {
            Debug.Log("LOAD ERROR: " + e);
        }
        

        return t_profile;
    }
}
