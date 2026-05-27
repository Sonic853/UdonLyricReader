using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Text.RegularExpressions;

namespace Sonic853.Lyric.Editors
{
    [CustomEditor(typeof(LyricReader))]
    public class LyricReaderEditor : Editor
    {
        static readonly string patternTime = @"\[(\d{2}:\d{2}(?:[.:]\d{2,3})?)\]";
        static readonly Regex regexTime = new(patternTime);
        static readonly string patternMeta = @"^\[(\w+):(.*)\]$";
        static readonly Regex regexMeta = new(patternMeta);
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.AddToClassList("CustomEditor");
            InsertStyleSheet(ref root, "Assets/Sonic853/Udon Lab/LyricReader/Scripts/Editor/LyricReaderEditor.uss");
            root.Bind(serializedObject);
            var container = new IMGUIContainer(() =>
            {
                UdonSharpEditor.UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target, true, false);
            })
            {
                name = "UdonSharpGUI"
            };
            root.Add(container);
            var lyricReader = (LyricReader)target;
            var musicLrcs = SerializedObjectKit.GetSerializedUnityObjectList<MusicLrc>(serializedObject, "musicLrcs", false);
            var musicListFoldout = new Foldout()
            {
                text = $"Music List ({musicLrcs.Count})",
                value = true,
            };
            root.Add(musicListFoldout);
            var musicListView = new ListView()
            {
                name = "musicList",
                makeItem = () => new VisualElement(),
                fixedItemHeight = 140,
                itemsSource = musicLrcs,
                selectionType = SelectionType.None,
            };
            musicListView.bindItem = (element, index) =>
            {
                var ve = (VisualElement)element;
                ve.Clear();
                var audioClipField = new ObjectField()
                {
                    name = "audioClip",
                    objectType = typeof(AudioClip),
                    // value = audioClips[index],
                    value = musicLrcs[index].audioClip,
                    label = "Audio Clip",
                };
                audioClipField.RegisterValueChangedCallback((e) =>
                {
                    // audioClips[index] = (AudioClip)e.newValue;
                    musicLrcs[index].audioClip = (AudioClip)e.newValue;
                    if (e.newValue != null) musicLrcs[index].name = e.newValue.name;
                    // SerializedObjectKit.SetSerializedObjectList(serializedObject, "audioClips", audioClips);
                    SerializedObjectKit.SetSerializedObjectList(serializedObject, "musicLrcs", musicLrcs);
                });
                ve.Add(audioClipField);
                var lrcFileField = new ObjectField()
                {
                    name = "lrcFile",
                    objectType = typeof(TextAsset),
                    // value = lrcFiles[index],
                    value = musicLrcs[index].lrcFile,
                    label = "LRC File",
                };
                lrcFileField.RegisterValueChangedCallback((e) =>
                {
                    // lrcFiles[index] = (TextAsset)e.newValue;
                    musicLrcs[index].lrcFile = (TextAsset)e.newValue;
                    // SerializedObjectKit.SetSerializedObjectList(serializedObject, "lrcFiles", lrcFiles);
                    SerializedObjectKit.SetSerializedObjectList(serializedObject, "musicLrcs", musicLrcs);
                    ReadAllLrcFile(serializedObject);
                    musicListView.Rebuild();
                });
                ve.Add(lrcFileField);
                var titleField = new TextField()
                {
                    name = "title",
                    value = musicLrcs[index].lyricInfo[0],
                    label = "Title",
                };
                titleField.RegisterValueChangedCallback((e) =>
                {
                    musicLrcs[index].lyricInfo[0] = e.newValue;
                    SerializedObjectKit.SetSerializedObjectList(serializedObject, "musicLrcs", musicLrcs);
                });
                ve.Add(titleField);
                var artistField = new TextField()
                {
                    name = "artist",
                    value = musicLrcs[index].lyricInfo[1],
                    label = "Artist",
                };
                artistField.RegisterValueChangedCallback((e) =>
                {
                    musicLrcs[index].lyricInfo[1] = e.newValue;
                    SerializedObjectKit.SetSerializedObjectList(serializedObject, "musicLrcs", musicLrcs);
                });
                ve.Add(artistField);
                var albumField = new TextField()
                {
                    name = "album",
                    value = musicLrcs[index].lyricInfo[2],
                    label = "Album",
                };
                albumField.RegisterValueChangedCallback((e) =>
                {
                    musicLrcs[index].lyricInfo[2] = e.newValue;
                    SerializedObjectKit.SetSerializedObjectList(serializedObject, "musicLrcs", musicLrcs);
                });
                ve.Add(albumField);
                var offsetField = new FloatField()
                {
                    name = "offset",
                    // value = offsets[index],
                    value = musicLrcs[index].offset,
                    label = "Offset",
                };
                offsetField.RegisterValueChangedCallback((e) =>
                {
                    // Debug.Log($"offset: {e.newValue}");
                    // offsets[index] = e.newValue;
                    musicLrcs[index].offset = e.newValue;
                    // SerializedObjectKit.SetSerializedObjectList(serializedObject, "offsets", offsets);
                    SerializedObjectKit.SetSerializedObjectList(serializedObject, "musicLrcs", musicLrcs);
                });
                ve.Add(offsetField);
                var _ve = new VisualElement();
                _ve.style.flexDirection = FlexDirection.Row;
                var upButton = new Button(() =>
                {
                    if (index == 0) return;
                    (musicLrcs[index - 1], musicLrcs[index]) = (musicLrcs[index], musicLrcs[index - 1]);
                    SerializedObjectKit.SetSerializedObjectList(serializedObject, "musicLrcs", musicLrcs);
                    ReadAllLrcFile(serializedObject);
                    SortMusicLrcs(musicLrcs);
                    musicListView.Rebuild();
                })
                {
                    text = "↑",
                };
                upButton.style.flexGrow = 1;
                _ve.Add(upButton);
                var downButton = new Button(() =>
                {
                    if (index == musicLrcs.Count - 1) return;
                    (musicLrcs[index + 1], musicLrcs[index]) = (musicLrcs[index], musicLrcs[index + 1]);
                    SerializedObjectKit.SetSerializedObjectList(serializedObject, "musicLrcs", musicLrcs);
                    ReadAllLrcFile(serializedObject);
                    SortMusicLrcs(musicLrcs);
                    musicListView.Rebuild();
                })
                {
                    text = "↓",
                };
                downButton.style.flexGrow = 1;
                _ve.Add(downButton);
                var removeButton = new Button(() =>
                {
                    DestroyImmediate(musicLrcs[index].gameObject);
                    musicLrcs.RemoveAt(index);
                    musicListFoldout.text = $"Music List ({musicLrcs.Count})";
                    musicListView.style.height = musicLrcs.Count * 135;
                    musicListView.style.display = musicLrcs.Count == 0 ? DisplayStyle.None : DisplayStyle.Flex;
                    SerializedObjectKit.SetSerializedObjectList(serializedObject, "musicLrcs", musicLrcs);
                    ReadAllLrcFile(serializedObject);
                    SortMusicLrcs(musicLrcs);
                    musicListView.Rebuild();
                })
                {
                    text = "Remove",
                };
                removeButton.style.flexGrow = 1;
                _ve.Add(removeButton);
                ve.Add(_ve);
            };
            // musicListView.style.height = lrcFiles.Count * 135;
            musicListView.style.height = musicLrcs.Count * 135;
            musicListView.style.display = musicLrcs.Count == 0 ? DisplayStyle.None : DisplayStyle.Flex;
            musicListFoldout.Add(musicListView);
            var addMusicButton = new Button(() =>
            {
                // audioClips.Add(null);
                // lrcFiles.Add(null);
                // offsets.Add(0f);
                var musicLrcObj = new GameObject($"MusicLrc ({musicLrcs.Count})");
                var musicLrc = musicLrcObj.AddComponent<MusicLrc>();
                musicLrcObj.transform.SetParent(lyricReader.transform);
                musicLrcObj.transform.localPosition = Vector3.zero;
                musicLrcObj.transform.localRotation = Quaternion.identity;
                musicLrcObj.transform.localScale = Vector3.one;
                musicLrcs.Add(musicLrc);
                // musicListFoldout.text = $"Music List ({lrcFiles.Count})";
                // musicListView.style.height = lrcFiles.Count * 135;
                musicListFoldout.text = $"Music List ({musicLrcs.Count})";
                musicListView.style.height = musicLrcs.Count * 135;
                musicListView.style.display = musicLrcs.Count == 0 ? DisplayStyle.None : DisplayStyle.Flex;
                // SerializedObjectKit.SetSerializedObjectList(serializedObject, "lrcFiles", lrcFiles);
                // SerializedObjectKit.SetSerializedObjectList(serializedObject, "audioClips", audioClips);
                // SerializedObjectKit.SetSerializedObjectList(serializedObject, "offsets", offsets);
                SerializedObjectKit.SetSerializedObjectList(serializedObject, "musicLrcs", musicLrcs);
                musicListView.Rebuild();
            })
            {
                text = "Add Music",
            };
            musicListFoldout.Add(addMusicButton);
            var refreshLrcButton = new Button(() =>
            {
                ReadAllLrcFile(serializedObject);
                musicListView.Rebuild();
            })
            {
                text = "Refresh Lrc",
            };
            musicListFoldout.Add(refreshLrcButton);
            return root;
        }
        static void ReadAllLrcFile(SerializedObject serializedObject)
        {
            var musicLrcs = SerializedObjectKit.GetSerializedUnityObjectList<MusicLrc>(serializedObject, "musicLrcs");
            for (int i = 0; i < musicLrcs.Count; i++)
            {
                var musicLrc = musicLrcs[i];
                var _lrcFile = musicLrc.lrcFile;
                var _lrcString = musicLrc.lrcString;
                if (string.IsNullOrEmpty(_lrcString) && _lrcFile != null) _lrcString = _lrcFile.text;
                ReadLrcFile(_lrcString, out var _lrcText, out var _lrcTime, out var _offset, out var _lyricInfo, out var _hasLyric);
                var _lineTime = new List<float>();
                for (int j = 0; j < _lrcTime.Count; j++)
                {
                    if (1 + j >= _lrcTime.Count)
                    {
                        _lineTime.Add(float.MaxValue);
                        break;
                    }
                    _lineTime.Add(_lrcTime[j + 1] - _lrcTime[j]);
                }
                musicLrc.lrcText = _lrcText.ToArray();
                musicLrc.lrcTime = _lrcTime.ToArray();
                musicLrc.lineTime = _lineTime.ToArray();
                musicLrc.offset = _hasLyric ? _offset : musicLrc.offset;
                for (int j = 0; j < _lyricInfo.Length; j++)
                {
                    if (string.IsNullOrEmpty(musicLrc.lyricInfo[j]))
                    {
                        musicLrc.lyricInfo[j] = _lyricInfo[j];
                    }
                }
            }
        }
        static void ReadLrcFile(string _lrcString, out List<string> _lrcText, out List<float> _lrcTime, out float _offset, out string[] _lyricInfo, out bool _hasLyric)
        {
            _hasLyric = false;
            _lrcText = new List<string>();
            _lrcTime = new List<float>();
            _offset = 0f;
            _lyricInfo = new string[] {
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
            if (string.IsNullOrEmpty(_lrcString))
            {
                return;
            }
            _hasLyric = true;
            string[] lines = _lrcString.Split('\n');
            if (_lrcString.Contains("\r\n"))
            {
                lines = _lrcString.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            }
            var times = new List<float>();
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
                        int _index = _lrcTime.FindIndex((x) => x > times[j]);
                        if (_index == -1)
                        {
                            _lrcTime.Add(times[j]);
                            _lrcText.Add(lyric);
                        }
                        else
                        {
                            _lrcTime.Insert(_index, times[j]);
                            _lrcText.Insert(_index, lyric);
                        }
                    }
                    var matches = regexTime.Matches(line);
                    if (matches.Count > 0)
                    {
                        times.Clear();
                        lyric = regexTime.Replace(line, "");
                        foreach (Match match in matches)
                        {
                            var timeStr = match.Groups[1].Value;
                            var time = StringTimeToFloat(timeStr);
                            if (time != -1)
                            {
                                int _index = times.FindIndex((x) => x > time);
                                if (_index == -1)
                                {
                                    times.Add(time);
                                }
                                else
                                {
                                    times.Insert(_index, time);
                                }
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
                                            if (float.TryParse(offsetStr[1..], out float offset))
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
                int _index = _lrcTime.FindIndex((x) => x > times[j]);
                if (_index == -1)
                {
                    _lrcTime.Add(times[j]);
                    _lrcText.Add(lyric);
                }
                else
                {
                    _lrcTime.Insert(_index, times[j]);
                    _lrcText.Insert(_index, lyric);
                }
            }
        }
        static float StringTimeToFloat(string value)
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
        // 重新排序 musicLrcs 的层级
        static void SortMusicLrcs(List<MusicLrc> musicLrcs)
        {
            for (int i = 0; i < musicLrcs.Count; i++)
            {
                musicLrcs[i].transform.SetSiblingIndex(i);
            }
        }
        static void InsertStyleSheet(ref VisualElement root, string s_StyleSheetPath)
        {
            root.styleSheets.Add(EditorGUIUtility.Load(s_StyleSheetPath) as StyleSheet);
        }
    }
}