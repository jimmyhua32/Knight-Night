using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

// This class should be placed immidiately below the sprite renderer in the inspector
public class Spritimation : MonoBehaviour {

    public string spriteSheetFolderPath;
    private SpriteRenderer sr;
    private Dictionary<string, Animation> animations;
    private Stack<KeyValuePair<Animation, int>> pausedAnimations;     // stores animation and the frame it was on
    public bool startOff;            // Used for if the animation should not be playing immidiately or if you may change the folder path
    public Animation currAnimation;
    private float maxSpeed;

    public Dictionary<string, AudioClip> loadedSounds;
    public List<AudioSource> sources;
    public AudioClip currentSound;
    public string currentSoundName;
    
    private int lastPlayedSource;
    private const float RUNNING_VOLUME = 0.9f;

    private int currIndex;
    private float timer;

    private object animLock = new object();

    void Start() {
        this.sr = this.GetComponent<SpriteRenderer>();
        if (this.sr == null) {
            Debug.LogError("Spritimation needs a Sprite Renderer attached to the same object.");
        }
        else {
            Load();
        }
        
    }

    public void Load() {
        if (this.spriteSheetFolderPath == "") {
            if (!startOff) {
                Debug.LogError("Expected file path not set for gameObject: \"" + this.name +
                               "\"!!! \n Deleting object");
                Destroy(this);
            } 
            return;
        }
        
        if (this.spriteSheetFolderPath.Contains("Assets/Resources/"))
            this.spriteSheetFolderPath = this.spriteSheetFolderPath.Replace("Assets/Resources/", "");
        else if (spriteSheetFolderPath.Contains("Assets\\Resources\\")) 
            this.spriteSheetFolderPath = this.spriteSheetFolderPath.Replace("Assets\\Resources\\", "");
        
        if (this.spriteSheetFolderPath.Last() != '\\' && this.spriteSheetFolderPath.Last() != '/') {
            this.spriteSheetFolderPath += '/';
            //Debug.LogError("Expected file path to end with char \'\\\' (See gameObject: \"" + this.name + "\")");
        }
        
        // Pull all sprite sheets out from the given file.
        var spriteNames = Resources.LoadAll<Texture2D>(spriteSheetFolderPath);
        Dictionary<string, List<Sprite>> spriteSheets = new Dictionary<string, List<Sprite>>();
        foreach (var spriteSheet in spriteNames) {
            string sheetName = spriteSheet.ToString().Split('(')[0].Trim();
            Sprite[] sprites = Resources.LoadAll<Sprite>(spriteSheetFolderPath + sheetName);
            if (sprites.Length == 0) {
                Debug.LogError("SPRITE SHEET \"" + sheetName + "\" HAS NOT BE SLICED CORRECTLY! FIX IN SPRITE EDITOR!");
            }
            spriteSheets[sheetName] = new List<Sprite>(sprites);
        }
        
        // Pull all sounds, if there are any
        this.loadedSounds = new Dictionary<string, AudioClip>();
        var soundFiles = Resources.LoadAll<AudioClip>(this.spriteSheetFolderPath + "Data\\Sounds");
        foreach (var sound in soundFiles) {
            string soundName = sound.ToString().Split('(')[0].Trim();
            loadedSounds[soundName] = sound;
        }
        this.sources = new List<AudioSource>();
        
        // Create animations from animation data in file
        TextAsset dataFile = Resources.Load<TextAsset>(spriteSheetFolderPath + "Data\\animations");
        AnimationData[] loadedData = JsonUtility.FromJson<Bleg>(dataFile.text).bleg;
        this.animations = new Dictionary<string, Animation>();
        foreach (var data in loadedData) {
            if (!spriteSheets.ContainsKey(data.spriteSheetName)) {
                Debug.LogError(this.gameObject.name + " IS MISSING ANIMATION SPRITESHEET NAMED: " + data.spriteSheetName);
            }
            List<Sprite> crandburries = spriteSheets[data.spriteSheetName];
            if (data.length + data.startFrame > crandburries.Count) {
                Debug.LogError(this.gameObject.name + " HAS INCORRECT DATA (LENGTH & STARTFRAME) FOR ANIMATION NAMED: \"" + data.animationName +
                               "\"\nGiven Length [" + data.length + "] starting at frame [" + data.startFrame + "]" +
                               " but spritesheet ends at frame [" + (crandburries.Count - 1) + "]! FIX!");
            }
            else {
                List<Sprite> animSprites = crandburries.GetRange(data.startFrame, data.length);
                // If this animation has sounds
                if (data.soundOnIndices != null && data.soundNames != null) {
                    Dictionary<string, AudioClip> animationSounds = new Dictionary<string, AudioClip>();

                    // Gather all sounds associated with this animation
                    foreach (var soundName in data.soundNames) {
                        if (soundName.Contains("Assets/Resources/") || soundName.Contains("Assets\\Resources\\")) {
                            var soundPath = soundName.Replace("Assets/Resources/", "");
                            soundPath = soundPath.Replace("Assets\\Resources\\", "");
                            soundPath = soundPath.Replace(".mp3", "");
                            
                            
                            AudioClip sound = Resources.Load<AudioClip>(soundPath);
                            if (sound == null) {
                                Debug.LogError("Could not load audio clip found at file path \"" + soundName + "\"");
                                continue;
                            }
                            string realSoundName = sound.ToString().Split('(')[0].Trim();
                            animationSounds[realSoundName] = sound;
                            
                        } else {


                            if (!loadedSounds.ContainsKey(soundName)) {
                                Debug.LogError(this.gameObject.name + " specifies a sound named \"" + soundName +
                                               "\" for animation named \"" + data.animationName +
                                               "\", but no sound found in Data/Sounds");
                            }

                            animationSounds[soundName] = loadedSounds[soundName];
                        }
                    }

                    // Set up the audio sources for this spritimation
                    // If there are already audio sources attatched to this object, get their reference
                    AudioSource[] preSources = this.gameObject.GetComponents<AudioSource>();
                    foreach (var source in preSources) {
                        this.sources.Add(source);
                    }
                    // Worst case, the most sources we might need is the number of times an animation plays on a sound
                    for (int i = this.sources.Count; i < data.soundOnIndices.Length; i++) {
                        this.sources.Add(this.gameObject.AddComponent<AudioSource>());
                        this.sources[i].loop = false;
                        this.sources[i].volume = RUNNING_VOLUME;
                    }
                    //Debug.Log("Animation \"" + data.animationName + "\" has " + animationSounds.Count + " sounds");
                    // If the sounds are supposed to be played sequentially, get their specified order
                    string[] clipOrder = new string[0];
                    if (data.sequentialSounds)
                        clipOrder = data.soundNames;
                    
                    // Create the actual animation object
                    this.animations[data.animationName] =
                        new Animation(data.animationName, animSprites, data.framesPerSecond, data.soundOnIndices, animationSounds, clipOrder);
                } else {
                    if (data.soundOnIndices != null && data.soundNames == null) {
                        Debug.LogError("Could not load sounds for " + this.gameObject.name + "\'s animation \"" +
                                       data.animationName + "\"; sound names were not included.");
                    } else if (data.soundOnIndices == null && data.soundNames != null) {
                        Debug.LogError("Could not load sounds for " + this.gameObject.name + "\'s animation \"" +
                                       data.animationName + "\" either indices were not included.");
                    }
                    this.animations[data.animationName] =
                        new Animation(data.animationName, animSprites, data.framesPerSecond);
                }
            }
        }
        // TODO: Check if this exists and wait
        this.pausedAnimations = new Stack<KeyValuePair<Animation, int>>();

        if (this.animations.ContainsKey("Idle") && !startOff) {
            this.currAnimation = this.animations["Idle"];
            if (this.currAnimation.hasSounds) {
                var first = this.currAnimation.sounds.First();
                this.SetSound(first.Key);
            }
        } else
            this.currAnimation = Animation.Empty;
        
        // Application.Bleg
    }

    // Update is called once per frame
    void Update() {
        if (this.currAnimation.name == Animation.Empty.name)
            return;

        lock (this.animLock) {
            this.timer += Time.deltaTime;

            int newIndex = (int) ((this.timer * this.currAnimation.framesPerSecond) % this.currAnimation.sprites.Count);
            if (newIndex != this.currIndex) {
                // If popping an animation off the one-shot stack
                if (this.pausedAnimations.Count > 0 && newIndex == 0 && this.currIndex != -1) {
                    var bleg = this.pausedAnimations.Pop();
                    this.currAnimation = bleg.Key;
                    this.currIndex = bleg.Value;
                    //Debug.Log("Reverting back to animation \"" + this.currAnimation.name + "\" on frame " + this.currIndex);
                }
                else {
                    this.currIndex = newIndex;
                }

                try {
                    this.sr.sprite = this.currAnimation.sprites[this.currIndex];
                }
                catch (Exception e) {
                    // Do nothing
                }

                // If sound is supposed to play on this index
                if (this.currAnimation.hasSounds && this.currAnimation.soundOnIndices[this.currIndex]) {
                    // If it's supposed to be sequential, then get the appropriate sound for this spot.
                    if (this.currAnimation.sequentialSounds)
                        this.SetSound(this.currAnimation.NextClipName());

                    if (this.sources[this.lastPlayedSource].isPlaying)
                        this.lastPlayedSource++;
                    this.lastPlayedSource %= this.sources.Count;
                    this.sources[this.lastPlayedSource].clip = this.currentSound;
                    this.sources[this.lastPlayedSource].Play();
                }
            }
        }
    }

    public SpriteRenderer GetSpriteRenderer() {
        return this.sr;
    }

    /// <summary>
    /// <para> Returns true if the animation can be played.</para>
    /// </summary>
    /// <param name="animName">name of the animation</param>
    public bool HasAnimation(string animName) {
        return this.animations.ContainsKey(animName);
    }
    
    
    /// <summary>
    /// Sets the animation of the current sheet based on the animation name.
    /// </summary>
    /// <param name="animName">name of the animation</param>
    public void SetAnimation(string animName) {
        if (!this.HasAnimation(animName)) {
            Debug.LogError("Animation \"" + animName + "\" does not exist for gameObject \"" + this.name + "\"");
            return;
        }

        if (animName == this.currAnimation.name) {
            return;
        }
        
        lock (this.animLock) {

            this.currIndex = -1;
            this.timer = 0f;
            this.currAnimation = this.animations[animName];


            if (this.currAnimation.hasSounds) {
                var first = this.currAnimation.sounds.First();
                this.SetSound(first.Key);
            }
            
        }
    }
    
    /// <summary>
    /// Sets the animation of the current sheet based on the animation name.
    /// Given a max speed, sets the framerate to be directly proportional to the current
    ///     speed of this gameObject, where speed > maxSpeed ceilings at anim.frameRate
    /// </summary>
    /// <param name="animName"></param>
    /// <param name="maxSpeed"></param>
    public void SetVelocityDependentAnimation(string animName, float maxSpeed) {
        
    }
    
    /// <summary>
    /// Plays this animation once, then reverts back to the previous animation playing.
    /// If the given animation is already playing, just restarts the animation
    /// Otherwise, pauses the previous animation and saves it to be resumed later
    /// </summary>
    /// <param name="animName"></param>
    /// returns the predicted duration of the animation being played; 0
    public float SetOneShotAnimation(string animName) {
        if (!this.HasAnimation(animName)) {
            Debug.LogError("Animation \"" + animName + "\" does not exist for gameObject \"" + this.name + "\"");
            return 0.0f;
        }

        lock (this.animLock) {
            if (animName == this.currAnimation.name) {
                this.currIndex = 0;
                return (1.0f / this.currAnimation.framesPerSecond) * this.currAnimation.sprites.Count;
            }

            this.pausedAnimations.Push(new KeyValuePair<Animation, int>(this.currAnimation, this.currIndex));

            this.currIndex = -1;
            this.timer = 0f;
            this.currAnimation = this.animations[animName];

            //Debug.Log(animName + " has sounds: " + this.currAnimation.hasSounds);

            if (this.currAnimation.hasSounds) {
                var first = this.currAnimation.sounds.First();
                this.SetSound(first.Key);
            }

            // 1 / (f/s) * (f/A) = (s/A) {seconds per animation}
            return (1.0f / this.currAnimation.framesPerSecond) * this.currAnimation.sprites.Count;
        }
    }

    // Loads a sound for the current animation
    public void SetSound(string soundName) {
        if (soundName == this.currentSoundName)
            return;
        
        if (!this.currAnimation.sounds.ContainsKey(soundName)) {
            Debug.Log("Spritimation does not have sound for " + soundName);
//            Debug.LogError("The animation \"" + this.currAnimation.name
//                                              + "\" does not have the sound \"" + soundName + "\"");
            return;
        }

        AudioClip sound = this.currAnimation.sounds[soundName];
//        foreach (var source in this.sources) {
//            source.Stop();
//            source.clip = sound;
//        }

        this.lastPlayedSource = 0;

        this.currentSound = sound;
        this.currentSoundName = soundName;
    }

    // Stops playing animations. 
    // Removes the sprite so that players don't see it
    // Can be turned back on by calling play for any animation
    public void Off() {
        this.sr.sprite = null;
        this.currIndex = 0;
        this.currAnimation = Animation.Empty;
    }
    
    // Pauses the sprite on the first frame
    public void Pause() {
        this.currIndex = 0;
        this.currAnimation = Animation.Empty;
    }

    [System.Serializable]
    public class Animation {
        public string name;
        public List<Sprite> sprites;
        public float framesPerSecond;
        public bool hasSounds;
        public bool[] soundOnIndices;
        public Dictionary<string, AudioClip> sounds;
        // Fields for sound order
        public bool sequentialSounds;
        private string[] clipOrder;
        private int currentSoundClipIndex;

        public Animation() {
            this.sprites = new List<Sprite>();
            this.name = "Empty";
        }

        // If velocityDependantFrameRate, framesPerSecond is the max number of FpS the animation can play
        public Animation(string name, List<Sprite> sprites, float framesPerSecond) {
            this.name = name;
            this.sprites = sprites;
            this.framesPerSecond = framesPerSecond;
            this.hasSounds = false;
        }

        public Animation(string name, List<Sprite> sprites, float framesPerSecond, int[] indices, Dictionary<string, AudioClip> sounds, string[] clipOrder) {
            this.name = name;
            this.sprites = sprites;
            this.framesPerSecond = framesPerSecond;
            this.hasSounds = true;
            this.soundOnIndices = new bool[sprites.Count];
            foreach (int i in indices) {
                if (i >= sprites.Count) {
                    Debug.LogError("Sound Indices contains index " + i
                                   + ", but sprite length is " + sprites.Count);
                    continue;
                }
                this.soundOnIndices[i] = true;
            }
            this.sounds = sounds;
            this.sequentialSounds = clipOrder.Length > 0;
            this.clipOrder = clipOrder;
            this.currentSoundClipIndex = 0;
        }
        
        public static Animation Empty = new Animation();

        // Returns the clip that currentSoundClipIndex is currently pointing to before increment.
        public string NextClipName() {
            if (!this.sequentialSounds)
                return sounds.First().Key;
            string clipName = this.clipOrder[this.currentSoundClipIndex];
            this.currentSoundClipIndex = ++this.currentSoundClipIndex % this.clipOrder.Length;
            return clipName;
        }
    }

    [System.Serializable]
    private struct Bleg {
        public AnimationData[] bleg;

        public Bleg(AnimationData[] bleg) {
            this.bleg = bleg;
        }
    }
    
    [System.Serializable]
    private struct AnimationData {
        public string spriteSheetName;
        public string animationName;
        public int startFrame;
        public int length;
        public float framesPerSecond;
        public int[] soundOnIndices;
        public string[] soundNames;
        public bool sequentialSounds;

        public AnimationData(string spriteSheetName, string animationName, int startFrame, int length,
                             float framesPerSecond) {
            this.spriteSheetName = spriteSheetName;
            this.animationName = animationName;
            this.startFrame = startFrame;
            this.length = length;
            this.framesPerSecond = framesPerSecond;
            this.soundOnIndices = new int[]{};
            this.soundNames = new string[]{};
            this.sequentialSounds = false;
        }
        
        public AnimationData(string spriteSheetName, string animationName, int startFrame, int length,
                             float framesPerSecond, int[] soundOnIndices, string[] soundNames, bool sequentialSounds) {
            this.spriteSheetName = spriteSheetName;
            this.animationName = animationName;
            this.startFrame = startFrame;
            this.length = length;
            this.framesPerSecond = framesPerSecond;
            this.soundOnIndices = soundOnIndices;
            this.soundNames = soundNames;
            this.sequentialSounds = sequentialSounds;
        }
    }
}
