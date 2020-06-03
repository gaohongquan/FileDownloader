using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Yunchang.Download;

[CustomEditor(typeof(DownloadAsyncOperation))]
public class DownloadManagerEditor : Editor
{
    private void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
}
