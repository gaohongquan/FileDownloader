using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DownloadAsyncOperation))]
public class DownloadManagerEditor : Editor
{
    private void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
}
