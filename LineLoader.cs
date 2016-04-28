using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codeLine
{
    class LineLoader
    {
        string m_strCurrentPath = "";
        public bool IsFinishFileSearch { get; set; }
        private int m_nCountLoad = 0;

        public LineLoader() {
            m_strCurrentPath = "";
        }
    
        public int LoadData() {
            while (true)
            {
                // 完成加载代码行
                if (!Program.TryDequeue(out m_strCurrentPath))
	            {
                    if (Program.IsFinishLoadFileInfo)
                    {
                        return m_nCountLoad;
                    }
                    else
                    {
                        continue;
                    }
	            }

                m_nCountLoad++;

                // 读取一个代码文件
                Queue<string> queueLines = new Queue<string>();
                using (FileStream fs = new FileStream(m_strCurrentPath, FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string str;
                        while ((str = sr.ReadLine()) != null)
                        {
                            queueLines.Enqueue(str);
                        }
                    }
                }

                // 加入加载完成完成队列中
                FileLinesManager.Instance.EnqueOne(new FileLines() {strName = m_strCurrentPath, queueLines = queueLines });
            }
        }
    }
}
