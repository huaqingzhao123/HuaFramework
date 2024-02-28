using System.Collections;
using System.Collections.Generic;
using Nireus;
using UnityEngine;

public static class FastStringManager 
{
   //实测  如果使用Replace
   //先toString后Replace更快
   //先Replace后toString()  GC更小
   public static FastString fastString = new FastString(512);
   
}
