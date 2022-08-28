# Sprite Animator for Unity
This package is designed to work with sprite sheet animation in unity.

## Creating Animations
To create a sprite animation, right-click the folder you want to create it in and select:

> Animations/ Sprite Animation

### Custom Inspector
By default, the custom inspector will be enabled.<br>
It allows you to load in a texture and, given that you have set it to multiple and cut it correctly beforehand, it will import all child sprites.<br>
I am not the best with custom inspectors though, so you can turn it off via a toggle on the top of the window.

## Playing an Animation
Use the `SpriteAnimator` component to handle playing the animation.<br>
Call the `PlayAnimation(index)` method to play an animation of the given index.

## Animator States
You can get the animator's current state with the `AnimatorPlaybackState` property.<br>
You can also see if the animator's coroutine is currently running with the `IsRunning` property.

### Pause
To pause the animator, use the `PausePlayback()` method.

### Stop
To stop the animator completely and reset the frame counter, call the `StopPlayback()` method.

### Restart or Resume
To restart the animator or, if paused, resume it, use the `StartPlayback()` method.

## Synchronizing Animators
Sometimes it's useful de update multiple animations together, fortunately, we can use the `SpriteAnimatorComposite` component to do just that. <br>
Add the animators you wish to sync to the composite's list and it will make sure all of them update based on the longest frame duration.

The composite animator operates the same way as a regular animator, the big difference is that when you pass in an animation index for it to play, it will play that index on all children animators.
This can be good in some cases, but if you wish to play an animation on a specific animator you can always tell that animator directly to play a certain animation and the composite will still control when the frame changes.