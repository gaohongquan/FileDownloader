using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Demo : MonoBehaviour
{
    public Object txtPrefab;
    public TextAsset manifest;
    public Text txtProgress;
    public Text txtTime;
    public Text txtSpeed;
    public Text txtSize;
    public Transform list;

    public Image imgProgress;
    public Button startButton;

    DownloadInfo[] m_TaskInfos;
    DownloadAsyncOperation m_DownloadOperation;
    Dictionary<string, Text> m_logTextCache = new Dictionary<string, Text>();
    

    float m_StartTime;

    void Start()
    {
        m_StartTime = 0.0f;
        startButton.onClick.AddListener(() =>
        {
            m_StartTime = Time.unscaledTime;
            m_DownloadOperation = DownloadAsyncOperation.Start<UnityWebRequestHandler>(m_TaskInfos);
        });
        LoadManifest();
    }

    private void OnDestroy()
    {
        if (m_DownloadOperation != null)
            m_DownloadOperation.Cancel();
    }

    void Update()
    {
        if (m_DownloadOperation == null)
            return;

        if (!m_DownloadOperation.isDone)
        {
            long size = 0;
            foreach (var task in m_DownloadOperation.downloadHandles)
            {
                size += task.info.currentSize;
                if (task.state == DownloadState.downloading)
                {
                    Log(task);
                }
            }
            txtProgress.text = string.Format("下载进度: {0:f2}%", m_DownloadOperation.progress * 100);
            txtSize.text = string.Format("下载量: {0:f2}M", size / (1024 * 1024));
            imgProgress.fillAmount = m_DownloadOperation.progress;

            float time = Time.unscaledTime - m_StartTime;
            txtTime.text = string.Format("下载耗时: {0:f2} (秒)", time);
            txtSpeed.text = string.Format("下载速度:{0:f2}K", size / time / 1024);
        }
        
    }

    void Log(IDownloadHandler handler)
    {
        Text logText;
        if(!m_logTextCache.TryGetValue(handler.info.name,out logText))
        {
            GameObject go = GameObject.Instantiate(txtPrefab) as GameObject;
            go.name = handler.info.name;
            go.transform.SetParent(list);
            logText = go.GetComponent<Text>();
            m_logTextCache.Add(handler.info.name, logText);
        }
        //logText.text = string.Format("{0} - {1:f2}%", task.info.name, task.progress * 100.0f);
        logText.text = handler.info.name + " - " + handler.info.currentSize;
        if (handler.progress < 1.0f)
            logText.color = Color.green;
        else
            logText.color = Color.white;
    }

    private void LoadManifest()
    {
        List<DownloadInfo> tasks = new List<DownloadInfo>();
        string[] lines = manifest.text.Split('\n');
        for(int i=1; i<lines.Length; i++)
        {
            string str = lines[i].Trim();
            if(str.Length > 0)
            {
                string[] item = str.Split('\t');
                DownloadInfo ti = new DownloadInfo
                {
                    name = item[0],
                    url = "http://dmccdn.sail2world.com/bundleupdate/android_v143714_202006021504/" + item[0],
#if UNITY_EDITOR
                    path = "Assets/../Download/" + item[0],
#else
                    path = Application.persistentDataPath + item[0],
#endif
                    totalSize = long.Parse(item[2]),
                    currentSize = 0
                };
                tasks.Add(ti);
            }
        }
        m_TaskInfos = tasks.ToArray();
    }
}
