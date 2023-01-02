
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonLab.Lyric
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
        [HideInInspector] public string[] lrcText = new string[0];
        /// <summary>
        /// 歌词时间
        /// </summary>
        [HideInInspector] public float[] lrcTime = new float[0];
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
    }
}
