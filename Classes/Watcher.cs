using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

            Log.WriteLine("Watching " + string.Join(Environment.NewLine + "\t- ",  watchFile .Split(';')));
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
            var filesList = RunArguments.WatchFile;
            var files = filesList.Split(';');

            var allContent = new List<string>();
            foreach (var file in files)
            {
                var content = RetrieveContent(file);
                if (string.IsNullOrEmpty(content))
                {
                    continue;
                }

                // allContent.Add(content);
                var hash = MD5(file);

                try
                {
                    var timeString = RunArguments.FullDateTimeLog
                        ? DateTime.Now.ToShortTimeString()
                        : "";
                    timeString += " " + DateTime.Now.ToShortTimeString();
                    Log.Write(
                        $"{timeString} Changes detected; Updating {RunArguments.TargetFile} ... ");
                    CopyWatchFileInTarget(content, hash);
                    Log.WriteLine("[Done]");
                }
                catch (Exception exception)
                {
                    Log.WriteLine("[Failed] " + exception.Message);
                }
            }
        }

        private string RetrieveContent(string file)
        {
            file = file.Replace('/', '\\');

            if (!File.Exists(file))
            {
                return null;
            }

            var targetFile = RunArguments.TargetFile.Replace('/', '\\');
            if (File.Exists(targetFile))
            {
                RunArguments.LastChangeTime = File.GetLastWriteTime(file);
                if (RunArguments.LastChangeTime <= File.GetLastWriteTime(targetFile))
                {
                    return null;
                }
            }

            return File.ReadAllText(file);
        }

        private void CopyWatchFileInTarget(string watchFilesContent, string hash)
        {
            // SearchFor the marker

            var commentPrefix = Settings.Default.commentPrefix ?? "; ";
            var markerStart = $"{commentPrefix} ---- DynHostsStart - {hash}";
            var markerEnd = $"{commentPrefix} ---- DynHostsEnd - {hash}";

            var content = "";
            var targetFile = RunArguments.TargetFile.Replace('/', '\\');
            if (File.Exists(targetFile))
            {
                content = File.ReadAllText(targetFile);
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

            content = string.Format("{0}{1}{1}{1}{2}{1}{3}{1}{4}{1}", content, Environment.NewLine, markerStart,
                watchFilesContent, markerEnd);
            File.WriteAllText(targetFile, content);
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

        public static string MD5(string original)
        {
            using (var provider = System.Security.Cryptography.MD5.Create())
            {
                var builder = new StringBuilder();

                foreach (var b in provider.ComputeHash(Encoding.UTF8.GetBytes(original)))
                    builder.Append(b.ToString("x2").ToLower());

                return builder.ToString();
            }
        }
    }
}