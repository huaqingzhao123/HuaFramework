/* **************************************************************
* Copyright(c) 2016 Usens Inc, All Rights Reserved.  
* Description	CN 	: 
* Description	EN	: 
* Author           	: qnweng
* Created          	: #CreateTime#
* Revision by 		: 
* Revision History 	: 
******************************************************************/
#if !ASSET_RESOURCES && (!UNITY_IOS || USE_SUB_PACKAGE) 
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Nireus
{
    public interface IWebFileObserver
    {

#region Public variables

#endregion

#region Private variables

#endregion

#region Public methods

        void OnComplate(IWebFileObserver obs, byte[] datas,string url, object user_data, WebFileLoader loader);
        void OnError(IWebFileObserver obs, string error, object user_data);
        void OnProcess(IWebFileObserver obs, float p,int downloaded_bytes,object user_data);

#endregion

#region Private methods


#endregion

#region constructors

#endregion
    }

}
#endif