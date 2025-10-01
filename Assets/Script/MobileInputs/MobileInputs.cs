using UnityEngine;

public class MobileInput : MonoBehaviour
{
    public static bool MoveLeft;
    public static bool MoveRight;
    public static bool ClimbUp;
    public static bool ClimbDown;
    public static bool Shoot;

    public void PressLeft() => MoveLeft = true;
    public void ReleaseLeft() => MoveLeft = false;

    public void PressRight() => MoveRight = true;
    public void ReleaseRight() => MoveRight = false;

    public void PressUp() => ClimbUp = true;
    public void ReleaseUp() => ClimbUp = false;

    public void PressDown() => ClimbDown = true;
    public void ReleaseDown() => ClimbDown = false;

    public void PressShoot() => Shoot = true;
    public void ReleaseShoot() => Shoot = false;
}
