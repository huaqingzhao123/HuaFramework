using System.Collections.Generic;
using UnityEngine;
using Nireus;
public class RandomVector2Service
{
    static RandomVertor2Handel s_onRandomCreat;
    
    static bool s_isFixedRandom = false;
    static List<Vector2> s_randomList = new List<Vector2>();
    static Dictionary<int,List<Vector2>> s_randomDic = new Dictionary<int,List<Vector2>>();
    public static RandomVertor2Handel OnRandomCreat
    {
        get { return RandomVector2Service.s_onRandomCreat; }
        set { RandomVector2Service.s_onRandomCreat = value; }
    }
    
    public static void SetRandomList(Dictionary<int,List<Vector2>> dic)
    {
        s_isFixedRandom = true;
        s_randomDic = dic;
    }
    public static int GetRandomListCount()
    {
        return s_randomList.Count;
    }
    public static Vector2 GetRandInsideUnitCircle(int ID)
    {
        return RangeInsideUnitCircle(ID);
    }

    public static Vector2 RangeInsideUnitCircle(int ID)
    {
        if (!s_isFixedRandom)
        {
            Vector2 random = UnityEngine.Random.insideUnitCircle;
            
            bool isexit = s_randomDic.TryGetValue(ID, out List<Vector2> list);
            if (list == null)
            {
                List<Vector2> tempList = new List<Vector2>();
                tempList.Add(random);
                s_randomDic.Add(ID,tempList);
            }
            else
            {
                list.Add(random);
            }
            
            DispatchRandom(ID,random);
            return random;
        }
        else
        {
            return GetFixedRandom(ID);
        }
    }
    static void DispatchRandom(int ID,Vector2 random)
    {
        if (s_onRandomCreat != null)
        {
            s_onRandomCreat(ID,random);
        }
    }
    static Vector2 GetFixedRandom(int ID)
    {
        if (s_randomDic.ContainsKey(ID))
        {
            s_randomDic.TryGetValue(ID, out List<Vector2> tempList);
            if (tempList != null && tempList.Count!=0)
            {
                Vector2 random = tempList[0];
                tempList.RemoveAt(0);
                return random;
            }
            else
            {
                GameDebug.LogError("wocaoV2，ID="+ID);
                return UnityEngine.Random.insideUnitCircle;
            }
        }
        else
        {
            return UnityEngine.Random.insideUnitCircle;
        }
    }
}
public delegate void RandomVertor2Handel(int ID,Vector2 random);