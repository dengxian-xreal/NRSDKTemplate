﻿/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/

namespace NRKernal.Record
{
    using System;
    using UnityEngine;
    using NRKernal;
    using System.Collections.Generic;

    /// <summary> A capture behaviour base. </summary>
    public class CaptureBehaviourBase : MonoBehaviour, IFrameConsumer
    {
        /// <summary> The RGB camera rig. </summary>
        [SerializeField] Transform RGBCameraRig;
        /// <summary> The capture camera. </summary>
        public Camera CaptureCamera;
        public Camera CaptureCamera2;
        private FrameCaptureContext m_FrameCaptureContext;

        /// <summary> Gets the context. </summary>
        /// <returns> The context. </returns>
        public FrameCaptureContext GetContext()
        {
            return m_FrameCaptureContext;
        }

        /// <summary> Initializes this object. </summary>
        /// <param name="context">     The context.</param>
        /// <param name="blendCamera"> The blend camera.</param>
        public virtual void Init(FrameCaptureContext context)
        {
            this.m_FrameCaptureContext = context;
        }
        
        /// <summary> Updates the capture behaviour. </summary>
        protected virtual void Update()
        {
            if (m_FrameCaptureContext != null && m_FrameCaptureContext.RequestCameraParam().lockRoll)
            {
                Vector3 eulerAngles = CaptureCamera.transform.eulerAngles;
                eulerAngles.z = 0;
                CaptureCamera.transform.eulerAngles = eulerAngles;
                Vector3 eulerAngles2 = CaptureCamera2.transform.eulerAngles;
                eulerAngles2.z = 0;
                CaptureCamera2.transform.eulerAngles = eulerAngles2;
            }
        }

        public void SetCameraMask(int mask)
        {
            CaptureCamera.cullingMask = mask;
            CaptureCamera2.cullingMask = mask;
        }

        public void SetBackGroundColor(Color color)
        {
            this.CaptureCamera.backgroundColor = color;
            this.CaptureCamera2.backgroundColor = color;
        }

        /// <summary> Executes the 'frame' action. </summary>
        /// <param name="frame"> The frame.</param>
        public virtual void OnFrame(UniversalTextureFrame frame)
        {
            var mode = this.GetContext().GetBlender().BlendMode;
            switch (mode)
            {
                case BlendMode.RGBOnly:
                    MoveToGod();
                    break;
                case BlendMode.Blend:
                case BlendMode.VirtualOnly:
                case BlendMode.WidescreenBlend:
                    // update camera pose
                    UpdateHeadPoseByTimestamp(frame.timeStamp);
                    break;
                default:
                    break;
            }
        }

        private void MoveToGod()
        {
            RGBCameraRig.transform.position = Vector3.one * 9999;
        }

        /// <summary> Updates the head pose by timestamp described by timestamp. </summary>
        /// <param name="timestamp"> The timestamp.</param>
        private void UpdateHeadPoseByTimestamp(UInt64 timestamp)
        {
            Pose head_pose = Pose.identity;
            var poseTracker = NRSessionManager.Instance.NRHMDPoseTracker;
            var result = poseTracker.GetHeadPoseByTimeInUnityWorld(ref head_pose, timestamp);
            if (result)
            {
                // NRDebugger.Info("UpdateHeadPoseByTimestamp: timestamp={0}, pos={1}", timestamp, head_pose.ToString("F2"));
                if (poseTracker.UseRelative)
                {
                    RGBCameraRig.transform.localPosition = head_pose.position;
                    RGBCameraRig.transform.localRotation = head_pose.rotation;
                }
                else
                {
                    RGBCameraRig.transform.position = head_pose.position;
                    RGBCameraRig.transform.rotation = head_pose.rotation;
                }
            }
        }
    }
}
