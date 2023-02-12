using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nireus
{
    public interface IAssetLoaderReceiver
    {
        void OnAssetLoadReceive(string url, UnityEngine.Object asset_data, System.Object info);
		void OnAssetLoadError(string url, System.Object info);

        /// <summary>
        /// @deprecated: 此方法有问题，暂时不要用
        /// </summary>
		void OnAssetLoadProgress(string url, System.Object info, float progress, int bytesLoaded, int bytesTotal);
    }
}
