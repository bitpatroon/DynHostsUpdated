using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Timers;
using DynHosts.Classes;
using Timer = System.Timers.Timer;

namespace DynHosts
{
    internal class Service : ServiceBase
    {
        public const string name = "BpnDynHostsUpdater";

        public Timer Timer { get; set; }
        protected List<string> buffer = new List<string>();

        public Service()
        {
            ServiceName = name;
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                base.OnStart(args);
                WriteLine("Starting service. ");
                Program.RunArguments.AsService = true;
                Program.Run();
                WriteLine("Success");

                var thread = new Thread(InitTimer);
                thread.Start();
            }
            catch (Exception exception)
            {
                WriteLine("Failed");
                Log.Errors.Add(exception.Message);
            }
        }

        protected override void OnStop()
        {
            base.OnStop();
            try
            {
                Write("Stopping service: ");
                Program.Stop();
                WriteLine("Success");
            }
            catch (Exception exception)
            {
                WriteLine("Failed");
                Log.Errors.Add(exception.Message);
            }
        }

        public void Dispose()
        {
            if (Log.Errors.Count > 0) Log.Flush();
        }


        public void StartService()
        {
            using (var sc = new ServiceController(name))
            {
                sc.Start();
            }
        }

        public void StopService()
        {
            using (var sc = new ServiceController(name))
            {
                sc.Stop();
            }
        }

        protected void Write(string text)
        {
            buffer.Add(text);
        }

        protected void WriteLine(string text)
        {
            if (buffer.Count > 0) text = string.Join("", buffer.ToArray()) + text;

            Log.WriteLine(text);
        }


        private void InitTimer()
        {
            Timer = new Timer();
            Timer.Elapsed += new ElapsedEventHandler(timer_Tick);
            Timer.Interval = 1000; // ms
            Timer.Enabled = true;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            Program.Watcher.HandleSingleRun();
            if (Program.RunArguments.StopSignaled) Timer.Enabled = false;
        }
    }
}