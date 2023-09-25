using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UdonLab.QuickUIElement;
using UdonLab.EditorUI;
using System;

namespace UdonLab.Lyric
{
    [CustomEditor(typeof(LyricReader))]
    public class LyricReaderEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.AddToClassList("CustomEditor");
            UIElementMethod.InsertStyleSheet(ref root);
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
            // List<TextAsset> lrcFiles = SerializedObjectKit.GetSerializedUnityObjectList<TextAsset>(serializedObject, "lrcFiles", true);
            // List<AudioClip> audioClips = SerializedObjectKit.GetSerializedUnityObjectList<AudioClip>(serializedObject, "audioClips", true);
            // var offsets_obj = SerializedObjectKit.GetSerializedUnityObjectList(serializedObject, "offsets", true);
            // var offsets = new List<float>();
            // foreach (var offset_obj in offsets_obj)
            // {
            //     offsets.Add((float)offset_obj);
            // }
            // if (lrcFiles.Count > audioClips.Count)
            // {
            //     for (int i = audioClips.Count; i < lrcFiles.Count; i++)
            //     {
            //         audioClips.Add(null);
            //     }
            // }
            // else if (lrcFiles.Count < audioClips.Count)
            // {
            //     for (int i = lrcFiles.Count; i < audioClips.Count; i++)
            //     {
            //         lrcFiles.Add(null);
            //     }
            // }
            // if (lrcFiles.Count > offsets.Count)
            // {
            //     for (int i = offsets.Count; i < lrcFiles.Count; i++)
            //     {
            //         offsets.Add(0);
            //     }
            // }
            // else if (lrcFiles.Count < offsets.Count)
            // {
            //     for (int i = lrcFiles.Count; i < offsets.Count; i++)
            //     {
            //         offsets.RemoveAt(i);
            //     }
            // }
            var musicListFoldout = new Foldout()
            {
                // text = $"Music List ({lrcFiles.Count})",
                text = $"Music List ({musicLrcs.Count})",
                value = true,
            };
            root.Add(musicListFoldout);
            var musicListView = new ListView()
            {
                name = "musicList",
                makeItem = () => new VisualElement(),
                itemHeight = 135,
                // itemsSource = lrcFiles,
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
                    musicListView.Refresh();
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
                    // var _lrcFile = lrcFiles[index];
                    // var _audioClip = audioClips[index];
                    // var _offset = offsets[index];
                    // lrcFiles[index] = lrcFiles[index - 1];
                    // audioClips[index] = audioClips[index - 1];
                    // offsets[index] = offsets[index - 1];
                    // lrcFiles[index - 1] = _lrcFile;
                    // audioClips[index - 1] = _audioClip;
                    // offsets[index - 1] = _offset;
                    var _musicLrc = musicLrcs[index];
                    musicLrcs[index] = musicLrcs[index - 1];
                    musicLrcs[index - 1] = _musicLrc;
                    // SerializedObjectKit.SetSerializedObjectList(serializedObject, "lrcFiles", lrcFiles);
                    // SerializedObjectKit.SetSerializedObjectList(serializedObject, "audioClips", audioClips);
                    // SerializedObjectKit.SetSerializedObjectList(serializedObject, "offsets", offsets);
                    SerializedObjectKit.SetSerializedObjectList(serializedObject, "musicLrcs", musicLrcs);
                    ReadAllLrcFile(serializedObject);
                    sortMusicLrcs(musicLrcs);
                    musicListView.Refresh();
                })
                {
                    text = "↑",
                };
                upButton.style.flexGrow = 1;
                _ve.Add(upButton);
                var downButton = new Button(() =>
                {
                    // if (index == lrcFiles.Count - 1) return;
                    if (index == musicLrcs.Count - 1) return;
                    // var _lrcFile = lrcFiles[index];
                    // var _audioClip = audioClips[index];
                    // var _offset = offsets[index];
                    // lrcFiles[index] = lrcFiles[index + 1];
                    // audioClips[index] = audioClips[index + 1];
                    // offsets[index] = offsets[index + 1];
                    // lrcFiles[index + 1] = _lrcFile;
                    // audioClips[index + 1] = _audioClip;
                    // offsets[index + 1] = _offset;
                    var _musicLrc = musicLrcs[index];
                    musicLrcs[index] = musicLrcs[index + 1];
                    musicLrcs[index + 1] = _musicLrc;
                    // SerializedObjectKit.SetSerializedObjectList(serializedObject, "lrcFiles", lrcFiles);
                    // SerializedObjectKit.SetSerializedObjectList(serializedObject, "audioClips", audioClips);
                    // SerializedObjectKit.SetSerializedObjectList(serializedObject, "offsets", offsets);
                    SerializedObjectKit.SetSerializedObjectList(serializedObject, "musicLrcs", musicLrcs);
                    ReadAllLrcFile(serializedObject);
                    sortMusicLrcs(musicLrcs);
                    musicListView.Refresh();
                })
                {
                    text = "↓",
                };
                downButton.style.flexGrow = 1;
                _ve.Add(downButton);
                var removeButton = new Button(() =>
                {
                    // audioClips.RemoveAt(index);
                    // lrcFiles.RemoveAt(index);
                    // offsets.RemoveAt(index);
                    DestroyImmediate(musicLrcs[index].gameObject);
                    musicLrcs.RemoveAt(index);
                    // musicListFoldout.text = $"Music List ({lrcFiles.Count})";
                    musicListFoldout.text = $"Music List ({musicLrcs.Count})";
                    // musicListView.style.height = lrcFiles.Count * 135;
                    musicListView.style.height = musicLrcs.Count * 135;
                    // SerializedObjectKit.SetSerializedObjectList(serializedObject, "lrcFiles", lrcFiles);
                    // SerializedObjectKit.SetSerializedObjectList(serializedObject, "audioClips", audioClips);
                    // SerializedObjectKit.SetSerializedObjectList(serializedObject, "offsets", offsets);
                    SerializedObjectKit.SetSerializedObjectList(serializedObject, "musicLrcs", musicLrcs);
                    ReadAllLrcFile(serializedObject);
                    sortMusicLrcs(musicLrcs);
                    musicListView.Refresh();
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
                // SerializedObjectKit.SetSerializedObjectList(serializedObject, "lrcFiles", lrcFiles);
                // SerializedObjectKit.SetSerializedObjectList(serializedObject, "audioClips", audioClips);
                // SerializedObjectKit.SetSerializedObjectList(serializedObject, "offsets", offsets);
                SerializedObjectKit.SetSerializedObjectList(serializedObject, "musicLrcs", musicLrcs);
                musicListView.Refresh();
            })
            {
                text = "Add Music",
            };
            musicListFoldout.Add(addMusicButton);
            var refreshLrcButton = new Button(() =>
            {
                ReadAllLrcFile(serializedObject);
                musicListView.Refresh();
            })
            {
                text = "Refresh Lrc",
            };
            musicListFoldout.Add(refreshLrcButton);
            return root;
        }
        static void ReadAllLrcFile(SerializedObject serializedObject)
        {
            // var lrcFiles = SerializedObjectKit.GetSerializedUnityObjectList<TextAsset>(serializedObject, "lrcFiles");
            // var audioClips = SerializedObjectKit.GetSerializedUnityObjectList<AudioClip>(serializedObject, "audioClips");
            // var lrcTexts = new List<string[]>();
            // var lrcTimes = new List<float[]>();
            // var offsets_obj = SerializedObjectKit.GetSerializedUnityObjectList(serializedObject, "offsets");
            // var offsets = new List<float>();
            // foreach (var offset_obj in offsets_obj)
            // {
            //     offsets.Add((float)offset_obj);
            // }
            var musicLrcs = SerializedObjectKit.GetSerializedUnityObjectList<MusicLrc>(serializedObject, "musicLrcs");
            // for (int i = 0; i < lrcFiles.Count; i++)
            // {
            //     ReadLrcFile(lrcFiles[i], out var _lrcText, out var _lrcTime, out var _offset, out var _lyricInfo, out var _hasLyric);
            //     lrcTexts.Add(_lrcText.ToArray());
            //     lrcTimes.Add(_lrcTime.ToArray());
            //     offsets[i] = _hasLyric ? _offset : offsets[i];
            // }
            for (int i = 0; i < musicLrcs.Count; i++)
            {
                var musicLrc = musicLrcs[i];
                ReadLrcFile(musicLrc.lrcFile, out var _lrcText, out var _lrcTime, out var _offset, out var _lyricInfo, out var _hasLyric);
                musicLrc.lrcText = _lrcText.ToArray();
                musicLrc.lrcTime = _lrcTime.ToArray();
                musicLrc.offset = _hasLyric ? _offset : musicLrc.offset;
                for (int j = 0; j < _lyricInfo.Length; j++)
                {
                    // musicLrc.lyricInfo[j] = _lyricInfo[j];
                    if (string.IsNullOrEmpty(musicLrc.lyricInfo[j]))
                    {
                        musicLrc.lyricInfo[j] = _lyricInfo[j];
                    }
                }
            }
            // SerializedObjectKit.SetSerializedObjectList(serializedObject, "lrcTexts", lrcTexts);
            // SerializedObjectKit.SetSerializedObjectList(serializedObject, "lrcTimes", lrcTimes);
            // SerializedObjectKit.SetSerializedObjectList(serializedObject, "offsets", offsets);
        }
        static void ReadLrcFile(TextAsset _lrcFile, out List<string> _lrcText, out List<float> _lrcTime, out float _offset, out string[] _lyricInfo, out bool _hasLyric)
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
            if (_lrcFile == null)
            {
                return;
            }
            _hasLyric = true;
            string[] lines = _lrcFile.text.Split('\n');
            if (_lrcFile.text.Contains("\r\n"))
            {
                lines = _lrcFile.text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
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
                    if (line.Length > 1 && char.IsDigit(line[1]))
                    {
                        // 分离时间和歌词
                        // [00:00.00]歌词
                        // [00:00]歌词
                        // [00:00:00]歌词
                        // [00:00.00][00:00.00]歌词
                        // 一行歌词可能有多个时间
                        string[] timeAndLyric = line.Split(']');
                        // [00:00.00 歌词
                        // [00:00 歌词
                        // [00:00:00 歌词
                        // [00:00.00 [00:00.00 歌词
                        times.Clear();
                        lyric = "";
                        for (int j = 0; j < timeAndLyric.Length; j++)
                        {
                            if (timeAndLyric[j].Trim().StartsWith("["))
                            // [00:00.00
                            // [00:00
                            // [00:00:00
                            // [00:00.0 [00:00.00
                            {
                                float time = stringTimeToFloat(timeAndLyric[j].Trim().Substring(1));
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
        // 重新排序 musicLrcs 的层级
        static void sortMusicLrcs(List<MusicLrc> musicLrcs)
        {
            for (int i = 0; i < musicLrcs.Count; i++)
            {
                musicLrcs[i].transform.SetSiblingIndex(i);
            }
        }
    }
}