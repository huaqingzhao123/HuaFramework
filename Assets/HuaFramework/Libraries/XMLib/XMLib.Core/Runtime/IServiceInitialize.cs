

using System.Collections;

namespace XMLib
{
    /// <summary>
    /// IServiceInitialize
    /// </summary>
    public interface IServiceInitialize
    {
        IEnumerator OnServiceInitialize();
    }

    /// <summary>
    /// IServiceInitialize
    /// </summary>
    public interface IServiceLateInitialize
    {
        IEnumerator OnServiceLateInitialize();
    }
}