using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nireus;

namespace Nireus
{
	public struct Vector2Int  
	{
        public int x;
        public int y;
        public Vector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            return "("+x+","+y+")";
        }

        public static Vector2Int operator +(Vector2Int left, Vector2Int right)
        {
            Vector2Int r = new Vector2Int();
            r.x = left.x + right.x;
            r.y = left.y + right.y;
            return r;
        }

        public static Vector2Int operator -(Vector2Int left, Vector2Int right)
        {
            Vector2Int r = new Vector2Int();
            r.x = left.x - right.x;
            r.y = left.y - right.y;
            return r;
        }

        public static Vector2Int operator *(Vector2Int left, Vector2Int right)
        {
            Vector2Int r = new Vector2Int();
            r.x = left.x * right.x;
            r.y = left.y * right.y;
            return r;
        }

        public static Vector2Int operator/(Vector2Int left, Vector2Int right)
        {
            Vector2Int r = new Vector2Int();
            r.x = left.x / right.x;
            r.y = left.y / right.y;
            return r;
        }

        public static float Distance(Vector2Int v1,Vector2Int v2)
        {
            Vector2 v3 = new Vector2(v1.x, v1.y);
            Vector2 v4 = new Vector2(v2.x, v2.y);
            return Vector2.Distance(v3, v4);

        }

        public static Vector2Int ZERO
        {
            get { return new Vector2Int(0, 0); }
        }
    }
}