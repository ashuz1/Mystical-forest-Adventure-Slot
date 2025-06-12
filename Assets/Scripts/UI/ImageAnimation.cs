using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageAnimation : MonoBehaviour
{
    [SerializeField] private SkeletonGraphic[] skeletonGraphic;
    private int currentSymbol = 0;

    internal void SetSymbol(int symbol, bool current = false)
    {
        currentSymbol = symbol;

        // Disable all skeletons
        foreach (var s in skeletonGraphic)
        {
            s.gameObject.SetActive(false);
        }

        // Choose which to activate
        int indexToUse = current ? currentSymbol : symbol;

        var sg = skeletonGraphic[indexToUse];
        sg.gameObject.SetActive(true);

        string animName = GetMatchingAnimationName("STOP", sg);
        PlayAnimation(animName, indexToUse, false);
    }

    private string GetMatchingAnimationName(string name, SkeletonGraphic sg)
    {
        var animations = sg.Skeleton.Data.Animations;

        foreach (var anim in animations)
        {
            if (anim.Name == name) return anim.Name;
            if (anim.Name == name.ToLower()) return anim.Name;
        }

        // Default fallback
        return name == "STOP" ? "land" : "win2";
    }

    internal void PlayWinAnimation()
    {
        var sg = skeletonGraphic[currentSymbol];

        if (!sg.IsValid) sg.Initialize(true);

        string winAnim = GetMatchingAnimationName("WIN", sg);
        PlayAnimation(winAnim, currentSymbol, true);

    }

    private void PlayAnimation(string animationName, int symbolIndex, bool loop)
    {
        if (skeletonGraphic == null || skeletonGraphic[symbolIndex].AnimationState == null)
        {
            Debug.LogWarning("SkeletonGraphic is not assigned or invalid.");
            return;
        }

        skeletonGraphic[symbolIndex].AnimationState.SetAnimation(0, animationName, loop);
    }
}