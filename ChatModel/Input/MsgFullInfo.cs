using System;
using System.Collections.Generic;
using System.Text;

namespace ChatModel.Input
{
    public class MsgFullInfo
    {
        public bool NewMsg { get; set; }

        public long SessionId { get; set; }

        public long MsgAllLength { get; set; }

        public MsgFullInfo() { }

        public MsgFullInfo(long sessionId, long length)
        {
            NewMsg = false;
            SessionId = sessionId;
            MsgAllLength = length;
        }

        public MsgFullInfo(long id) : this(id, 0)
        {

        }

        public void Reset()
        {
            NewMsg = false;
            MsgAllLength = 0;
        }
    }
}
