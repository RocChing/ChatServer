using System;
using System.Collections.Generic;
using System.Text;

namespace ChatModel.Input
{
    public enum MsgType
    {
        /// <summary>
        /// 文本
        /// </summary>
        Text = 1,
        /// <summary>
        /// 图片
        /// </summary>
        Image = 2,
        /// <summary>
        /// 语音
        /// </summary>
        Voice = 3,
        /// <summary>
        /// 视频
        /// </summary>
        Video = 4,
        /// <summary>
        /// 链接
        /// </summary>
        Link = 5,
        /// <summary>
        /// 文件
        /// </summary>
        File = 6
    }
}
