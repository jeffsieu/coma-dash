using System;
using Godot;

public class RunningCharacter : Spatial
{
    AnimationPlayer player;

    MeshInstance weaponPosition;

    bool stopped = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        weaponPosition = GetNode<MeshInstance>("WeaponPosition");

        player = GetNode<AnimationPlayer>("AnimationPlayer");
        player.Play("animation_run_pistol_1");
        player.GetAnimation("animation_run_pistol_1").Loop = true;
        player.GetAnimation("Idle").Loop = true;
        stopped = false;
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionPressed("movement_sprint"))
        {
            if (stopped)
            {
                RunAnimation();
            }
            else
            {
                StopAnimation();
            }
        }
    }

    public Vector3 GetWeaponOffset()
    {
        return weaponPosition.Translation;
    }

    public void SetSpeed(float speed)
    {
        player.PlaybackSpeed = speed;
    }

    public void PlayIdle()
    {
        player.Play("Idle");
    }

    public void RunAnimation()
    {
        player.Play("animation_run_pistol_1");
    }

    public void StopAnimation()
    {
        player.Stop();
    }

}
