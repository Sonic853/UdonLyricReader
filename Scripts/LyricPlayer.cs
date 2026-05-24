
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonLab.Lyric
{
    public class LyricPlayer : UdonSharpBehaviour
    {
        /// <summary>
        /// 版本号
        /// </summary>
        [Header("版本号")]
        [NonSerialized] public string version = "0.2.0";
        /// <summary>
        /// LyricReader
        /// </summary>
        [Header("LyricReader")]
        [SerializeField] private LyricReader _lyricReader;
        public LyricReader lyricReader
        {
            get
            {
                return _lyricReader;
            }
        }
        /// <summary>
        /// 音源
        /// </summary>
        [Header("音源")]
        [SerializeField] private AudioSource _audioSource;
        public AudioSource audioSource
        {
            get
            {
                return _audioSource;
            }
        }
        /// <summary>
        /// 显示歌词的Text
        /// </summary>
        [Header("显示歌词的Text")]
        [SerializeField] private Text[] lyricTexts;
        /// <summary>
        /// 显示歌词的 Text 索引，-1 表示未初始化，-1 以外表示当前显示的 Text 索引
        /// </summary>
        [NonSerialized] private int lyricTextIndex = -1;
        // /// <summary>
        // /// 默认显示歌词的 Text 索引，-1 表示未初始化，-1 以外表示当前显示的 Text 索引
        // /// </summary>
        // [NonSerialized] private int defaultLyricTextIndex = -1;
        /// <summary>
        /// 切换歌词的 Text 索引，-1 表示未初始化，-1 以外表示当前的 Text 索引
        /// </summary>
        [NonSerialized] private int switchLyricTextIndex = -1;
        // /// <summary>
        // /// 默认切换歌词的 Text 索引，-1 表示未初始化，-1 以外表示当前的 Text 索引
        // /// </summary>
        // [NonSerialized] private int defaultSwitchLyricTextIndex = -1;
        /// <summary>
        /// 歌词动画器
        /// </summary>
        [Header("歌词动画器")]
        [SerializeField] private Animator lyricAnimator;
        /// <summary>
        /// 动画过渡时间
        /// </summary>
        [NonSerialized] private float AnimatorTransitionTime = 0f;
        /// <summary>
        /// 自动播放
        /// </summary>
        [Header("自动播放")]
        [SerializeField] private bool autoPlay = true;
        /// <summary>
        /// 播放模式：0-列表循环，1-单曲循环，2-随机播放，3-顺序播放，4-单曲播放
        /// </summary>
        [Header("播放模式：0-列表循环，1-单曲循环，2-随机播放，3-顺序播放，4-单曲播放")]
        [Range(0, 3)]
        [SerializeField] public int playMode = 0;
        /// <summary>
        /// 当前音乐索引，在下一帧时为上一条歌词的索引
        /// </summary>
        [NonSerialized] private int currentMusicIndex = -1;
        /// <summary>
        /// 当前音乐索引
        /// </summary>
        [NonSerialized] private MusicLrc _currentMusic = null;
        public MusicLrc currentMusic
        {
            get
            {
                return _currentMusic;
            }
        }
        /// <summary>
        /// 是否正在播放
        /// </summary>
        [NonSerialized] private bool isPlaying = false;
        /// <summary>
        /// 是否暂停
        /// </summary>
        [NonSerialized] private bool isPause = false;
        // /// <summary>
        // /// 当前歌词文本
        // /// </summary>
        // [NonSerialized] private string currentLrcText = null;
        /// <summary>
        /// 准备切换歌词索引
        /// </summary>
        [NonSerialized] private int readyScrollLrcIndex = -1;
        /// <summary>
        /// 当前歌词索引
        /// </summary>
        [NonSerialized] private int currentLrcIndex = -1;
        /// <summary>
        /// 播放器UI
        /// </summary>
        [Header("播放器UI")]
        [SerializeField] private PlayerUI playerUI;

        void Start()
        {
            if (lyricReader == null)
            {
                Debug.LogError("LyricReader is null");
                return;
            }
            if (audioSource == null)
            {
                Debug.LogError("AudioSource is null");
                return;
            }
            if (lyricTexts.Length == 0)
            {
                Debug.LogError("LyricTexts is empty");
            }
            else
            {
                var lyricTextsLength = lyricTexts.Length;
                for (int i = 0; i < lyricTextsLength; i++)
                {
                    if (lyricTexts[i] == null)
                    {
                        Debug.LogError("LyricTexts[" + i + "] is null");
                        return;
                    }
                }
                // lyricTextIndex = defaultLyricTextIndex = (lyricTextsLength - 1) / 2;
                // switchLyricTextIndex = defaultSwitchLyricTextIndex = lyricTextsLength - 1;
                lyricTextIndex = (lyricTextsLength - 1) / 2;
                switchLyricTextIndex = lyricTextsLength - 1;
                Debug.Log($"lyricTextIndex: {lyricTextIndex}");
            }
            if (lyricAnimator == null)
            {
                Debug.LogError("LyricAnimator is null");
            }
            else
            {
                // AnimatorTransitionTime = lyricAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
                foreach (var clip in lyricAnimator.runtimeAnimatorController.animationClips)
                {
                    if (clip.length > AnimatorTransitionTime)
                    {
                        AnimatorTransitionTime = clip.length;
                    }
                }
                Debug.Log($"AnimatorTransitionTime: {AnimatorTransitionTime}");
            }
            if (autoPlay)
            {
                Play();
            }
        }
        void Update()
        {
            if (!isPlaying || currentMusicIndex == -1 || currentMusicIndex >= lyricReader._musicLrcs.Length) return;
            var audioSourceTime = audioSource.time;
            if (audioSourceTime >= lyricReader._musicLrcs[currentMusicIndex].audioClip.length - 0.1f)
            {
                switch (playMode)
                {
                    case 0:
                    case 3:
                        {
                            isPlaying = false;
                            currentMusicIndex++;
                            Play();
                        }
                        break;
                    case 2:
                        {
                            isPlaying = false;
                            Play();
                        }
                        break;
                    case 4:
                        {
                            isPlaying = false;
                            audioSourceTime = audioSource.time = 0;
                            audioSource.Stop();
                        }
                        break;
                    case 1:
                        {
                            audioSourceTime = audioSource.time = 0;
                            audioSource.Play();
                        }
                        break;
                }
            }
            else
            {
                // 下一句歌词
                var _readyScrollLrcIndex = currentMusic.GetLyricIndex((float)Math.Round(audioSourceTime + AnimatorTransitionTime, 3));
                // var currentMusicLineTimeLength = currentMusic.lineTime.Length;
                // 下一句歌词的显示时间
                // var _readyScrollLrcTime = (_readyScrollLrcIndex < 0 || _readyScrollLrcIndex >= currentMusicLineTimeLength) ? float.MaxValue : currentMusic.lineTime[_readyScrollLrcIndex];
                // 当前歌词
                var _currentLrcIndex = currentMusic.GetLyricIndex((float)Math.Round(audioSourceTime, 3));
                // 当前歌词的显示时间
                // var _currentLrcTime = (_currentLrcIndex < 0 || _currentLrcIndex >= currentMusicLineTimeLength) ? float.MaxValue : currentMusic.lineTime[_currentLrcIndex];
                var _lyricText = (_currentLrcIndex < 0 || _currentLrcIndex >= currentMusic.lrcText.Length) ? "" : currentMusic.lrcText[_currentLrcIndex];
                if (lyricAnimator != null && lyricAnimator.gameObject.activeInHierarchy)
                {
                    if (_currentLrcIndex >= 0 && currentLrcIndex != _currentLrcIndex)
                    {
                        lyricAnimator.SetBool("Scroll", false);
                        lyricAnimator.Play("Wait");
                        for (int i = 0; i < lyricTexts.Length; i++)
                        {
                            if (i == lyricTextIndex) continue;
                            var _index = _currentLrcIndex + (i - lyricTextIndex);
                            lyricTexts[i].text = _index >= 0 && _index < currentMusic.lrcText.Length ? currentMusic.lrcText[_index] : "";
                        }
                        lyricTexts[lyricTextIndex].text = _lyricText;
                        Debug.Log($"LateUpdate Switch: {lyricTexts[lyricTextIndex].text}");
                    }
                    if (!lyricAnimator.GetBool("Scroll") && _readyScrollLrcIndex >= 0 && readyScrollLrcIndex != _readyScrollLrcIndex && ((currentLrcIndex < 0 || currentLrcIndex >= currentMusic.lrcText.Length) ? float.MaxValue : currentMusic.lineTime[currentLrcIndex]) >= AnimatorTransitionTime)
                    {
                        lyricAnimator.SetBool("Scroll", true);
                        lyricAnimator.Play("Scroll", 0, 0);
                    }
                    if (_readyScrollLrcIndex >= 0) readyScrollLrcIndex = _readyScrollLrcIndex;
                    if (_currentLrcIndex >= 0) currentLrcIndex = _currentLrcIndex;
                }
                else
                {
                    for (int i = 0; i < lyricTexts.Length; i++)
                    {
                        var _index = _currentLrcIndex + (i - lyricTextIndex);
                        lyricTexts[i].text = _index >= 0 && _index < currentMusic.lrcText.Length ? currentMusic.lrcText[_index] : "";
                    }
                }
            }
        }
        [NonSerialized] public int PlayInt_int = -1;
        public void PlayInt()
        {
            if (PlayInt_int < 0 || PlayInt_int >= lyricReader._musicLrcs.Length)
            {
                return;
            }
            currentMusicIndex = PlayInt_int;
            _Play();
        }
        int _Play()
        {
            _currentMusic = lyricReader._musicLrcs[currentMusicIndex];
            if (currentMusic == null)
            {
                Debug.LogError("Music is null");
                return -1;
            }
            audioSource.clip = currentMusic.audioClip;
            audioSource.time = 0;
            audioSource.Play();
            isPlaying = true;
            // lyricTextIndex = defaultLyricTextIndex;
            if (lyricAnimator != null)
            {
                lyricAnimator.SetBool("Scroll", false);
                // lyricAnimator.Play(lyricAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name, 0, 0);
            }
            // switchLyricTextIndex = defaultSwitchLyricTextIndex;
            for (int i = 0; i < lyricTexts.Length; i++)
            {
                lyricTexts[i].text = currentMusic.GetLyricTextOffset(0, i - lyricTextIndex);
            }
            // currentLrcText = lyricReader.GetLyricTextOffset(currentMusic, 0, 0);
            readyScrollLrcIndex = currentMusic.GetLyricIndex(AnimatorTransitionTime);
            currentLrcIndex = currentMusic.GetLyricIndex(0);
            // switchLrcText = lyricReader.GetLyricTextOffset(music, AnimatorTransitionTime, defaultSwitchLyricTextIndex - defaultLyricTextIndex);
            // Debug.Log($"Play: {switchLrcText}");
            if (playerUI != null)
            {
                playerUI.SetMusicInfo(currentMusic);
            }
            return currentMusicIndex;
        }
        public int Play()
        {
            if (isPause)
            {
                audioSource.UnPause();
                isPause = false;
                isPlaying = true;
                return currentMusicIndex;
            }
            if (isPlaying) return currentMusicIndex;
            if (lyricReader == null)
            {
                Debug.LogError("LyricReader is null");
                return -1;
            }
            if (audioSource == null)
            {
                Debug.LogError("AudioSource is null");
                return -1;
            }
            if (currentMusicIndex < 0 || currentMusicIndex >= lyricReader._musicLrcs.Length)
            {
                switch (playMode)
                {
                    case 0:
                    case 1:
                    case 4:
                        {
                            currentMusicIndex = 0;
                        }
                        break;
                    case 2:
                        {
                            var _currentMusicIndex = UnityEngine.Random.Range(0, lyricReader._musicLrcs.Length);
                            while (lyricReader._musicLrcs.Length > 1 && currentMusicIndex == _currentMusicIndex)
                            {
                                _currentMusicIndex = UnityEngine.Random.Range(0, lyricReader._musicLrcs.Length);
                            }
                            currentMusicIndex = _currentMusicIndex;
                        }
                        break;
                    case 3:
                        {
                            if (currentMusicIndex != -1)
                            {
                                currentMusicIndex = -1;
                                audioSource.time = 0;
                                audioSource.Stop();
                                isPlaying = false;
                                return currentMusicIndex;
                            }
                            currentMusicIndex = 0;
                        }
                        break;
                    default:
                        currentMusicIndex = 0;
                        break;
                }
            }
            else
            {
                if (playMode == 2)
                {
                    currentMusicIndex = UnityEngine.Random.Range(0, lyricReader._musicLrcs.Length);
                }
            }
            return _Play();
        }
        public void PlayOrPause()
        {
            if (isPlaying)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }
        public void Pause()
        {
            isPlaying = false;
            isPause = true;
            if (audioSource != null)
            {
                audioSource.Pause();
            }
        }
        public void WaitLyricAnimation()
        {
            lyricAnimator.SetBool("Scroll", false);
            lyricAnimator.Play("Wait");
        }
        public void Stop()
        {
            isPlaying = false;
            isPause = false;
            lyricAnimator.Play("Wait");
            if (audioSource != null)
            {
                audioSource.Stop();
            }
            if (playerUI != null) playerUI.SetMusicInfo(null);
            for (int i = 0; i < lyricTexts.Length; i++)
            {
                if (lyricTextIndex != -1 && i == lyricTextIndex) lyricTexts[i].text = $"Udon静听 {version}";
                else lyricTexts[i].text = "";
            }
        }
        public void Next()
        {
            if (isPlaying)
            {
                Stop();
            }
            if (playMode != 2) currentMusicIndex++;
            if (currentMusicIndex >= lyricReader._musicLrcs.Length)
            {
                currentMusicIndex = 0;
            }
            Play();
        }
        public void Prev()
        {
            if (isPlaying)
            {
                Stop();
            }
            if (playMode != 2) currentMusicIndex--;
            if (currentMusicIndex < 0)
            {
                currentMusicIndex = lyricReader._musicLrcs.Length - 1;
            }
            Play();
        }
        public override void OnVideoPlay() => Stop();
    }
}
