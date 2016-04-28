using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codeLine
{
    public struct FileLines {
        public string strName;
        public Queue<string> queueLines;
    }


    public class FileLinesManager
    {
        public static FileLinesManager Instance = new FileLinesManager();
        public ConcurrentQueue<FileLines> m_loadedFileLines = new ConcurrentQueue<FileLines>();

        public void EnqueOne(FileLines fileLine) {
            m_loadedFileLines.Enqueue(fileLine);
        }

        public bool TryDequeue(out FileLines fileLines) {
            return m_loadedFileLines.TryDequeue(out fileLines);
        }
    }
}
