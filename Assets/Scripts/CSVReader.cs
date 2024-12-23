using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.Networking;

public class CSVReader : MonoBehaviour
{
    public TextAsset textAssetData;
    public class Path
    {
        public List<Vector3> coordinates;
        public int Rx_Number;
        public float Path_power;
        public string Interaction_Description;
        public int Total_Interactions_for_Path;
    }

    public List<Path> pathList = new List<Path>();
    public string data;

    void Awake()
    {   
        
        data = textAssetData.text;
        ReadCSV();
    }

    void ReadCSV()
    {
        string[] data1 =  data.Split(new string[] {"\"", "\n"}, StringSplitOptions.None);
        
        for (int i = 1; i < data1.Length - 3; i+=3)
        {
            Path path = new Path();
            
            string[] initInfo = data1[i].Split(new string[] {","}, StringSplitOptions.None);

            //Debug.Log(initInfo[0]);
            //Debug.Log(initInfo[3]);

            
            path.Rx_Number = int.Parse(initInfo[0]);
            path.Path_power = float.Parse(initInfo[1]);
            path.Interaction_Description = initInfo[2];
            path.Total_Interactions_for_Path = int.Parse(initInfo[3]);
            

            ///
            string coordInfo = data1[i + 1].Trim('\"');
            coordInfo = coordInfo.Trim('[');
            coordInfo = coordInfo.Trim(']');

            string[] coordStrings = coordInfo.Split(new string[] {", "}, StringSplitOptions.None);

            List<Vector3> coords = new List<Vector3>();
            foreach (string coordString in coordStrings)
            {
                string[] tempStrings = coordString.Trim('\'').Split(new string[] {" "}, StringSplitOptions.None);

                string x = tempStrings[0];
                string y = tempStrings[1];
                string z = tempStrings[2];
                //Debug.Log(x);
                //Debug.Log(y);
                //Debug.Log(z);


                Vector3 coord = new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
                coords.Add(coord);
                path.coordinates = coords;
                
            }
            pathList.Add(path);

        }
        

    }

}
