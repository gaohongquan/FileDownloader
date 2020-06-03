using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yunchang.Download
{
    public enum DownloadState
    {
        none,
        downloading,
        error,
        done
    }

    public interface IDownloadHandler
    {
        DownloadInfo info { get; set; }
        DownloadState state { get; set; }
        float progress { get; }
        void Start();
        void Cancel();
    }
}



