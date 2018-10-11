using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using g3;
using f3;

public class VRGestureBehavior : StandardInputBehavior
{
    FContext context;

    public VRGestureBehavior(FContext context)
    {
        this.context = context;
    }

    public override InputDevice SupportedDevices {
        get { return InputDevice.AnySpatialDevice; }
    }



    /// <summary>
    /// THIS CLASS STORES THE ACCUMULATED HAND-POSITION STROKES
    /// </summary>
    class GestureInfo
    {
        public List<Frame3f> StrokeFrames;

        public GestureInfo(Frame3f handFrame)
        {
            StrokeFrames = new List<Frame3f>();
            AppendPoint(handFrame);
        }

        public void AppendPoint(Frame3f handFrame)
        {
            StrokeFrames.Add(handFrame);
        }
    }



    // CHECK IF WE SHOULD BEGIN "CAPTURING" THE CONTROLLER INPUT ON EITHER HAND
    public override CaptureRequest WantsCapture(InputState input)
    {
        // YOU SHOULD CHANGE THESE TO BE SUITABLE FOR YOUR BUTTON MAPPINGS ETC

        if (input.bLeftTriggerPressed) {
            return CaptureRequest.Begin(this, CaptureSide.Left);

        } else if (input.bRightTriggerPressed) {
            return CaptureRequest.Begin(this, CaptureSide.Right);

        } else
            return CaptureRequest.Ignore;
    }


    // STARTS THE A STROKE CAPTURE
    public override Capture BeginCapture(InputState input, CaptureSide eSide)
    {
        Ray3f useRay = (eSide == CaptureSide.Left) ? input.vLeftSpatialWorldRay : input.vRightSpatialWorldRay;
        Frame3f handF = (eSide == CaptureSide.Left) ? input.LeftHandFrame : input.RightHandFrame;
        return Capture.Begin(this, eSide, new GestureInfo(handF) );
    }


    // CALLED EVERY FRAME WITH NEW INPUT STATE (BUTTON POSITIONS, ETC)
    public override Capture UpdateCapture(InputState input, CaptureData data)
    {
        GestureInfo gi = data.custom_data as GestureInfo;
        gi.AppendPoint((data.which == CaptureSide.Left) ? input.LeftHandFrame : input.RightHandFrame);


        // FIGURE OUT IF STROKE IS FINISHED
        bool bFinished = false;
        if (data.which == CaptureSide.Left && input.bLeftTriggerReleased) {
            bFinished = true;
        } else if (data.which == CaptureSide.Right && input.bRightTriggerReleased) {
            bFinished = true;
        }


        if (bFinished) {

            // PROCESS GESTURE INFO HERE

            // (HERE IS A SAMPLE THAT DETECTS UP VS DOWN STROKES)
            double dy = gi.StrokeFrames.Last().Origin.y - gi.StrokeFrames.First().Origin.y;
            if (dy > 0)
                f3.DebugUtil.Log("UP STROKE!");
            else
                f3.DebugUtil.Log("DOWN STROKE!");

            return Capture.End;
        } 

        return Capture.Continue;
    }

    public override Capture ForceEndCapture(InputState input, CaptureData data)
    {
        GestureInfo gi = data.custom_data as GestureInfo;
        return Capture.End;
    }

}