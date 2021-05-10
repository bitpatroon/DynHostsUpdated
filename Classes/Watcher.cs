using System;
using System.Collections.Generic;
using System.Data;
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
            var watchFiles = RunArguments.WatchFileList;
            var targetFile = RunArguments.TargetFile;

            Log.WriteLine("Watching " + string.Join(Environment.NewLine + "\t- ", RunArguments.WatchFileList));
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

                // SearchFor the marker and update IP (if found)
                var updatedContent = UpdateIpInSource(content);
                var updated = (updatedContent != content);
                var updatedInfo = updated ? " (with corrected IP)" : "";

                var hash = MD5(file);

                try
                {
                    var timeString = (RunArguments.FullDateTimeLog
                        ? DateTime.Now.ToShortDateString() + " "
                        : "") + DateTime.Now.ToShortTimeString();
                    Log.Write($"{timeString} Changes detected; Updating{updatedInfo} {RunArguments.TargetFile} ... ");
                    CopyWatchFileInTarget(updatedContent, hash);
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
                if (File.GetLastWriteTime(file) <= File.GetLastWriteTime(targetFile))
                {
                    // not changed
                    return null;
                }
            }

            return File.ReadAllText(file);
        }

        private void CopyWatchFileInTarget(string watchFilesContent, string hash)
        {
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

        private string UpdateIpInSource(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return content;
            }

            if (!Settings.Default.updateIP)
            {
                return content;
            }

            if (string.IsNullOrEmpty(Settings.Default.updateIPTarget))
            {
                return content;
            }

            var key = Settings.Default.updateIPLineContains ?? "BPN_DYNHOSTS_UPDATER";

            var r = new Regex("(\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}).*?" + key);
            var matches = r.Matches(content);

            if (matches.Count == 0)
            {
                return content;
            }

            var ipAddress = matches[0].Groups[1].Value;

            return content.Replace(ipAddress, Settings.Default.updateIPTarget);
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