//using Godot;

//public partial class TankController : VehicleBody3D
//{

//    [ExportGroup("Stability")]
//    [Export] public float AlignSpeed = 6f;
//    [Export] public float AngularDamping = 4f;

//    [ExportGroup("Movement")]
//    [Export] public float EnginePower = 1200f;
//    [Export] public float TurnMultiplier = 4f;

//    [ExportGroup("Wheels")]
//    [Export] public VehicleWheel3D FrontLeftWheel = new();
//    [Export] public VehicleWheel3D RearLeftWheel = new();
//    [Export] public VehicleWheel3D FrontRightWheel = new();
//    [Export] public VehicleWheel3D RearRightWheel = new();

//    public override void _PhysicsProcess(double delta)
//    {
//        Vector2 movement = PlayerInput.Movement;

//        // X = left/right turning
//        // Negative X means left, positive X means right
//        float steer = movement.X;

//        // Y = forward/backward movement
//        // Godot's GetVector returns forward as negative Y,
//        // so we invert it to make forward = +1
//        float moveDir = -movement.Y;

//        if (steer != 0 && moveDir != 0)
//        {
//            // Move and turn at the same time.
//            // One side gets less power, the other gets more power.
//            SetLeftWheelForce(EnginePower * (moveDir + steer));
//            SetRightWheelForce(EnginePower * (moveDir - steer));
//        }
//        else if (steer != 0)
//        {
//            // Turn in place.
//            // Left and right wheels move in opposite directions.
//            SetLeftWheelForce(EnginePower * steer * TurnMultiplier);
//            SetRightWheelForce(EnginePower * -steer * TurnMultiplier);
//        }
//        else
//        {
//            // Move straight forward or backward.
//            SetLeftWheelForce(EnginePower * moveDir);
//            SetRightWheelForce(EnginePower * moveDir);
//        }

//    }

//    //private void SetLeftWheelForce(float force)
//    //{
//    //    foreach (VehicleWheel3D wheel in LeftWheels)
//    //    {
//    //        if (wheel == null)
//    //            continue;

//    //        wheel.EngineForce = force;
//    //    }

//    //}

//    //private void SetRightWheelForce(float force)
//    //{
//    //    foreach (VehicleWheel3D wheel in RightWheels)
//    //    {
//    //        if (wheel == null)
//    //            continue;

//    //        wheel.EngineForce = force;
//    //    }
//    //}

    
//}