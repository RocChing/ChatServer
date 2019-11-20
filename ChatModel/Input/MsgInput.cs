using System;
using System.Collections.Generic;
using System.Text;

namespace ChatModel.Input
{
    public class MsgInput
    {
        public string Msg { get; set; }

        public MsgType Type { get; set; }

        public long From { get; set; }

        public string To { get; set; }

        public MsgToType ToType { get; set; }
    }
}
