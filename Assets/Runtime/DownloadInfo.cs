using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yunchang.Download
{
    public class DownloadInfo
    {
        public string name { get; set; }
        public string url { get; set; }
        public string path { get; set; }
        public long totalSize { get; set; }
        public long currentSize { get; set; }
    }
}
