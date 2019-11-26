using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using ChatModel.Input;

namespace ChatServer
{
    public class MessageManager
    {
        private ConcurrentDictionary<int, ConcurrentQueue<CmdInfo>> dic;

        public MessageManager()
        {
            dic = new ConcurrentDictionary<int, ConcurrentQueue<CmdInfo>>();
        }

        public void Add(int key, CmdInfo msg)
        {
            if (dic.ContainsKey(key))
            {
                dic[key].Enqueue(msg);
            }
            else
            {
                ConcurrentQueue<CmdInfo> queue = new ConcurrentQueue<CmdInfo>();
                queue.Enqueue(msg);
                dic.TryAdd(key, queue);
            }
        }

        public ConcurrentQueue<CmdInfo> GetQueue(int to)
        {
            ConcurrentQueue<CmdInfo> queue;
            dic.TryGetValue(to, out queue);
            return queue;
        }
    }
}
