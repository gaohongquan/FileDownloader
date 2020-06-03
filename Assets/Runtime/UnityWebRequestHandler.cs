using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class FileDownloadHandler : DownloadHandlerScript
{
    FileStream m_Stream;
    bool m_IsCancel;

    DownloadInfo m_DownloadInfo;

    public FileDownloadHandler()
    : base(new byte[1024 * 1024])
    {
    }

    public bool OpenOrCreateFile(DownloadInfo info)
    {
        try
        {
            m_IsCancel = false;
            m_DownloadInfo = info;
            m_Stream = File.Open(info.path,FileMode.OpenOrCreate, FileAccess.Write);
            m_Stream.Seek(info.currentSize, SeekOrigin.Begin);
        }
        catch (Exception exception)
        {
            return false;
        }
        return true;
    }

    protected override float GetProgress()
    {
        return (float)((double)m_DownloadInfo.currentSize / (double)m_DownloadInfo.totalSize);
    }

    protected override void ReceiveContentLength(int contentLength)
    {
        base.ReceiveContentLength(contentLength);
    }

    protected override bool ReceiveData(byte[] data, int dataLength)
    {
        if (data == null || dataLength == 0 || m_IsCancel || m_Stream == null)
            return false;
        try
        {
            m_Stream.Write(data, 0, dataLength);
            m_DownloadInfo.currentSize = m_Stream.Length;
        }
        catch (Exception e)
        {
            return false;
        }
        return true;
    }

    protected override void CompleteContent()
    {
        base.CompleteContent();
        CloseStream();
    }

    public void CloseStream()
    {
        if (m_Stream != null)
        {
            m_Stream.Close();
            m_Stream.Dispose();
            m_Stream = null;
        }
    }

    public void Cancel()
    {
        m_IsCancel = true;
    }
}


public class UnityWebRequestHandler : IDownloadHandler
{
    FileDownloadHandler m_FileDownloadHandler;

    public DownloadInfo info { get; set; }

    public DownloadState state { get; set; }

    public UnityWebRequestHandler()
    {
        state = DownloadState.none;
        m_FileDownloadHandler = new FileDownloadHandler();
    }

    public void Start()
    {
        state = DownloadState.downloading;

        var dirname = Path.GetDirectoryName(info.path);
        if (!Directory.Exists(dirname))
            Directory.CreateDirectory(dirname);

        if (info.currentSize == info.totalSize)
        {
            state = DownloadState.done;
        }
        else
        {
            if (!m_FileDownloadHandler.OpenOrCreateFile(info))
            {
                state = DownloadState.error;
                m_FileDownloadHandler.CloseStream();
            }
            else
            {
                var request = UnityWebRequest.Get(info.url);
                request.timeout = 60;
                request.redirectLimit = 1;
                request.downloadHandler = m_FileDownloadHandler;
                if (info.currentSize == 0)
                    request.SetRequestHeader("Range", "bytes=0-");
                else
                    request.SetRequestHeader("Range", "bytes=" + info.currentSize + "-" + info.totalSize);
                var operation = request.SendWebRequest();
                operation.completed += OnOperationCompleted;
            }
        }
    }

    public void Cancel()
    {
        m_FileDownloadHandler.Cancel();
    }

    private void OnOperationCompleted(AsyncOperation opt)
    {
        var operation = opt as UnityWebRequestAsyncOperation;
        var request = operation.webRequest;

        if (request.isNetworkError || request.isHttpError
            || info.currentSize < info.totalSize)
        {
            state = DownloadState.error;
            //info.error = request.error;
            //info.responseCode = request.responseCode;
        }
        else
        {
            /*string hashCode = GetMD5HashFromFile(m_TaskInfo.path);
            if (!info.md5.Equals(hashCode))
            {
                info.state = HttpDownloadState.error;
                info.error = info.request.error;
                info.responseCode = info.request.responseCode;
            }
            else
            {*/
                state = DownloadState.done;
            //}
        }
        m_FileDownloadHandler.CloseStream();
        request.Dispose();
    }

    public float progress => (float)((double)info.currentSize / (double)info.totalSize);
}
