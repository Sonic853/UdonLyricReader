
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Sonic853.Lyric
{
    public class MusicLrc : UdonSharpBehaviour
    {
        /// <summary>
        /// lrc文件
        /// </summary>
        [Header("txt 结尾的歌词格式文件")]
        [SerializeField] public TextAsset lrcFile;
        /// <summary>
        /// 音频
        /// </summary>
        [Header("音频")]
        [SerializeField] public AudioClip audioClip;
        /// <summary>
        /// 歌词文本
        /// </summary>
        [TextArea(3, 10)]
        public string[] lrcText = new string[0];
        /// <summary>
        /// 歌词时间
        /// </summary>
        public float[] lrcTime = new float[0];
        /// <summary>
        /// 歌词时长
        /// </summary>
        public float[] lineTime = new float[0];
        /// <summary>
        /// 偏移
        /// </summary>
        [Header("偏移")]
        [SerializeField] public float offset = 0f;
        [HideInInspector]
        public string[] lyricInfo = new string[] {
            // 歌曲：
            "",
            // 歌手：
            "",
            // 专辑：
            "",
            // 作词：
            "",
            // 歌词：
            "",
            // 时长：
            "",
        };
        /// <summary>
        /// 获取当前时间的第几行歌词
        /// </summary>
        /// <param name="musicLrc"></param>
        /// <param name="time"></param>
        /// <returns>无歌词为 -1，播放完为 -2</returns>
        public int GetLyricIndex(float time)
        {
            var musicLrc = this;
            if (musicLrc.lrcTime.Length == 0 && musicLrc.lrcFile != null) LyricReader.ReadLrcFile(ref musicLrc);
            if (musicLrc.lrcTime.Length == 0) return -1;
            time += musicLrc.offset;
            if (time < musicLrc.lrcTime[0]) return -1;
            for (int i = 0; i < musicLrc.lrcTime.Length; i++)
            {
                // 当前时间大于等于当前行歌词时间，小于下一行歌词时间
                if (time >= musicLrc.lrcTime[i] && (i == musicLrc.lrcTime.Length - 1 || time < musicLrc.lrcTime[i + 1]))
                {
                    return i;
                }
            }
            return -2;
        }
        /// <summary>
        /// 获取当前时间的歌词
        /// </summary>
        /// <param name="musicLrc"></param>
        /// <param name="time"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        // public string GetLyricTextOffset(int index, float time, int offset)
        public string GetLyricTextOffset(float time, int offset)
        {
            var musicLrc = this;
            if (musicLrc.lrcTime.Length == 0 && musicLrc.lrcFile != null) LyricReader.ReadLrcFile(ref musicLrc);
            var index = musicLrc.GetLyricIndex(time);
            if (index == -2) index = musicLrc.lrcTime.Length;
            if (index + offset < 0 || index + offset >= musicLrc.lrcTime.Length)
            {
                return "";
            }
            else
            {
                return musicLrc.lrcText[index + offset];
            }
        }
        /// <summary>
        /// 获取歌词文本
        /// </summary>
        /// <param name="musicLrc"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public string GetLyricText(float time)
        {
            var musicLrc = this;
            if (musicLrc.lrcTime.Length == 0 && musicLrc.lrcFile != null) LyricReader.ReadLrcFile(ref musicLrc);
            var index = musicLrc.GetLyricIndex(time);
            if (index < 0) return "";
            return musicLrc.lrcText[index];
        }
    }
}
