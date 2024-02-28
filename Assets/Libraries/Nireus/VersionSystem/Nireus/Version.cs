using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nireus
{
    public class Version
    {
        private int _length;
        private string _md5;
        private int _v1, _v2, _v3, _v4;
        private string _ver_str;
        public string ver_str { get { return _ver_str; } }
        public int v1() { return _v1; }
#if !ASSET_RESOURCES && (!UNITY_IOS || USE_SUB_PACKAGE)
        public string patch_path { get; set; } = "";
        private string _patch_url;
#endif
        public int v2() { return _v2; }
        public int v3() { return _v3; }
        public int v4() { return _v4; }

        public Version(string ver_str)
        {
            _ver_str = ver_str;
            _length = 0;
            _md5 = "";
            _v1 = _v2 = _v3 = _v4 = 0;
            if (string.IsNullOrEmpty(ver_str) == false)
            {
                string[] sv = ver_str.Split('.');
                if (sv.Length > 0) _v1 = int.Parse(sv[0]);
                if (sv.Length > 1) _v2 = int.Parse(sv[1]);
                if (sv.Length > 2) _v3 = int.Parse(sv[2]);
                if (sv.Length > 3) _v4 = int.Parse(sv[3]);
            }
        }

        public bool isOlderThan(Version v)
        {
            if (_v1 != v.v1()) return _v1 < v.v1();
            if (_v2 != v.v2()) return _v2 < v.v2();
            if (_v3 != v.v3()) return _v3 < v.v3();
            if (_v4 != v.v4()) return _v4 < v.v4();
            return false;
        }

        public bool isNewerThan(Version v)
        {
            if (_v1 != v.v1()) return _v1 > v.v1();
            if (_v2 != v.v2()) return _v2 > v.v2();
            if (_v3 != v.v3()) return _v3 > v.v3();
            if (_v4 != v.v4()) return _v4 > v.v4();
            return false;
        }

        public bool isEqual(Version v)
        {
            return _v1 == v.v1() && _v2 == v.v2() && _v3 == v.v3() && _v4 == v.v4();
        }
        public bool isEmpty()
        {
            return v1() == 0 && v2() == 0 && v3() == 0 && v4() == 0;
        }
        public void setMD5(string md5) { _md5 = md5; }
        public string getMd5() { return _md5; }

        public void setLength(int len) { _length = len; }
        public int getLength() { return _length; }
#if !ASSET_RESOURCES && (!UNITY_IOS || USE_SUB_PACKAGE)
        public void setPatchUrl(string patch_url) { _patch_url = patch_url; }
        public string getPatchUrl() { return _patch_url; }
#endif
        public override string ToString()
        {
            return ver_str;
        }
    }
}
