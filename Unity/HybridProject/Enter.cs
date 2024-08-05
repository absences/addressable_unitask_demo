using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HybridProject
{
    public class Enter
    {
        public static void Init()
        {
            Debug.Log(11);
           // var t = new T();

           // var s = JsonTool.ToJson(t);
          //  UnityEngine.Debug.Log(s);

          //  UnityEngine.Debug.Log(JsonTool.FromJson<T>(s).a);


            new List<int>();
            new List<uint>();
            new List<long>();
            new List<ulong>();
            new List<string>();
            new List<short>();
            new List<ushort>();
            new List<double>();
            new List<Vector2>();
            new List<Vector3>();
            new List<Color>();
            new List<Quaternion>().ToList();


            //var test = new Tick2Backup();

            //test.Players.Add(new PlayerBackup());

            //Debug.Log(test);

            //var s1 = JsonTool.ToJson(test);

            //var t2 = JsonTool.FromJson<Tick2Backup>(test.ToString());

            //Debug.Log(t2.Players.Count);

            //var list1=new List<int>();  

            //list1.Add(1);   
            //list1.Add(2);

            //list1.Remove(1);
            //Debug.Log(list1[0]);



            //var list1=new List<T>();

            //list1.Add(new T());

            //Debug.Log(list1.Count);

            //Debug.Log(list1.Contains(new T ()));


            //var list = new List<MyVector2>();
            //list.Add(new MyVector2());
            //Debug.Log(list.Count);
        }
        public static void ShutDown()
        {

        }
        public static void Update(float f1,float f2)
        {

        }


    }
}
