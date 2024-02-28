//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections;

//namespace Nireus
//{
//    public class UIUtils
//    {
//        static public Color parseColor(int color)
//        {
//            return new Color(((color >> 16) & 0xFF) / 255.0f, ((color >> 8) & 0xFF) / 255.0f, (color & 0xFF) / 255.0f);
//        }
//        static public string ChangerNumberToWenZi(int number)
//        {
//            string tempS = "";
//            int qian = number / 1000;
//            if (qian != 0)
//            {
//                tempS += Lang.Get("NUMBER_" + qian) + Lang.Get("QIAN");
//            }
//            int bai = (number - qian * 1000) / 100;
//            if (bai != 0)
//            {
//                tempS += Lang.Get("NUMBER_" + bai) + Lang.Get("BAI");
//            }
//            int shi = (number - qian * 1000 - bai * 100) / 10;
//            if (shi != 0)
//            {
//                tempS += Lang.Get("NUMBER_" + shi) + Lang.Get("SHI");
//            }
//            int ge = (number - qian * 1000 - bai * 100 - shi * 10);
//            if (bai != 0 && shi == 0 && ge != 0)
//            {
//                tempS += Lang.Get("LING");
//            }
//            if (ge != 0)
//            {
//                tempS += Lang.Get("NUMBER_" + ge);
//            }
//            return tempS;
//        }

//        static public Rect GetRect(RectTransform rect_transform)
//        {
//            if (rect_transform == null)
//            {
//                return new Rect();
//            }
//            Vector3[] corners = new Vector3[4];
//            rect_transform.GetWorldCorners(corners);
//            float width = Mathf.Abs(Vector2.Distance(corners[0], corners[3]));
//            float height = Mathf.Abs(Vector2.Distance(corners[0], corners[1]));
//            return new Rect(corners[0], new Vector2(width, height));
//        }

//         static public Vector3 TransToWorldPos(Vector3 inputMousePos,UIDialog tpl)
//        {
//            Vector3 worldPos = Vector3.zero;
//            Camera uiCamera = PopUpManager.Instance.GetUICamera(tpl);
//            if (uiCamera == null)
//            {
//                return worldPos;
//            }
//            RectTransform canvas_rect = tpl.GetComponent<RectTransform>();
//            if (canvas_rect == null)
//            {
//                return worldPos;
//            }
//            RectTransformUtility.ScreenPointToWorldPointInRectangle(canvas_rect, inputMousePos, uiCamera, out worldPos);
//            return worldPos;
//        }

//    }
//}
