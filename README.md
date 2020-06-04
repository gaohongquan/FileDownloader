# FileDownloader
一个在Unity中实现的文件下载工具，支持多线程，支持断点续传，并且可以方便在协程中使用。支持 **HttpWebRequest** 和 **UnityWebRequest** 两种方式，建议优先使用 **UnityWebRequest** 方式。
### 快捷使用方式
```Java
IEnumerator DownloadFiles()
{
    DownloadInfo[] downloadInfos = LoadDownloadInfos();
    var downloadOperation = DownloadAsyncOperation.Start<UnityWebRequestHandler>(downloadInfos, 4);
    downloadOperation.completed += OnDownloadCompleted;
    yield return downloadOperation;
}
```
### 显示进度方式
```Java
IEnumerator DownloadFiles()
{
   DownloadInfo[] downloadInfos = LoadDownloadInfos();
   var downloadOperation = DownloadAsyncOperation.Start<UnityWebRequestHandler>(downloadInfos, 4);
   downloadOperation.completed += OnDownloadCompleted;
   while(!downloadOperation.isDone)
   {
      Debug.LogFormat("Download Progress:{0:f2} ", downloadOperation.progress);
      yield return null;
  }
}
```
### 说明
代码 `DownloadAsyncOperation.Start<UnityWebRequestHandler>(downloadInfos, 4)` 创建一个 **DownloadAsyncOperation** 对象实例，其中第一个参数是下载的文件列表，第二个参数是同时下载的文件数量。下载完成后，会回调 **completed** 接口。
