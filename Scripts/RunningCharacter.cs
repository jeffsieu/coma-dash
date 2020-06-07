using Godot;
using System;

public class RunningCharacter : Spatial
{
    AnimationPlayer player;

    bool stopped = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        player = GetNode<AnimationPlayer>("AnimationPlayer");
        player.Play("animation_run_pistol_1");
        player.GetAnimation("animation_run_pistol_1").Loop = true;
        player.PlaybackSpeed = 2;
        stopped = false;
    }

    public override void _Process(float delta) {
        if (Input.IsActionPressed("movement_sprint")) {
            if (stopped) {
                RunAnimation();
            }
            else {
                StopAnimation();
            }
        }
    }

    public void SetSpeed(float speed) {
        player.PlaybackSpeed = speed;
    }

    public void RunAnimation() {
        player.Play("animation_run_pistol_1");
    }

    public void StopAnimation() {
        player.Stop();
    }

}
