using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CPU_LoadingMonitor.Controllers
{
    public class HomeController : Controller
    {
        static Process masterCPU_Monitor;
        static Process slave1CPU_Monitor;
        static Process slave2CPU_Monitor;

        static List<float> masterMonitorData = new List<float>{0,0,0,0,0,0,0,0,0,0};
        static List<float> slave1MonitorData = new List<float>{0,0,0,0,0,0,0,0,0,0};
        static List<float> slave2MonitorData = new List<float>{0,0,0,0,0,0,0,0,0,0};

        static HomeController()
        {
            masterCPU_Monitor = new Process();
            masterCPU_Monitor.StartInfo.FileName = "bash";
            masterCPU_Monitor.StartInfo.RedirectStandardInput = true;
            masterCPU_Monitor.StartInfo.RedirectStandardOutput = true;
            masterCPU_Monitor.StartInfo.CreateNoWindow = true;
            masterCPU_Monitor.Start();

            slave1CPU_Monitor = new Process();
            slave1CPU_Monitor.StartInfo.FileName = "bash";
            slave1CPU_Monitor.StartInfo.RedirectStandardInput = true;
            slave1CPU_Monitor.StartInfo.RedirectStandardOutput = true;
            slave1CPU_Monitor.StartInfo.CreateNoWindow = true;
            slave1CPU_Monitor.Start();
            slave1CPU_Monitor.StandardInput.WriteLine($"ssh slave1");
            slave1CPU_Monitor.StandardInput.Flush();
            ClearProcess(slave1CPU_Monitor);

            slave2CPU_Monitor = new Process();
            slave2CPU_Monitor.StartInfo.FileName = "bash";
            slave2CPU_Monitor.StartInfo.RedirectStandardInput = true;
            slave2CPU_Monitor.StartInfo.RedirectStandardOutput = true;
            slave2CPU_Monitor.StartInfo.CreateNoWindow = true;
            slave2CPU_Monitor.Start();
            slave2CPU_Monitor.StandardInput.WriteLine($"ssh slave1");
            slave2CPU_Monitor.StandardInput.Flush();
            ClearProcess(slave2CPU_Monitor);

            Task.Run(async delegate
            {
                while(true)
                {
                    masterCPU_Monitor.StandardInput.WriteLine("top -d 0.5 -b -n2 | grep \"Cpu(s)\"|tail -n 1 | awk '{print $2 + $4}'");
                    masterCPU_Monitor.StandardInput.Flush();
                    masterMonitorData.Add(float.Parse(masterCPU_Monitor.StandardOutput.ReadLine()));
                    ClearProcess(masterCPU_Monitor);
                    
                    slave1CPU_Monitor.StandardInput.WriteLine("top -d 0.5 -b -n2 | grep \"Cpu(s)\"|tail -n 1 | awk '{print $2 + $4}'");
                    slave1CPU_Monitor.StandardInput.Flush();
                    slave1MonitorData.Add(float.Parse(slave1CPU_Monitor.StandardOutput.ReadLine()));
                    ClearProcess(slave1CPU_Monitor);

                    slave2CPU_Monitor.StandardInput.WriteLine("top -d 0.5 -b -n2 | grep \"Cpu(s)\"|tail -n 1 | awk '{print $2 + $4}'");
                    slave2CPU_Monitor.StandardInput.Flush();
                    slave2MonitorData.Add(float.Parse(slave2CPU_Monitor.StandardOutput.ReadLine()));
                    ClearProcess(slave2CPU_Monitor);

                    if(masterMonitorData.Count > 10)
                        masterMonitorData.RemoveAt(0);
                    if(slave1MonitorData.Count > 10)
                        slave1MonitorData.RemoveAt(0);
                    if(slave2MonitorData.Count > 10)
                        slave2MonitorData.RemoveAt(0);
                    await Task.Delay(1000);
                }
            });
        }
        static void ClearProcess(Process process)
        {
            while (process.StandardOutput.Peek() > -1)
            {
                process.StandardOutput.ReadLine();
            }
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        public JsonResult GetChart()
        {
            float[][] monitorData = new float[10][];
            for(int i = 0; i < 10; i++)
            {
                monitorData[i] = new float[4];
                monitorData[i][0] = i - 10;
                monitorData[i][1] = masterMonitorData.Count > i ? masterMonitorData[i] : 0;
                monitorData[i][2] = slave1MonitorData.Count > i ? slave1MonitorData[i] : 0;
                monitorData[i][3] = slave2MonitorData.Count > i ? slave2MonitorData[i] : 0;
            }

            return Json(monitorData);
        }
    }
}
