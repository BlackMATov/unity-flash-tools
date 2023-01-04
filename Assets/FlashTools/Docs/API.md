## SwfClip

```csharp
// Occurs when clip changes
event Action<SwfClip> OnChangeClipEvent;

// Occurs when sequence changes
event Action<SwfClip> OnChangeSequenceEvent;

// Occurs when current frame changes
event Action<SwfClip> OnChangeCurrentFrameEvent;
```

```csharp
// Gets or sets the animation mesh renderer sorting layer
// [value] - The sorting layer
string sortingLayer { get; set; }

// Gets or sets the animation mesh renderer sorting order
// [value] - The sorting order
int sortingOrder { get; set; }

// Gets or sets the animation tint color
// [value] - The tint color
Color tint { get; set; }

// Gets or sets the animation asset (reset sequence and current frame)
// [value] - The animation asset
SwfClipAsset clip { get; set; }

// Gets or sets the animation sequence (reset current frame)
// [value] - The animation sequence
string sequence { get; set; }

// Gets or sets the animation current frame
// [value] - The animation current frame
int currentFrame { get; set; }

// Gets the current animation sequence frame count
// [value] - The frame count
int frameCount { get; }

// Gets the animation frame rate
// [value] - The frame rate
float frameRate { get; }

// Gets the current frame label count
// [value] - The frame label count
int currentLabelCount { get; }
```

```csharp
// Gets the current frame mesh bounding volume in local space (Since 1.3.8)
// [value] - The bounding volume in local space
Bounds currentLocalBounds { get; }

// Gets the current frame mesh bounding volume in world space (Since 1.3.8)
// [value] - The bounding volume in world space
Bounds currentWorldBounds { get; }
```

```csharp
// Rewind current sequence to begin frame
void ToBeginFrame();

// Rewind current sequence to end frame
void ToEndFrame();

// Rewind current sequence to previous frame
// [returns] - [true], if animation was rewound, [false] otherwise
bool ToPrevFrame();

// Rewind current sequence to next frame
// [returns] - [true], if animation was rewound, [false] otherwise
bool ToNextFrame();

// Gets the current frame label by index
// [returns] - The current frame label
// [index] - Current frame label index
string GetCurrentFrameLabel(int index);
```

## SwfClipController

```csharp
// Occurs when the controller stops played clip
event Action<SwfClipController> OnStopPlayingEvent;

// Occurs when the controller plays stopped clip
event Action<SwfClipController> OnPlayStoppedEvent;

// Occurs when the controller rewinds played clip
event Action<SwfClipController> OnRewindPlayingEvent;
```

```csharp
// Controller play modes
enum PlayModes {
	// Forward play mode
	Forward,
	// Backward play mode
	Backward
}

// Controller loop modes
enum LoopModes {
	// Once loop mode
	Once,
	// Repeat loop mode
	Loop
}

// Gets or sets a value indicating whether controller play after awake on scene
// [value] - [true] if auto play; otherwise, [false]
bool autoPlay { get; set; }

// Gets or sets a value indicating whether controller uses unscaled delta time
// [value] - [true] if uses unscaled delta time; otherwise, [false]
bool useUnscaledDt { get; set; }

// Gets or sets the controller rate scale
// [value] - The rate scale
float rateScale { get; set; }

// Gets or sets the controller group name
// [value] - The group name
string groupName { get; set; }

// Gets or sets the controller play mode
// [value] - The play mode
PlayModes playMode { get; set; }

// Gets or sets the controller loop mode
// [value] - The loop mode
LoopModes loopMode { get; set; }

// Gets the controller clip
// [value] - The clip
SwfClip clip { get; }

// Gets a value indicating whether controller is playing
// [value] - [true] if is playing; otherwise, [false]
bool isPlaying { get; }

// Gets a value indicating whether controller is stopped
// [value] - [true] if is stopped; otherwise, [false]
bool isStopped { get; }
```

```csharp
// Changes the animation frame with stops it
// [frame] - The new current frame
void GotoAndStop(int frame);

// Changes the animation sequence and frame with stops it
// [sequence] - The new sequence
// [frame]    - The new current frame
void GotoAndStop(string sequence, int frame);

// Changes the animation frame with plays it
// [frame] - The new current frame
void GotoAndPlay(int frame);

// Changes the animation sequence and frame with plays it
// [sequence] - The new sequence
// [frame]    - The new current frame
void GotoAndPlay(string sequence, int frame);

// Stop with specified rewind action
// [rewind] - If set to [true] rewind animation to begin frame
void Stop(bool rewind);

// Changes the animation sequence and stop controller with rewind
// [sequence] - The new sequence
void Stop(string sequence);

// Play with specified rewind action
// [rewind] - If set to [true] rewind animation to begin frame
void Play(bool rewind);

// Changes the animation sequence and play controller with rewind
// [sequence] - The new sequence
void Play(string sequence);

// Rewind animation to begin frame
void Rewind();
```

## SwfManager

```csharp
// Get cached manager instance from scene or create it (if allowed)
// [allow_create] - If set to [true] allow create
static SwfManager GetInstance(bool allow_create);
```

```csharp
// Get animation clip count on scene
// [value] - Clip count
int clipCount { get; }

// Get animation clip controller count on scene
// [value] - Clip controller count
int controllerCount { get; }

// Get or set a value indicating whether animation updates is paused
// [value] - [true] if is paused; otherwise, [false]
bool isPaused { get; set; }

// Get or set a value indicating whether animation updates is playing
// [value] - [true] if is playing; otherwise, [false]
bool isPlaying { get; set; }

// Get or set a value indicating whether animation updates uses unscaled delta time
// [value] - [true] if uses unscaled delta time; otherwise, [false]
bool useUnscaledDt { get; set; }

// Get or set the global animation rate scale
// [value] - Global rate scale
float rateScale { get; set; }
```

```csharp
// Pause animation updates
void Pause();

// Resume animation updates
void Resume();

// Pause the group of animations by name
// [group_name] - Group name
void PauseGroup(string group_name);

// Resume the group of animations by name
// [group_name] - Group name
void ResumeGroup(string group_name);

// Determines whether group of animations is paused
// [returns]    - [true] if group is paused; otherwise, [false]
// [group_name] - Group name
bool IsGroupPaused(string group_name);

// Determines whether group of animations is playing
// [returns]    - [true] if group is playing; otherwise, [false]
// [group_name] - Group name
bool IsGroupPlaying(string group_name);

// Set the group of animations use unscaled delta time
// [group_name] - Group name
// [yesno]      - [true] if group will use unscaled delta time; otherwise, [false]
void SetGroupUseUnscaledDt(string group_name, bool yesno);

// Determines whether group of animations uses unscaled delta time
// [returns]    - [true] if group uses unscaled delta time; otherwise, [false]
// [group_name] - Group name
bool IsGroupUseUnscaledDt(string group_name);

// Set the group of animations rate scale
// [group_name] - Group name
// [rate_scale] - Rate scale
void SetGroupRateScale(string group_name, float rate_scale);

// Get the group of animations rate scale
// [returns]    - The group rate scale
// [group_name] - Group name
float GetGroupRateScale(string group_name);
```
