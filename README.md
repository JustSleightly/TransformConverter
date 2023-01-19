# TransformConverter [<img src="https://github.com/JustSleightly/Resources/raw/main/Icons/JSLogo.png" width="30" height="30">](https://vrc.sleightly.dev/ "JustSleightly") [<img src="https://github.com/JustSleightly/Resources/raw/main/Icons/Discord.png" width="30" height="30">](https://discord.sleightly.dev/ "Discord") [<img src="https://github.com/JustSleightly/Resources/raw/main/Icons/GitHub.png" width="30" height="30">](https://github.sleightly.dev/ "Github") [<img src="https://github.com/JustSleightly/Resources/raw/main/Icons/Store.png" width="30" height="30">](https://store.sleightly.dev/ "Store")

[![GitHub stars](https://img.shields.io/github/stars/JustSleightly/TransformConverter)](https://github.com/JustSleightly/TransformConverter/stargazers) [![GitHub Tags](https://img.shields.io/github/tag/JustSleightly/TransformConverter)](https://github.com/JustSleightly/TransformConverter/tags) [![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/JustSleightly/TransformConverter?include_prereleases)](https://github.com/JustSleightly/TransformConverter/releases) [![GitHub issues](https://img.shields.io/github/issues/JustSleightly/TransformConverter)](https://github.com/JustSleightly/TransformConverter/issues) [![GitHub last commit](https://img.shields.io/github/last-commit/JustSleightly/TransformConverter)](https://github.com/JustSleightly/TransformConverter/commits/main) [![Discord](https://img.shields.io/discord/780192344800362506)](https://discord.sleightly.dev/) ![Twitter Follow](https://img.shields.io/twitter/follow/SleightlyDev?style=social)

A Unity editor extension that replaces all transform animation properties within selected animation clips with position/rotation/parent/~~scale~~ constraint offset properties. This is primarily developed to assist with avoiding transform animations on [VRChat Avatars due to complex issues with Avatar Masks on Animator Controllers](https://docs.vrchat.com/docs/playable-layers#fx).

While transform animations can mostly be emulated with constraint offset animations, this script only handles replacing the properties of animation clips and does not create the constraint components on your animated gameobjects in your hierarchy.

**Scale Transform Animations do not convert nicely so they have been removed from functionality until a better solution is implemented.**

---
### **[Download Here!](https://github.com/JustSleightly/TransformConverter/releases)**
---

######

![](https://github.com/JustSleightly/TransformConverter/raw/main/Examples/Demo.gif)

## Constraint Setup

For any gameobject that you would normally animate transforms for, add a constraint component instead for the respective transform you are trying to emulate. If you are animating both position and rotation, you can opt to use a parent constraint instead and toggle the option for `Using Parent Constraints` in the TransformConverter editor window.

For each constraint, you will typically set the constraint settings so that the `At Rest` and `Offset` values match your default transform values for that transform property. 

For each constraint source, you will typically set the immediate parent gameobject from the hierarchy as the only source. ~~with the exception of _**Scale Constraints**_ which should use a *WorldTransform* prefab of *(1,1,1)* scale instead. This prefab should not exist in your hierarchy/scene and only exist in your project assets instead so as to maintain absolute values. Note that the actual transform value of your scale may deviate from your animated offset when not a power of 2.~~


An example prefab can be found in the [examples](https://github.com/JustSleightly/TransformConverter/tree/main/Examples) folder.


<details> 

  <summary> Position Constraint Example </summary>

######

<blockquote>

![](https://github.com/JustSleightly/TransformConverter/raw/main/Examples/PositionConstraint.png)

</details>

<details> 

  <summary> Rotation Constraint Example </summary>

######

<blockquote>

![](https://github.com/JustSleightly/TransformConverter/raw/main/Examples/RotationConstraint.png)

</details>

<details> 

  <summary> Parent Constraint Example </summary>

######

<blockquote>

![](https://github.com/JustSleightly/TransformConverter/raw/main/Examples/ParentConstraint.png)

</details>

<details> 

  <summary> <s> Scale Constraint Example </s> </summary>

######

<blockquote>

![](https://github.com/JustSleightly/TransformConverter/raw/main/Examples/ScaleConstraint.png)

</details>
