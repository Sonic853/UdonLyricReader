
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonLab
{
    public class LyricReader : UdonSharpBehaviour
    {
        // [SerializeField] private MusicLrc[] musicLrcs;
        /// <summary>
        /// lrc文件
        /// </summary>
        [Header("txt 结尾的歌词格式文件")]
        [SerializeField] private TextAsset[] lrcFiles;
        /// <summary>
        /// 音频
        /// </summary>
        [Header("音频")]
        [SerializeField] public AudioClip[] audioClips;
        /// <summary>
        /// 歌词文本
        /// </summary>
        [HideInInspector] private string[][] lrcText;
        /// <summary>
        /// 歌词时间
        /// </summary>
        [HideInInspector] private float[][] lrcTime;
        /// <summary>
        /// 偏移
        /// </summary>
        [SerializeField] private float[] offsets;
        /// <summary>
        /// 歌曲信息
        /// </summary>
        [HideInInspector] private string[][] lyricInfo;
        //  = new string[][] {
        //         // 歌曲：
        //         "",
        //         // 歌手：
        //         "",
        //         // 专辑：
        //         "",
        //         // 作词：
        //         "",
        //         // 歌词：
        //         "",
        //         // 时长：
        //         "",
        //     };
        /// <summary>
        /// 当前音乐索引
        /// </summary>
        [NonSerialized] public int currentMusicIndex = -1;
        void Start()
        {
            // for (int i = 0; i < musicLrcs.Length; i++)
            // {
            //     ReadLrcFile(musicLrcs[i]);
            // }
            if (lrcText.Length == 0 || lrcTime.Length == 0 || offsets.Length == 0 || lyricInfo.Length == 0)
            {
                lrcText = new string[lrcFiles.Length][];
                lrcTime = new float[lrcFiles.Length][];
                offsets = new float[lrcFiles.Length];
                lyricInfo = new string[lrcFiles.Length][];
                for (int i = 0; i < lrcFiles.Length; i++)
                {
                    ReadLrcFile(i);
                }
            }
        }
        // void ReadLrcFile(MusicLrc musicLrc)
        void ReadLrcFile(int index)
        {
            // var _lrcFile = musicLrc.lrcFile;
            var _lrcFile = lrcFiles[index];
            if (_lrcFile == null)
                return;
            var _lrcTime = new float[0];
            var _lrcText = new string[0];
            var _offset = 0f;
            var _lyricInfo = new string[] {
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
            string[] lines = _lrcFile.text.Split('\n');
            if (_lrcFile.text.Contains("\r\n"))
            {
                lines = _lrcFile.text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            }
            float[] times = new float[0];
            string lyric = "";
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                // [by:LRC 文件的创建者]
                // [offset:+/- 以毫秒为单位的整体时间戳调整，+ 时间上移，- 下移]
                // [re:创建 LRC 文件的播放器或编辑器]
                // [ve:程序版本]

                if (line.StartsWith("["))
                {
                    for (int j = 0; j < times.Length; j++)
                    {
                        int _index = floatArrayFindMax(_lrcTime, times[j]);
                        _lrcTime = floatArrayAddIndex(_lrcTime, times[j], _index);
                        _lrcText = stringArrayAddIndex(_lrcText, lyric, _index);
                    }
                    if (line.Length > 1 && char.IsDigit(line[1]))
                    {
                        // 分离时间和歌词
                        // [00:00.00]歌词
                        // [00:00.000]歌词
                        // [00:00]歌词
                        // [00:00:00]歌词
                        // [00:00.00][00:00.00]歌词
                        // 一行歌词可能有多个时间
                        string[] timeAndLyric = line.Split(']');
                        // [00:00.00 歌词
                        // [00:00 歌词
                        // [00:00:00 歌词
                        // [00:00.00 [00:00.00 歌词
                        // if (timeAndLyric.Length > 1)
                        // {
                        times = new float[0];
                        lyric = "";
                        for (int j = 0; j < timeAndLyric.Length; j++)
                        {
                            if (timeAndLyric[j].Trim().StartsWith("["))
                            // [00:00.00
                            // [00:00.000
                            // [00:00
                            // [00:00:00
                            // [00:00.00 [00:00.00
                            {
                                float time = stringTimeToFloat(timeAndLyric[j].Trim().Substring(1));
                                if (time != -1)
                                {
                                    times = floatArrayAddIndex(times, time, floatArrayFindMax(times, time));
                                }
                            }
                            else
                            // 歌词
                            {
                                lyric += timeAndLyric[j].Trim();
                            }
                            if (times.Length == 1 && timeAndLyric.Length == 1)
                            {
                                lyric = "";
                            }
                        }
                        // }
                    }
                    else switch (true)
                        {
                            // [ar:歌手名]
                            case true when line.StartsWith("[ar:"):
                                {
                                    _lyricInfo[1] = "歌手：" + line.Substring(4, line.LastIndexOf(']') - 4);
                                }
                                break;
                            // [al:专辑]
                            case true when line.StartsWith("[al:"):
                                {
                                    _lyricInfo[2] = "专辑：" + line.Substring(4, line.LastIndexOf(']') - 4);
                                }
                                break;
                            // [ti:歌词（歌曲）标题]
                            case true when line.StartsWith("[ti:"):
                                {
                                    _lyricInfo[0] = "歌曲：" + line.Substring(4, line.LastIndexOf(']') - 4);
                                }
                                break;
                            // [au:作词]
                            case true when line.StartsWith("[au:"):
                                {
                                    _lyricInfo[3] = "作词：" + line.Substring(4, line.LastIndexOf(']') - 4);
                                }
                                break;
                            // [by:LRC 文件的创建者]
                            case true when line.StartsWith("[by:"):
                                {
                                    _lyricInfo[4] = "歌词：" + line.Substring(4, line.LastIndexOf(']') - 4);
                                }
                                break;
                            // [length:这首歌有多长]
                            case true when line.StartsWith("[length:"):
                                {
                                    _lyricInfo[5] = "时长：" + line.Substring(8, line.LastIndexOf(']') - 8);
                                }
                                break;
                            case true when line.StartsWith("[offset:"):
                                {
                                    // +/- 以毫秒为单位的整体时间戳调整，+ 时间上移，- 下移
                                    string offsetStr = line.Substring(8, line.LastIndexOf(']') - 8);
                                    // [offset:+0]
                                    if (offsetStr.StartsWith("+"))
                                    {
                                        // 解析不报错
                                        if (float.TryParse(offsetStr.Substring(1), out float offset))
                                        {
                                            _offset = offset / 1000f;
                                        }
                                    }
                                    // [offset:0]
                                    // [offset:-0]
                                    else
                                    {
                                        // 解析不报错
                                        if (float.TryParse(offsetStr, out float offset))
                                        {
                                            _offset = offset / 1000f;
                                        }
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                }
                else
                {
                    // 换行歌词
                    if (times.Length > 0 && lines[i] != "")
                    {
                        lyric += "\n" + line;
                    }
                }
            }
            for (int j = 0; j < times.Length; j++)
            {
                int _index = floatArrayFindMax(_lrcTime, times[j]);
                _lrcTime = floatArrayAddIndex(_lrcTime, times[j], _index);
                _lrcText = stringArrayAddIndex(_lrcText, lyric, _index);
            }
            lrcTime[index] = _lrcTime;
            lrcText[index] = _lrcText;
            offsets[index] = _offset;
            lyricInfo[index] = _lyricInfo;
            // musicLrc.lrcTime = _lrcTime;
            // musicLrc.lrcText = _lrcText;
            // musicLrc.offset = _offset;
            // musicLrc.lyricInfo = _lyricInfo;
        }
        float stringTimeToFloat(string value)
        {
            string[] time = value.Split(':');
            // [01:02.03]
            // [01:02.003]
            if (time.Length == 2 && value.Contains("."))
            {
                // 01
                // 02.03
                // 02.003
                string[] _time = time[1].Split('.');
                // 02
                // 03
                // 003
                int minute = int.Parse(time[0]);
                int second = int.Parse(_time[0]);
                // _time[1] 补齐 3 位
                int millisecond = int.Parse(_time[1].PadRight(3, '0'));
                return minute * 60 + second + millisecond / 1000f;
            }
            // [00:00]
            else if (time.Length == 2)
            {
                int minute = int.Parse(time[0]);
                int second = int.Parse(time[1]);
                // return minute * 60 * 1000 + second * 1000;
                return minute * 60 + second;
            }
            // [00:00:00]
            else if (time.Length == 3)
            {
                int minute = int.Parse(time[0]);
                int second = int.Parse(time[1]);
                int millisecond = int.Parse(time[2].PadRight(3, '0'));
                // return minute * 60 * 1000 + second * 1000 + millisecond;
                return minute * 60 + second + millisecond / 1000f;
            }
            return -1;
        }
        int stringArrayIndexOf(string[] array, string value)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == value)
                {
                    return i;
                }
            }
            return -1;
        }
        string[] stringArrayAdd(string[] array, string value)
        {
            string[] newArray = new string[array.Length + 1];
            array.CopyTo(newArray, 0);
            newArray[array.Length] = value;
            return newArray;
        }
        string[] stringArrayAddIndex(string[] array, string value, int index)
        {
            string[] newArray = new string[array.Length + 1];
            for (int i = 0; i < index; i++)
            {
                newArray[i] = array[i];
            }
            newArray[index] = value;
            for (int i = index + 1; i < array.Length; i++)
            {
                newArray[i] = array[i - 1];
            }
            return newArray;
        }
        string[] stringArrayRemove(string[] array, string value)
        {
            string[] newArray = new string[array.Length - 1];
            int index = stringArrayIndexOf(array, value);
            if (index == -1)
            {
                return array;
            }
            for (int i = 0; i < index; i++)
            {
                newArray[i] = array[i];
            }
            for (int i = index + 1; i < array.Length; i++)
            {
                newArray[i - 1] = array[i];
            }
            return newArray;
        }
        // int intArrayIndexOf(int[] array, int value)
        // {
        //     for (int i = 0; i < array.Length; i++)
        //     {
        //         if (array[i] == value)
        //         {
        //             return i;
        //         }
        //     }
        //     return -1;
        // }
        float[] floatArrayAddIndex(float[] array, float _number, int index)
        {
            float[] newArray = new float[array.Length + 1];
            for (int i = 0; i < index; i++)
            {
                newArray[i] = array[i];
            }
            newArray[index] = _number;
            for (int i = index + 1; i < array.Length; i++)
            {
                newArray[i] = array[i - 1];
            }
            return newArray;
        }
        // int[] intArrayAdd(int[] array, int _number)
        // {
        //     int[] newArray = new int[array.Length + 1];
        //     array.CopyTo(newArray, 0);
        //     newArray[array.Length] = _number;
        //     return newArray;
        // }
        // int[] intArrayRemove(int[] array, int _number)
        // {
        //     int[] newArray = new int[array.Length - 1];
        //     int index = intArrayIndexOf(array, _number);
        //     if (index == -1)
        //     {
        //         return array;
        //     }
        //     for (int i = 0; i < index; i++)
        //     {
        //         newArray[i] = array[i];
        //     }
        //     for (int i = index + 1; i < array.Length; i++)
        //     {
        //         newArray[i - 1] = array[i];
        //     }
        //     return newArray;
        // }
        // 在int数组中从0开始查找比number大的最小值
        int floatArrayFindMax(float[] array, float number)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] > number)
                {
                    return i;
                }
            }
            return 0;
        }
        /// <summary>
        /// 获取歌词文本
        /// </summary>
        /// <param name="_index"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public string GetLyricTextIndex(int _index, float time)
        {
            // if (musicLrcs.Length <= _index) return "";
            // return GetLyricText(musicLrcs[_index], time);
            if (lrcText.Length <= _index || lrcTime.Length <= _index || offsets.Length <= _index) return "";
            time = time + offsets[_index];
            var _lrcTime = lrcTime[_index];
            var _lrcText = lrcText[_index];
            for (int i = 0; i < _lrcTime.Length; i++)
            {
                // 当前时间大于等于当前行歌词时间，小于下一行歌词时间
                if (time >= _lrcTime[i] && (i == _lrcTime.Length - 1 || time < _lrcTime[i + 1]))
                {
                    return _lrcText[i];
                }
            }
            return "";
        }
        /// <summary>
        /// 获取歌词文本
        /// </summary>
        /// <param name="musicLrc"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        // public string GetLyricText(MusicLrc musicLrc, float time)
        // {
        //     // if (musicLrc.lrcTime.Length == 0 && musicLrc.lrcFile != null) ReadLrcFile(musicLrc);
        //     time = time + musicLrc.offset;
        //     for (int i = 0; i < musicLrc.lrcTime.Length; i++)
        //     {
        //         // 当前时间大于等于当前行歌词时间，小于下一行歌词时间
        //         if (time >= musicLrc.lrcTime[i] && (i == musicLrc.lrcTime.Length - 1 || time < musicLrc.lrcTime[i + 1]))
        //         {
        //             return musicLrc.lrcText[i];
        //         }
        //     }
        //     return "";
        // }
        /// <summary>
        /// 获取当前时间的上几句或下几句歌词
        /// </summary>
        /// <param name="musicLrc"></param>
        /// <param name="time"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        // public string GetLyricTextOffset(MusicLrc musicLrc, float time, int offset)
        public string GetLyricTextOffset(int index, float time, int offset)
        {
            // if (musicLrc.lrcTime.Length == 0 && musicLrc.lrcFile != null) ReadLrcFile(musicLrc);
            // time = time + musicLrc.offset;
            // for (int i = 0; i < musicLrc.lrcTime.Length; i++)
            // {
            //     // 当前时间大于等于当前行歌词时间，小于下一行歌词时间
            //     if (time >= musicLrc.lrcTime[i] && (i == musicLrc.lrcTime.Length - 1 || time < musicLrc.lrcTime[i + 1]))
            //     {
            //         if (i + offset >= 0 && i + offset < musicLrc.lrcTime.Length)
            //         {
            //             return musicLrc.lrcText[i + offset];
            //         }
            //         else
            //         {
            //             return "";
            //         }
            //     }
            // }
            time = time + offsets[index];
            var _lrcTime = lrcTime[index];
            var _lrcText = lrcText[index];
            for (int i = 0; i < _lrcTime.Length; i++)
            {
                // 当前时间大于等于当前行歌词时间，小于下一行歌词时间
                if (time >= _lrcTime[i] && (i == _lrcTime.Length - 1 || time < _lrcTime[i + 1]))
                {
                    if (i + offset >= 0 && i + offset < _lrcTime.Length)
                    {
                        return _lrcText[i + offset];
                    }
                    else
                    {
                        return "";
                    }
                }
            }
            return "";
        }
    }
}
