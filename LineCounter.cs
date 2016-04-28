using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codeLine
{
    public class LineCounter
    {
        private int m_nBlankLine;           // 空行
        private int m_nCommentLine;     // 注释
        private int m_nCodeLine;            // 有效行数
        private int m_nTotalLine;           // 总行书
        private string m_strPath;           // 路径
        private Queue<string> m_queueCurrent; // 代码行队列

        // 当前行是否包含该在多行注释内部
        private bool m_bBeginComment = false;
        // 当前处理的文件
        private FileLines m_currentFileLines;

        public void Init(FileLines fileLine)
        {
            m_strPath = fileLine.strName;
            m_queueCurrent = fileLine.queueLines;
            m_nTotalLine = m_queueCurrent.Count;

            m_nBlankLine = 0;
            m_nCommentLine = 0;
            m_nCodeLine = 0;
            m_bBeginComment = false;
        }

        // 开始处理文件
        public void ProcessFile()
        {
            while (true)
            {
                if (!FileLinesManager.Instance.TryDequeue(out m_currentFileLines))
                {
                    if (Program.IsFinishLoadLineInfo)
                    {
                        return;
                    }
                    else {
                        continue;
                    }
                }

                Init(m_currentFileLines);
                while (m_queueCurrent.Count > 0)
                {
                    string strLine = m_queueCurrent.Dequeue();
                    ProcessLine(strLine);
                }

                PrintCodeLineResult();
            }
        }

        public void ProcessLine(string strLine) {
            // 空行
            if (string.IsNullOrWhiteSpace(strLine))
            {
                m_nBlankLine++;
                return;
            }

            // 注释行
            string strRemoveComment = RemoveCommnet(strLine);
            
            // 代码行
            if (!string.IsNullOrWhiteSpace(strRemoveComment))
            {
                m_nCodeLine++;
            }
        }

        // 输出结果
        public void PrintCodeLineResult() {
            Console.WriteLine(string.Format("file:{0} total:{1} empty:{2} effective:{3} comment:{4}",
                m_strPath.Substring(Program.s_nPathLength, m_strPath.Length - Program.s_nPathLength),
                m_nTotalLine,
                m_nBlankLine,
                m_nCodeLine, 
                m_nCommentLine));
        }

        // 移除一行的注释
        private string RemoveCommnet(string strLine, bool hasRemoveOne = false /* 这一行是否已经被剔除过一次注释*/) {
            // 还没开始多行注释
            if (!m_bBeginComment)
            {
                int nIndexCStyle = strLine.IndexOf("/*");
                int nIndexCppStyle = strLine.IndexOf("//");

                //  “//” 这种类型的注释
                if ((nIndexCppStyle >= 0 && nIndexCStyle >=0 && nIndexCppStyle < nIndexCStyle)
                    || (nIndexCppStyle >= 0 && nIndexCStyle < 0))
                {
                    m_nCommentLine++;
                    return strLine.Substring(0, nIndexCppStyle);
                }
                // “/**/”这种类型的注释
                else if (nIndexCStyle >= 0)
                {
                    m_bBeginComment = true;
                    int nIndexEnd = strLine.IndexOf("*/", nIndexCStyle + 2);

                    // 一行最多只有一个注释
                    if (!hasRemoveOne)
                    {
                        m_nCommentLine++;
                    }

                    if (nIndexEnd >= 0)
                    {
                        m_bBeginComment = false;
                        return RemoveCommnet(strLine.Substring(0, nIndexCStyle) + strLine.Substring(nIndexEnd + 2, strLine.Length - nIndexEnd - 2), true);
                    }
                    else {
                        return strLine.Substring(0, nIndexCStyle);
                    }
                }
            }
            else { 
                // 在多行注释内部
                // 准备匹配 */
                m_nCommentLine++;

                int nIndexEnd = strLine.IndexOf("*/");
                if (nIndexEnd >= 0)
                {
                    m_bBeginComment = false;
                    return RemoveCommnet(strLine.Substring(nIndexEnd + 2, strLine.Length - nIndexEnd - 2), true);
                }
                else {
                    return "";
                }
            }

            return strLine;
        }
    }
}
