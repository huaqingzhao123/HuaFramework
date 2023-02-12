using System.Collections.Generic;
using System;
using Nireus;
public class RandomService
{
	static RandomHandel s_onRandomCreat;

    static bool s_isFixedRandom = false;
    private static Dictionary<int, List<float>> s_randomDic = new Dictionary<int, List<float>>();
    static List<float> s_randomList = new List<float>();

    public static RandomHandel OnRandomCreat
    {
        get { return RandomService.s_onRandomCreat; }
        set { RandomService.s_onRandomCreat = value; }
    }

    public static void SetRandomList(List<float> list)
    {
        s_isFixedRandom = true;
        s_randomList = list;
    }
    public static void SetRandomDic(Dictionary<int, List<float>> dic)
    {
        s_isFixedRandom = true;
        s_randomDic = dic;
    }

    public static int GetRandomListCount()
    {
        return s_randomList.Count;
    }

	private static float RangeRandom(float min, float max)
	{
		//UnityEngine.Random.InitState((int)(0xFFFFFFFF & DateTime.Now.Ticks));
		return UnityEngine.Random.Range(min, max);
	}

	public static float Range(int ID, float min, float max)
    {
        if (!s_isFixedRandom)
        {
			float random = RangeRandom(min, max);
			bool isexit = s_randomDic.TryGetValue(ID, out List<float> list);
            if (list == null)
            {
                List<float> tempList = new List<float>();
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
            return GetFixedRandom(ID,min,max);
        }
    }

    static void DispatchRandom(int ID, float random)
    {
        if (s_onRandomCreat != null)
        {
            s_onRandomCreat(ID,random);
        }
    }

    static float GetFixedRandom(int ID, float min, float max)
    {
        if (s_randomDic.ContainsKey(ID))
        {
            s_randomDic.TryGetValue(ID, out List<float> tempList);
            if (tempList != null && tempList.Count!=0)
            {
                float random = tempList[0];
                tempList.RemoveAt(0);
                return random;
            }
            else
            {
                GameDebug.LogError("wocao，ID="+ID);
                return RangeRandom(min, max);
            }
        }
        else
        {
            return RangeRandom(min, max);
        }
    }

    public class FixRandom
    {
        public int m_RandomSeed = 0;

        int m_randomA = 9301;
        int m_randomB = 49297;
        int m_randomC = 233280;

        public FixRandom(int seed)
        {
            SetFixRandomSeed(seed);
        }

        public void SetFixRandomSeed(int seed)
        {
            m_RandomSeed = seed;
        }

        public void SetFixRandomParm(int a, int b, int c)
        {
            m_randomA = a;
            m_randomB = b;
            m_randomC = c;
        }

        public int GetFixRandom()
        {
            m_RandomSeed = Math.Abs((m_RandomSeed * m_randomA + m_randomB) % m_randomC);

            return m_RandomSeed;
        }

        public int Range(int min, int max)
        {
            if (max <= min)
                return min;
            int random = GetFixRandom();
            int range = max - min;
            int res = (random % range) + min;
            return res;
        }
    }
}

public delegate void RandomHandel(int ID, float random);
