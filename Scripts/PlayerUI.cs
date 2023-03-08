
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonLab.Lyric
{
    public class PlayerUI : UdonSharpBehaviour
    {
        /// <summary>
        /// LyricPlayer
        /// </summary>
        [Header("LyricPlayer")]
        [SerializeField] private LyricPlayer lyricPlayer;
        /// <summary>
        /// 进度条
        /// </summary>
        [Header("进度条")]
        [SerializeField] private Slider musicSlider;
        /// <summary>
        /// 音量条
        /// </summary>
        [Header("音量条")]
        [SerializeField] private Slider volumeSlider;
        /// <summary>
        /// 播放时间
        /// </summary>
        [Header("播放时间")]
        [SerializeField] private Text playTimeText;
        /// <summary>
        /// 音乐名称
        /// </summary>
        [Header("音乐名称")]
        [SerializeField] private Text musicNameText;
        /// <summary>
        /// 播放模式
        /// </summary>
        [Header("播放模式")]
        [SerializeField] private Text modeText;
        /// <summary>
        /// 播放列表
        /// </summary>
        [Header("播放列表")]
        [SerializeField] private GameObject playList;
        /// <summary>
        /// 播放列表 item
        /// </summary>
        [Header("播放列表 item")]
        [SerializeField] private GameObject playListItem;
        void Start()
        {
            if (playList == null && playListItem == null && lyricPlayer == null)
            {
                Debug.LogError("playList or playListItem or lyricPlayer is null");
            }
            else
            {
                var musicLrcs = lyricPlayer.lyricReader._musicLrcs;
                for (int i = 0; i < musicLrcs.Length; i++)
                {
                    if (musicLrcs[i] == null) continue;
                    var item = Instantiate(playListItem, playList.transform);
                    var _musicName = musicLrcs[i].name;
                    if (!string.IsNullOrEmpty(musicLrcs[i].lyricInfo[0]))
                    {
                        _musicName = $"{musicLrcs[i].lyricInfo[0]}";
                        if (!string.IsNullOrEmpty(musicLrcs[i].lyricInfo[1]))
                        {
                            _musicName += $" - {musicLrcs[i].lyricInfo[1]}";
                        }
                    }
                    var _text = (Text)item.GetComponent(typeof(Text));
                    if (_text != null) _text.text = _musicName;
                    var _udonSharpBehaviour = (UdonSharpBehaviour)item.GetComponent(typeof(UdonSharpBehaviour));
                    if (_udonSharpBehaviour != null
                    && _udonSharpBehaviour.GetUdonTypeName() == "UdonLab.Toolkit.UdonInteractFunctionWithInt")
                    {
                        var _udonInteractFunctionWithInt = (UdonLab.Toolkit.UdonInteractFunctionWithInt)_udonSharpBehaviour;
                        _udonInteractFunctionWithInt.udonBehaviours = new UdonBehaviour[] { (UdonBehaviour)lyricPlayer.GetComponent(typeof(UdonBehaviour)) };
                        _udonInteractFunctionWithInt.functionName = "PlayInt";
                        _udonInteractFunctionWithInt.setIntValue = "PlayInt_int";
                        _udonInteractFunctionWithInt.value = i;
                    }
                    var _time = (Text)item.transform.Find("Time").GetComponent(typeof(Text));
                    if (_time != null) _time.text = $"{(int)musicLrcs[i].audioClip.length / 60:D2}:{(int)musicLrcs[i].audioClip.length % 60:D2}";
                    var index = i;
                }
                if (modeText != null)
                {
                    switch (lyricPlayer.playMode)
                    {
                        case 0:
                            modeText.text = "列表循环";
                            break;
                        case 1:
                            modeText.text = "单曲循环";
                            break;
                        case 2:
                            modeText.text = "随机播放";
                            break;
                        case 3:
                            modeText.text = "顺序播放";
                            break;
                        case 4:
                            modeText.text = "单曲播放";
                            break;
                    }
                }
            }
        }
        void Update()
        {
            if (musicSlider != null && lyricPlayer.audioSource != null)
            {
                musicSlider.value = lyricPlayer.audioSource.time;
            }
            if (playTimeText != null && lyricPlayer.audioSource != null)
            {
                // 分分:秒秒
                playTimeText.text = $"{(int)lyricPlayer.audioSource.time / 60:D2}:{(int)lyricPlayer.audioSource.time % 60:D2}";
            }
        }
        public void SetMusicInfo(MusicLrc currentMusic)
        {
            if (musicSlider != null && currentMusic != null) musicSlider.maxValue = currentMusic.audioClip.length;
            if (musicNameText != null)
            {
                var _name = $"Udon静听 {lyricPlayer.version}";
                if (currentMusic != null)
                {
                    if (!string.IsNullOrEmpty(currentMusic.lyricInfo[0]))
                    {
                        _name = $"{currentMusic.lyricInfo[0]}";
                        if (!string.IsNullOrEmpty(currentMusic.lyricInfo[1]))
                        {
                            _name += $" - {currentMusic.lyricInfo[1]}";
                        }
                    }
                    else
                    {
                        _name = currentMusic.name;
                    }
                }
                musicNameText.text = _name;
            }
        }
        public void SetVolume()
        {
            if (lyricPlayer.audioSource == null)
            {
                Debug.LogError("AudioSource is null");
                return;
            }
            lyricPlayer.audioSource.volume = volumeSlider.value;
        }
        public void SetMusic()
        {
            if (musicSlider != null && lyricPlayer.audioSource != null)
            {
                lyricPlayer.audioSource.time = musicSlider.value;
            }
        }
        public void SetPlayMode()
        {
            if (lyricPlayer == null)
            {
                Debug.LogError("LyricPlayer is null");
                return;
            }
            lyricPlayer.playMode++;
            if (lyricPlayer.playMode > 4) lyricPlayer.playMode = 0;
            if (modeText != null)
            {
                switch (lyricPlayer.playMode)
                {
                    case 0:
                        modeText.text = "列表循环";
                        break;
                    case 1:
                        modeText.text = "单曲循环";
                        break;
                    case 2:
                        modeText.text = "随机播放";
                        break;
                    case 3:
                        modeText.text = "顺序播放";
                        break;
                    case 4:
                        modeText.text = "单曲播放";
                        break;
                }
            }
        }
    }
}