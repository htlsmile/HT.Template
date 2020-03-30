using System;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace HT.Template.BackEnd.Tasks
{
    public abstract class AppTaskBase
    {
        protected AppTaskBase(int interval = 1000)
        {
            Id++;
            TaskName = $"[{Id}]{GetType().Name}";
            Interval = interval;
            lastTime = DateTime.Now.AddMilliseconds(-6 * Interval);
        }

        private static uint Id = 0;

        public virtual string TaskName { get; }

        public AppRepository Repository { get; private set; }

        private CancellationTokenSource cts;
        private DateTime lastTime;

        /// <summary>
        /// 时间间隔（单位ms）
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// 服务启动事件
        /// </summary>
        public event EventHandler<TaskEventArgs> TaskStarted;

        /// <summary>
        /// 服务停止事件
        /// </summary>
        public event EventHandler<TaskEventArgs> TaskStopped;

        /// <summary>
        /// 服务运行状态
        /// </summary>
        public bool IsRunning => DateTime.Now - lastTime < TimeSpan.FromMilliseconds(5 * Interval);

        public static int TaskCheckInterval => int.TryParse(Config.Configuration[nameof(TaskCheckInterval)], out var d) ? d : 600000;

        /// <summary>
        /// 启动服务
        /// </summary>
        public async Task Start()
        {
            cts = new CancellationTokenSource();
            await Task.Run(() =>
            {
                int times = 0;
                TaskStarted?.Invoke(this, new TaskEventArgs(TaskName));
                while (!cts.Token.IsCancellationRequested)
                {
                    times++;
                    if (times * Interval >= TaskCheckInterval)
                    {
                        Log.Information($"{TaskName}正在运行");
                        times = 0;
                    }
                    Repository = new AppRepository();
                    lastTime = DateTime.Now;
                    try
                    {
                        Run();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"{TaskName}抛出异常");
                    }
                    try
                    {
                        Task.Delay(Interval).Wait(cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
                Log.Warning($"{TaskName}已取消");
                TaskStopped?.Invoke(this, new TaskEventArgs(TaskName));
            }, cts.Token);
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public void Stop()
        {
            if (cts != null && !cts.IsCancellationRequested)
            {
                cts.Cancel();
            }
        }

        /// <summary>
        /// 需要重复执行的任务
        /// </summary>
        public abstract void Run();

    }

    public class TaskEventArgs : EventArgs
    {
        public TaskEventArgs(string taskName) => TaskName = taskName;

        public string TaskName { get; }
    }
}