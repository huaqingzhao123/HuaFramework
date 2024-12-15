namespace HuaFramework
{
    internal struct DirtyWordRNode
    {
        public int           start;
        public int           len;
        public DirtyWordType type;
    }

    internal enum DirtyWordType
    {
        // 字符替换
        StrReplace = 0,

        // 序号替换
        IndexReplace = 1,
    }
}