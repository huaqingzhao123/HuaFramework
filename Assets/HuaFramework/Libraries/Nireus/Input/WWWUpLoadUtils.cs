using System.Collections;
using UnityEngine;
namespace Nireus
{
    public class WWWUpLoadUtils
    {
        //public Sprite imgae;
        static string url = "http://10.0.101.67/wwb/Upload.php";
        static string CreateURL = "http://0.0.101.67/wwb/CreatFolder.php";
        static string DelURL = "http://0.0.101.67/wwb/DelFolder.php";

        void Start()
        {
            //StartCoroutine(CreatFolder("Photos"));//在服务器上创建文件夹
            //StartCoroutine(DelFolder("Photos"));//在服务器上删除文件夹
            //StartCoroutine(Upload());//上传图片到服务器指定的文件夹
        }

        //创建文件夹
        static IEnumerator CreatFolder(string FolderName)
        {
            WWWForm wForm = new WWWForm();
            wForm.AddField("FolderName", FolderName);
            WWW w = new WWW(CreateURL, wForm);
            yield return w;
            if (w.isDone)
            {
                GameDebug.Log("创建文件夹完成");
            }

            //yield return Upload();
        }

        //删除文件夹
        static IEnumerator DelFolder(string FolderName)
        {
            WWWForm wForm = new WWWForm();
            wForm.AddField("FolderName", FolderName);
            WWW w = new WWW(DelURL, wForm);
            yield return w;
            if (w.isDone)
            {
                GameDebug.Log("删除文件夹完成");
            }
        }

        //上传图片到指定的文件夹
        public static IEnumerator Upload(string filePath)
        {
            //byte[] bytes = SpriteToBytes(imgae);//获取图片数据
            string text = System.IO.File.ReadAllText(filePath);
            byte[] bytes = System.Text.Encoding.Default.GetBytes(text);
            WWWForm form = new WWWForm();//创建提交数据表单
            form.AddField("folder", "DevelopReplay/");//定义表单字段用来定义文件夹
            form.AddBinaryData("file", bytes, filePath, "image/png");//字段名，文件数据，文件名，文件类型
            WWW w = new WWW(url, form);
            yield return w;
            if (w.isDone)
            {
                GameDebug.Log("上传完成");
            }
        }

        //获取图片的二进制数据
        public byte[] SpriteToBytes(Sprite sp)
        {
            Texture2D t = sp.texture;
            byte[] bytes = t.EncodeToJPG();
            return bytes;
        }
    }

}
