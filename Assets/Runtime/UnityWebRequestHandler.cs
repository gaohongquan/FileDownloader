using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Yunchang.Download
{
    public class FileDownloadHandler : DownloadHandlerScript
    {
        FileStream m_Stream;
        bool m_IsCancel;

        DownloadInfo m_DownloadInfo;

        public FileDownloadHandler(DownloadInfo info)
            :base(new byte[1024 * 1024])
        {
            m_DownloadInfo = info;
        }

        public bool OpenOrCreateFile()
        {
            try
            {
                m_IsCancel = false;
                m_Stream = File.Open(m_DownloadInfo.path, FileMode.OpenOrCreate, FileAccess.Write);
                m_Stream.Seek(m_DownloadInfo.currentSize, SeekOrigin.Begin);
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
                m_FileDownloadHandler = new FileDownloadHandler(info);
                if (!m_FileDownloadHandler.OpenOrCreateFile())
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
            if(m_FileDownloadHandler != null)
            {
                m_FileDownloadHandler.Cancel();
            }
        }

        private void OnOperationCompleted(AsyncOperation opt)
        {
            var operation = opt as UnityWebRequestAsyncOperation;
            var request = operation.webRequest;

            if (request.isNetworkError || request.isHttpError
                || info.currentSize < info.totalSize)
            {
                state = DownloadState.error;
            }
            else
            {
                state = DownloadState.done;
            }
            m_FileDownloadHandler.CloseStream();
            m_FileDownloadHandler = null;
            request.Dispose();
        }

        public float progress => (float)((double)info.currentSize / (double)info.totalSize);
    }
}
