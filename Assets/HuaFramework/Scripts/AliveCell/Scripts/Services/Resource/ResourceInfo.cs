/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2019/10/12 16:52:48
 */

using XMLib.DataHandlers;

namespace AliveCell
{
    /// <summary>
    /// ResourceInfo
    /// </summary>
    [System.Serializable]
    [DataContract]
    [ProtoBuf.ProtoContract]
    public class ResourceInfo
    {
        [DataMember]
        [ProtoBuf.ProtoMember(1)]
        public int id;

        [DataMember]
        [ProtoBuf.ProtoMember(2)]
        public string name;

        [DataMember]
        [ProtoBuf.ProtoMember(3)]
        public ResourceType type;

        [DataMember]
        [ProtoBuf.ProtoMember(4)]
        public string path;

        [DataMember]
        [ProtoBuf.ProtoMember(5)]
        public int preloadCnt;

        public override string ToString()
        {
            return $"[RI]({id},{name},{type},{path},{preloadCnt})";
        }
    }
}