namespace Nireus
{
    public struct LoadingProgressData
    {
        public float percent;
        public float downloaded;
        public float total;
        public string progressStr;
        public LoadingProgressData(float percent, float downloaded_size_m, float total_size_m)
        {
            this.percent = percent;
            downloaded = downloaded_size_m;
            total = total_size_m;
            progressStr = "";
        }

        public void UpdateDownloadProgress(LoadingProgressData data)
        {
            percent = data.percent;
            downloaded = data.downloaded;
            total = data.total;
            progressStr = $"{downloaded:F1}M/{total:F1}M";
        }

        public void UpdatePercent(float percent)
        {
            this.percent = percent;
            this.progressStr = "";
        }
    }
}

