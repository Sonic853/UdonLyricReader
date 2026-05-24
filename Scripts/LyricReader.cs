
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonLab.Lyric
{
    public class LyricReader : UdonSharpBehaviour
    {
        [SerializeField] private MusicLrc[] musicLrcs;
        public MusicLrc[] _musicLrcs
        {
            get
            {
                return musicLrcs;
            }
        }
        void Start()
        {
            for (int i = 0; i < musicLrcs.Length; i++)
            {
                if (musicLrcs[i].lrcText.Length == 0 || musicLrcs[i].lrcTime.Length == 0 || musicLrcs[i].lyricInfo.Length == 0)
                    ReadLrcFile(ref musicLrcs[i]);
            }
        }
        public static void ReadLrcFile(ref MusicLrc musicLrc)
        {
            var _lrcFile = musicLrc.lrcFile;
            if (_lrcFile == null)
                return;
            var _lrcTime = new DataList();
            var _lrcText = new DataList();
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
            var times = new DataList();
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
                    for (int j = 0; j < times.Count; j++)
                    {
                        var time = times[j].Float;
                        int _index = floatArrayFindMax(_lrcTime, time);
                        // _lrcTime = floatArrayAddIndex(_lrcTime, time, _index);
                        _lrcTime.Insert(_index, time);
                        // _lrcText = stringArrayAddIndex(_lrcText, lyric, _index);
                        _lrcText.Insert(_index, lyric);
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
                        times = new DataList();
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
                                    // times = floatArrayAddIndex(times, time, floatArrayFindMax(times, time));
                                    times.Insert(floatArrayFindMax(times, time), time);
                                }
                            }
                            else
                            // 歌词
                            {
                                lyric += timeAndLyric[j].Trim();
                            }
                            if (times.Count == 1 && timeAndLyric.Length == 1)
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
                    if (times.Count > 0 && lines[i] != "")
                    {
                        lyric += "\n" + line;
                    }
                }
            }
            for (int j = 0; j < times.Count; j++)
            {
                var time = times[j].Float;
                int _index = floatArrayFindMax(_lrcTime, time);
                // _lrcTime = floatArrayAddIndex(_lrcTime, times[j], _index);
                _lrcTime.Insert(_index, time);
                // _lrcText = stringArrayAddIndex(_lrcText, lyric, _index);
                _lrcText.Insert(_index, lyric);
            }
            // lrcTime[index] = _lrcTime;
            // lrcText[index] = _lrcText;
            // offsets[index] = _offset;
            // lyricInfo[index] = _lyricInfo;
            // 歌词时长
            var _lineTime = new DataList();
            for (int i = 0; i < _lrcTime.Count; i++)
            {
                if (1 + i >= _lrcTime.Count)
                {
                    _lineTime.Add(float.MaxValue);
                    break;
                }
                _lineTime.Add(_lrcTime[i + 1].Float - _lrcTime[i].Float);
            }
            musicLrc.lrcTime = DataListToArrayFloat(_lrcTime);
            musicLrc.lineTime = DataListToArrayFloat(_lineTime);
            musicLrc.lrcText = DataListToArrayString(_lrcText);
            musicLrc.offset = _offset;
            musicLrc.lyricInfo = _lyricInfo;
        }
        static float stringTimeToFloat(string value)
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
        static int stringArrayIndexOf(string[] array, string value)
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
        static string[] stringArrayAdd(string[] array, string value)
        {
            string[] newArray = new string[array.Length + 1];
            array.CopyTo(newArray, 0);
            newArray[array.Length] = value;
            return newArray;
        }
        static string[] stringArrayAddIndex(string[] array, string value, int index)
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
        static string[] stringArrayRemove(string[] array, string value)
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
        static float[] floatArrayAddIndex(float[] array, float _number, int index)
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
        /// <summary>
        /// 在int数组中从0开始查找比number大的最小值
        /// </summary>
        /// <param name="array"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        static int floatArrayFindMax(float[] array, float number)
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
        /// 在int数组中从0开始查找比number大的最小值
        /// </summary>
        /// <param name="array"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        static int floatArrayFindMax(DataList array, float number)
        {
            for (int i = 0; i < array.Count; i++)
            {
                var data = array[i].Float;
                if (data > number)
                {
                    return i;
                }
            }
            return 0;
        }
        static float[] DataListToArrayFloat(DataList array)
        {
            float[] newArray = new float[array.Count];
            for (int i = 0; i < array.Count; i++)
            {
                newArray[i] = array[i].Float;
            }
            return newArray;
        }
        static string[] DataListToArrayString(DataList array)
        {
            string[] newArray = new string[array.Count];
            for (int i = 0; i < array.Count; i++)
            {
                newArray[i] = array[i].String;
            }
            return newArray;
        }
        /// <summary>
        /// 获取歌词文本
        /// </summary>
        /// <param name="_index"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public string GetLyricTextIndex(int _index, float time)
        {
            if (musicLrcs.Length <= _index) return "";
            return musicLrcs[_index].GetLyricText(time);
        }
    }
}
