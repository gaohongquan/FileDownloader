using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Net;
using System;

public class HttpWebRequestHandler : IDownloadHandler
{
    Task m_Task;
    CancellationTokenSource m_TokenSource;
    byte[] m_BufferBytes;

    public DownloadInfo info { get; set; }

    public DownloadState state { get; set; }

    public HttpWebRequestHandler()
    {
        m_BufferBytes = new byte[1024 * 1024];
        m_TokenSource = new CancellationTokenSource();
        state = DownloadState.none;
    }

    public void Start()
    {
        state = DownloadState.downloading;
        m_Task = Task.Factory.StartNew(Download, m_TokenSource.Token);
    }

    public void Cancel()
    {
        m_TokenSource.Cancel();
    }

    private void Download()
    {
        var dirname = Path.GetDirectoryName(info.path);
        if (!Directory.Exists(dirname))
            Directory.CreateDirectory(dirname);

        FileStream stream = null;
        HttpWebRequest request = null;
        WebResponse respone = null;
        Stream responseStream = null;

        try
        {
            stream = new FileInfo(info.path).Open(FileMode.OpenOrCreate, FileAccess.Write);
            request = (HttpWebRequest)WebRequest.Create(info.url);
            request.ServicePoint.ConnectionLimit = int.MaxValue;
            if (info.currentSize > 0)
            {
                stream.Seek(info.currentSize, SeekOrigin.Begin);
                request.AddRange((int)info.currentSize);
            }
            respone = request.GetResponse();
            responseStream = respone.GetResponseStream();
            int nReadSize = 0;
            while (info.currentSize < info.totalSize)
            {
                nReadSize = responseStream.Read(m_BufferBytes, 0, m_BufferBytes.Length);
                if (nReadSize > 0)
                {
                    stream.Write(m_BufferBytes, 0, nReadSize);
                    info.currentSize += nReadSize;
                }
                Thread.Sleep(10);
            }
            state = DownloadState.done;
        }
        catch (Exception exception)
        {
            state = DownloadState.error;
        }

        if (stream != null)
        {
            stream.Close();
        }

        if (respone != null)
        {
            respone.Close();
            respone.Dispose();
        }

        if (responseStream != null)
        {
            responseStream.Close();
        }
    }

    public float progress => (float)((double)info.currentSize / (double)info.totalSize);
}