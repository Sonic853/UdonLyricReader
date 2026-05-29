
using System;
using System.Text.RegularExpressions;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace Sonic853.Lyric
{
    public class LyricReader : UdonSharpBehaviour
    {
        static string PatternTime() => @"\[(?:(\d{1,}:)?\d{1,}(?:[.:]\d{1,})?)\]";
        static Regex RegexTime() => new Regex(PatternTime());
        static string PatternMeta() => @"^\[(\w+):(.*)\]$";
        static Regex RegexMeta() => new Regex(PatternMeta());
        [SerializeField] private MusicLrc[] musicLrcs;
        public MusicLrc[] MusicLrcs
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
                var musicLrc = musicLrcs[i];
                if (musicLrc.lrcText.Length == 0 || musicLrc.lrcTime.Length == 0 || musicLrc.lyricInfo.Length == 0)
                    ReadLrcFile(ref musicLrc);
            }
        }
        public static void ReadLrcFile(ref MusicLrc musicLrc)
        {
            var _lrcFile = musicLrc.lrcFile;
            var _lrcString = musicLrc.lrcString;
            if (_lrcFile == null && string.IsNullOrEmpty(_lrcString))
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
            if (string.IsNullOrEmpty(_lrcString)) _lrcString = _lrcFile.text;
            string[] lines = _lrcString.Split('\n');
            if (_lrcString.Contains("\r\n"))
            {
                lines = _lrcString.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            }
            var times = new DataList();
            var lyric = "";
            var regexTime = RegexTime();
            var regexMeta = RegexMeta();
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
                    var matches = regexTime.Matches(line);
                    if (matches.Count > 0)
                    {
                        times.Clear();
                        lyric = regexTime.Replace(line, "");
                        foreach (Match match in matches)
                        {
                            var timeStr = match.Groups[1].Value;
                            var time = stringTimeToFloat(timeStr);
                            if (time != -1)
                            {
                                times.Insert(floatArrayFindMax(times, time), time);
                            }
                        }
                    }
                    matches = regexMeta.Matches(line);
                    if (matches.Count > 0)
                    {
                        foreach (Match match in matches)
                        {
                            var key = match.Groups[1].Value.ToLower();
                            var value = match.Groups[2].Value;
                            switch (key)
                            {
                                case "ti":
                                    {
                                        _lyricInfo[0] = "歌曲：" + value;
                                    }
                                    break;
                                case "ar":
                                    {
                                        _lyricInfo[1] = "歌手：" + value;
                                    }
                                    break;
                                case "al":
                                    {
                                        _lyricInfo[2] = "专辑：" + value;
                                    }
                                    break;
                                case "au":
                                    {
                                        _lyricInfo[3] = "作词：" + value;
                                    }
                                    break;
                                case "by":
                                    {
                                        _lyricInfo[4] = "歌词：" + value;
                                    }
                                    break;
                                case "length":
                                    {
                                        _lyricInfo[5] = "时长：" + value;
                                    }
                                    break;
                                case "offset":
                                    {
                                        string offsetStr = match.Groups[3].Value;
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
                            }
                        }
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
            // [00:00]
            if (time.Length == 2)
            {
                int minute = int.Parse(time[0]);
                // [01:02.03]
                // [01:02.003]
                if (value.Contains("."))
                {
                    // 01
                    // 02.03
                    // 02.003
                    string[] _time = time[1].Split('.');
                    // 02
                    // 03
                    // 003
                    int second = int.Parse(_time[0]);
                    // _time[1] 补齐 3 位
                    int millisecond = int.Parse(_time[1].PadRight(3, '0'));
                    return minute * 60 + second + millisecond / 1000f;
                }
                // [00:00]
                else
                {
                    int second = int.Parse(time[1]);
                    // return minute * 60 * 1000 + second * 1000;
                    return minute * 60 + second;
                }
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
            // [0.000]
            // [0.00]
            // [0]
            else if (time.Length == 1)
            {
                string[] _time = time[1].Split('.');
                int second = int.Parse(_time[0]);
                // _time[1] 补齐 3 位
                int millisecond = int.Parse(_time.Length == 2 ? _time[1].PadRight(3, '0') : "000");
                return second + millisecond / 1000f;
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
            if (array.Length == 0) return 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] > number)
                {
                    return i;
                }
            }
            return array.Length - 1;
        }
        /// <summary>
        /// 在int数组中从0开始查找比number大的最小值
        /// </summary>
        /// <param name="array"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        static int floatArrayFindMax(DataList array, float number)
        {
            if (array.Count == 0) return 0;
            for (int i = 0; i < array.Count; i++)
            {
                var data = array[i].Float;
                if (data > number)
                {
                    return i;
                }
            }
            return array.Count - 1;
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
