﻿using Silk.NET.Input;
using System.Numerics;
using Silk.NET.Maths;

namespace Lab2_2
{
    internal class CameraDescriptor
    {
        //Setup the camera's location, directions, and movement speed
        private static Vector3D<float> CameraPosition = new Vector3D<float>(0.0f, 0.0f, 8.0f);
        private static Vector3D<float> CameraFront = new Vector3D<float>(0.0f, 0.0f, -1.0f);
        private static Vector3D<float> CameraUp = Vector3D<float>.UnitY;
        private static Vector3D<float> CameraDirection = Vector3D<float>.Zero;
        private static float CameraYaw = -90f;
        private static float CameraPitch = 0f;
        private static float CameraZoom = 45f;
        //Used to track change in mouse movement to allow for moving of the Camera
        private static Vector2 LastMousePosition;
        private static float DegreesToRadians(float degrees)
        {
            return MathF.PI / 180f * degrees;
        }
        public unsafe void LookAtMouse(IMouse mouse, Vector2 position)
        {
            var lookSensitivity = 0.1f;
            if (LastMousePosition == default) { LastMousePosition = position; }
            else
            {
                var xOffset = (position.X - LastMousePosition.X) * lookSensitivity;
                var yOffset = (position.Y - LastMousePosition.Y) * lookSensitivity;
                LastMousePosition = position;

                CameraYaw += xOffset;
                CameraPitch -= yOffset;

                //We don't want to be able to look behind us by going over our head or under our feet so make sure it stays within these bounds
                CameraPitch = Math.Clamp(CameraPitch, -89.0f, 89.0f);

                CameraDirection.X = MathF.Cos(DegreesToRadians(CameraYaw)) * MathF.Cos(DegreesToRadians(CameraPitch));
                CameraDirection.Y = MathF.Sin(DegreesToRadians(CameraPitch));
                CameraDirection.Z = MathF.Sin(DegreesToRadians(CameraYaw)) * MathF.Cos(DegreesToRadians(CameraPitch));
                CameraFront = Vector3D.Normalize(CameraDirection);

            }
        }
        public unsafe void ZoomMouseWheel(IMouse mouse, ScrollWheel scrollWheel)
        {
            //We don't want to be able to zoom in too close or too far away so clamp to these values
            CameraZoom = Math.Clamp(CameraZoom - scrollWheel.Y, 1.0f, 45f);
        }
        public Matrix4X4<float> getView()
        {
            return Matrix4X4.CreateLookAt<float>(CameraPosition, CameraPosition + CameraFront, CameraUp);
        }
        public Matrix4X4<float> getProjection(Vector2D<int> size)
        {
            return Matrix4X4.CreatePerspectiveFieldOfView(DegreesToRadians(CameraZoom), (float)size.X / size.Y, 0.1f, 100.0f);
        }
        public void MoveUp(float moveSpeed)
        {
            CameraPosition += moveSpeed * CameraUp;
        }

        public void MoveDown(float moveSpeed)
        {
            CameraPosition -= moveSpeed * CameraUp;
        }

        public void MoveRight(float moveSpeed)
        {
            CameraPosition += Vector3D.Normalize(Vector3D.Cross(CameraFront, CameraUp)) * moveSpeed;
        }

        public void MoveLeft(float moveSpeed)
        {
            CameraPosition -= Vector3D.Normalize(Vector3D.Cross(CameraFront, CameraUp)) * moveSpeed;
        }

        public void MoveForward(float moveSpeed)
        {
            CameraPosition += moveSpeed * CameraFront;
        }

        public void MoveBackward(float moveSpeed)
        {
            CameraPosition -= moveSpeed * CameraFront;
        }
    }
}
