using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuaFramework.Architecture
{
    public interface ICanSendQuery :IBelongToArchitecture
    {

    }
    public static class SendQueryExtension
    {
        public static TResult SendQuery<TResult>(this ICanSendQuery self,IQuery<TResult> query)
        {
            return self.GetArchitecture().SendQuery(query);
        }
    }

}
