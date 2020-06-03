using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yunchang.Download
{
    public class DownloadAsyncOperation : CustomYieldInstruction
    {
        static int MAX_DOWNLOAD_TASK = 4;

        private IDownloadHandler[] m_DownloadHandles;
        private float m_Progress = 0;
        private bool m_IsDone = false;
        private event Action<DownloadAsyncOperation> m_Completed;

        public float progress => m_Progress;
        public override bool keepWaiting => isDone;
        public IDownloadHandler[] downloadHandles => m_DownloadHandles;


        public event Action<DownloadAsyncOperation> completed
        {
            add
            {
                m_Completed += value;
                if (isDone) m_Completed(this);
            }
            remove
            {
                m_Completed -= value;
            }
        }

        public bool isDone
        {
            get
            {
                if (m_DownloadHandles == null)
                    return true;
                Update();
                long current = 0, total = 0;
                for (int i = 0; i < m_DownloadHandles.Length; i++)
                {
                    current += m_DownloadHandles[i].info.currentSize;
                    total += m_DownloadHandles[i].info.totalSize;
                }
                m_Progress = (float)((double)current / (double)total);
                if (current == total && m_Completed != null)
                    m_Completed(this);
                return current == total;
            }
        }

        private void Update()
        {
            if (m_DownloadHandles == null)
                return;
            int count = 0;
            for (int i = 0; i < m_DownloadHandles.Length; i++)
            {
                if (m_DownloadHandles[i].state == DownloadState.downloading)
                    count++;
                else if (m_DownloadHandles[i].state == DownloadState.error)
                    m_DownloadHandles[i].state = DownloadState.none;
            }

            if (count >= MAX_DOWNLOAD_TASK)
                return;

            for (int i = 0; i < m_DownloadHandles.Length; i++)
            {
                if (m_DownloadHandles[i].state == DownloadState.none)
                {
                    m_DownloadHandles[i].Start();
                    break;
                }
            }
        }

        public void Cancel()
        {
            for (int i = 0; i < m_DownloadHandles.Length; i++)
            {
                if (m_DownloadHandles[i].state == DownloadState.downloading)
                    m_DownloadHandles[i].Cancel();
            }
            m_DownloadHandles = null;
        }

        public static DownloadAsyncOperation Start<T>(DownloadInfo[] files) where T : IDownloadHandler
        {
            var opt = new DownloadAsyncOperation();
            opt.m_DownloadHandles = new IDownloadHandler[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                opt.m_DownloadHandles[i] = (IDownloadHandler)Activator.CreateInstance<T>();
                opt.m_DownloadHandles[i].info = files[i];
            }
            return opt;
        }
    }
}
