using UnityEngine;

public class EquipTrackModule : MobModule
{
    private Transform _toolTrack;  
    public override void Initialize()
    {
        _toolTrack = Machine.transform.Find("sprite").transform.Find("tool_track"); 
    }

    public override void Update()
    {
        if(!Info.Target) return;
        
        Vector3 targetPos = Camera.main.WorldToScreenPoint(Info.Target.transform.position);
        Vector3 selfPos = Camera.main.WorldToScreenPoint(Machine.transform.position);
        Vector2 direction = targetPos - selfPos;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (angle > 90)
            angle = 180 - angle;
        else if (angle < -90)
            angle = -angle - 180;

        float z = Mathf.Lerp(-0.45f, 0.45f, (angle + 90) / 180);
        if (z is > 0f and <= 0.12f)
            z = 0.12f;
        else if (z is < 0f and >= -0.11f)
            z = -0.11f;
        // float angleX = (Mathf.Lerp(0, 90, Math.Abs(angle) / 45) + 360) % 360;
        // Normalize angle to 0–360
        _toolTrack.localPosition = new Vector3(0, 0.3f, z);
        _toolTrack.localRotation = Quaternion.Euler(80, 0, (angle + 360) % 360);
    }
}