using System;
using System.IO;
using System.Threading;
using PCL_CE.Neo.Core.App;

namespace PCL_CE.Neo.Core.App.Essentials;

public static class UpdateHelper
{
    public static Exception? Replace(string source, string target)
    {
        var backup = $"{target}.bak.{DateTime.Now:yyyyMMddHHmmss}";
        Exception? lastEx = null;
        try
        {
            source = Path.GetFullPath(source);
            target = Path.GetFullPath(target);
            File.Copy(target, backup);
            if (!File.Exists(backup)) throw new FileNotFoundException("备份目标文件失败", backup);
            
            var watcher = new FileSystemWatcher(Basics.GetParentPathOrEmpty(target), Path.GetFileName(target));
            var deletedEvent = new ManualResetEventSlim(false);
            watcher.Deleted += (_, _) => deletedEvent.Set();
            watcher.EnableRaisingEvents = true;
            File.Delete(target);
            if (!deletedEvent.Wait(TimeSpan.FromSeconds(3))) throw new TimeoutException("删除目标文件失败");
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            
            File.Copy(source, target);
            if (!File.Exists(target)) throw new FileNotFoundException("复制到目标文件失败", target);
        }
        catch (Exception ex)
        {
            if (File.Exists(backup) && !File.Exists(target)) File.Move(backup, target);
            lastEx = ex;
        }
        if (File.Exists(backup)) File.Delete(backup);
        return lastEx;
    }
}