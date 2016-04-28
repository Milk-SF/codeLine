using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace codeLine
{
    class Program
    {
        static Thread s_threadLoad;                            // 加载线程
        static List<Thread> s_lstCounterThread = new List<Thread>();        // 计数线程
        const int MaxFileNameStore = 100;

        // 所有的文件名
        static ConcurrentQueue<string> s_fileQueue = new ConcurrentQueue<string>();
        public static bool TryDequeue(out string str) {
            return s_fileQueue.TryDequeue(out str);
        }

        // 是否完成了对整个目录的搜索
        public static bool IsFinishLoadFileInfo { get; set; }
        // 是否完成了对整个代码行的加载
        public static bool IsFinishLoadLineInfo { get; set; }
        // 一共处理了多少个文件
        public static int s_nFileCount;

        public static int s_nPathLength;

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("错误：只接受一个代码路径参数！");
                return;
            }

            string strPath = args[0];
            if (!Directory.Exists(strPath))
            {
                Console.WriteLine("错误：路径不存在！");
                return;
            }

            s_nPathLength = strPath.Length;

            // 启动加载线程
            StartLoadThread();

            // 启动counter线程
            StartCounterThread();

            // 找出代码行
            FetchDirFiles(strPath, "");
            IsFinishLoadFileInfo = true;

            // 等待线程结束退出
            foreach (var threadCounter in s_lstCounterThread)
            {
                threadCounter.Join();
            }
            s_threadLoad.Join();

           // Console.WriteLine("处理文件数量：" + s_nFileCount);
        }

        static void StartLoadThread() {
            LineLoader lineLoader = new LineLoader();

            // 加载线程
            s_threadLoad = new Thread(new ThreadStart(() =>
            {
                //Console.WriteLine("加载线程启动成功");
                Stopwatch watch = new Stopwatch();
                watch.Start();
                s_nFileCount = lineLoader.LoadData();
                IsFinishLoadLineInfo = true;
                watch.Stop();
                //Console.WriteLine("加载线程终止" + watch.ElapsedMilliseconds);
            }));

            // 启动加载线程
            s_threadLoad.Start();
        }

        static void StartCounterThread() {
            // 多少个计数线程
            int nWorker = Math.Max(1, Environment.ProcessorCount - 2);
            for (int i = 0; i < nWorker; i++)
			{
                int nIndex = i;
                LineCounter lineCounter = new LineCounter();
                Thread threadCounter = new Thread(new ThreadStart(() =>
                {
                    //Console.WriteLine("counter" + nIndex + "线程启动成功");
                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    lineCounter.ProcessFile();
                    watch.Stop();
                    Console.WriteLine("counter" + nIndex + "线程终止" + watch.ElapsedMilliseconds);
                }));

                s_lstCounterThread.Add(threadCounter);
                threadCounter.Start();
			}
        }

        // 加载并打印文件目录
        static void FetchDirFiles(string strPath, string strFront) {
            //Console.WriteLine(strFront + "-" + strPath);

            // 防止加载太多内存占用
            while (s_fileQueue.Count > MaxFileNameStore)
            {
                Thread.Sleep(1);
            }

            string strChildFont = strFront + "  ";

            string[] strDirs = Directory.GetDirectories(strPath);
            foreach (var item in strDirs)
            {
                FetchDirFiles(item, strChildFont);
            }

            string[] strFiles = Directory.GetFiles(strPath);
            foreach (var item in strFiles)
            {
                if (item.EndsWith(".cpp") || item.EndsWith(".h") || item.EndsWith(".c"))
                {
                    //Console.WriteLine(strChildFont + "+" + item);

                    // 开始加载出来进行检测
                    s_fileQueue.Enqueue(item);
                }
            }
        }
    }
}
