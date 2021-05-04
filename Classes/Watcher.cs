using System;
using System.IO;
using System.Text.RegularExpressions;
using DynHosts.Properties;

namespace DynHosts.Classes
{
    class Watcher
    {
        public RunArguments RunArguments { get; set; }

        public void Run()
        {
            var watchFile = RunArguments.WatchFile;
            var targetFile = RunArguments.TargetFile;

            if (File.Exists(RunArguments.WatchFile))
            {
                RunArguments.LastChangeTime = File.GetLastWriteTime(watchFile);
            }

            Log.WriteLine("Watching " + watchFile);
            Log.WriteLine("Targeting " + RunArguments.TargetFile);

            if (RunArguments.AsService)
            {
                // enable the service timer
                return;
            }
            
            Log.WriteLine();
            Log.WriteLine("Press ESC to quit.");

            while (!RunArguments.StopSignaled)
            {
                Log.WriteLine("checking...");
                HandleSingleRun();

                HandleUserKeyPress();

                System.Threading.Thread.Sleep(1000);
            }
        }

        public void HandleSingleRun()
        {

            // check if the file was changed
            if (File.Exists(RunArguments.WatchFile))
            {
                RunArguments.LastChangeTime = File.GetLastWriteTime(RunArguments.WatchFile);
                if (RunArguments.LastChangeTime > File.GetLastWriteTime(RunArguments.TargetFile))
                {
                    Log.Write(
                        $"{DateTime.Now.ToShortTimeString()} Change detected; Updating {RunArguments.TargetFile} ... ");
                    CopyWatchFileInTarget();
                    Log.WriteLine("[Done]");
                }
            }
        }

        private void CopyWatchFileInTarget()
        {
            // SearchFor the marker

            var commentPrefix = Settings.Default.commentPrefix ?? "; ";
            var markerStart = $"{commentPrefix} ---- DynHostsStart";
            var markerEnd = $"{commentPrefix} ---- DynHostsEnd";

            var content = "";
            if (File.Exists(RunArguments.TargetFile))
            {
                content = File.ReadAllText(RunArguments.TargetFile);
                if (content.IndexOf(markerStart, StringComparison.Ordinal) >= 0)
                {
                    // Remove the content 

                    var markerStartRegex = Regex.Escape(markerStart);
                    var markerEndRegex = Regex.Escape(markerEnd);
                    var regex = new Regex("(\r?\n)*" + markerStartRegex + ".*" + markerEndRegex + "(\r?\n)*",
                        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline);
                    var matches = regex.Match(content);
                    content = regex.Replace(content, "");
                }
            }

            var watchFileContent = File.ReadAllText(RunArguments.WatchFile);
            content = string.Format("{0}{1}{1}{2}{1}{3}{1}{4}{1}", content, Environment.NewLine, markerStart,
                watchFileContent, markerEnd);
            File.WriteAllText(RunArguments.TargetFile, content);
        }

        private void HandleUserKeyPress()
        {
            if (!Environment.UserInteractive)
            {
                return;
            }

            if (!Console.KeyAvailable) return;
            ConsoleKeyInfo key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.Escape:
                    RunArguments.StopSignaled = true;
                    Log.WriteLine("Quitting...");
                    break;
            }
        }
    }
}