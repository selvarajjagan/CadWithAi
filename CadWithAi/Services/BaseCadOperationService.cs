using CadWithAi.Models;
using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Media.Media3D;

namespace CadWithAi.Services
{
    public class BaseCadOperationService : IBaseCadOperationService
    {
        public HelixViewport3D Hvp { get; set; }

        public Task<ModelVisual3D> GetLoadedModelAsync()
        {
            ModelVisual3D modelVisual3D = null;
            if (this.Hvp != null)
            {
                modelVisual3D = this.Hvp.Children.Where(x => x is ModelVisual3D).FirstOrDefault() as ModelVisual3D;
            }
            return Task.FromResult(modelVisual3D);
        }

        public Task<string> LoadObjAsync([Description("File path of the OBJ file to load")] string objFilePath)
        {
            if (string.IsNullOrWhiteSpace(objFilePath))
                return Task.FromResult("Error: OBJ file path is null or empty.");
            if (!File.Exists(objFilePath))
                return Task.FromResult($"Error: File '{objFilePath}' does not exist.");
            try
            {
                Model3DGroup model3DGroup = new Model3DGroup();

                ObjReader objReader = new ObjReader();
                var model = objReader.Read(objFilePath);
                if (model != null) model3DGroup = model;
                objReader = null;

                ModelVisual3D modelVisual3D = new ModelVisual3D() { Content = model3DGroup };
                modelVisual3D.Transform = CreateTransform3DGroup();

                if (this.Hvp != null)
                {
                    this.Hvp.Children.Clear();
                    this.Hvp.Children.Add(new SunLight());
                    this.Hvp.Children.Add(modelVisual3D);
                    this.Hvp.ZoomExtents();
                }
                else
                {
                    return Task.FromResult("Error: HelixViewport3D (Hvp) is not initialized.");
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult($"Error loading OBJ file: {ex.Message}");
            }
            return Task.FromResult("OBJ file loaded successfully.");
        }

        public async Task<string> RotateObjAsync([Description("The axis to rotate around")] Axis axis, [Description("The angle in degrees to rotate the model")] double angle)
        {
            if (this.Hvp != null)
            {
                await this.ApplyRotationToViewport(axis, angle);
                return $"Model rotated {angle} degrees around {axis} axis.";
            }
            else
            {
                return "Error: HelixViewport3D (Hvp) is not initialized.";
            }
        }

        public Task<string> SetCameraAngleAsync([Description("The standard camera view. Supported values: Front, Back, Left, Right, Top, Bottom (case-insensitive).")] string direction, [Description("Clockwise rotation of the selected view. Supported values: 0, 90, 180, or 270 degrees.")] int rotationAngle)
        {
            throw new NotImplementedException();
        }


        private async Task ApplyRotationToViewport(Axis axis, double angle)
        {
            Rotation3D rotation3D = new AxisAngleRotation3D(new Vector3D(axis == Axis.X ? 1 : 0, axis == Axis.Y ? 1 : 0, axis == Axis.Z ? 1 : 0), angle);
            RotateTransform3D rotateTransform3D = new RotateTransform3D(rotation3D);

            foreach (var mv in this.Hvp.Children.OfType<ModelVisual3D>())
            {
                if (mv.Transform is Transform3DGroup g)
                {
                    if (g.Children.Count > 1)
                        g.Children[1] = rotateTransform3D;
                    else
                        g.Children.Add(rotateTransform3D);
                }
            }
        }

        private Transform3DGroup CreateTransform3DGroup(
             Double ttx = 0, Double tty = 0, Double ttz = 0,
             Double stx = 1, Double sty = 1, Double stz = 1,
             Double rtx = 0, Double rty = 0, Double rtz = 0
         )
        {
            Transform3DGroup transform3DGroup = new Transform3DGroup();

            ScaleTransform3D scaleTransform3D = new ScaleTransform3D() { ScaleX = stx, ScaleY = sty, ScaleZ = stz };

            RotateTransform3D rotateTransform3D = new RotateTransform3D();
            QuaternionRotation3D quaternionRotation3D = new QuaternionRotation3D();
            rotateTransform3D.Rotation = quaternionRotation3D;
            quaternionRotation3D.Quaternion = GetQuaternionFromEulerAngles(rtx, rty, rtz);
            TranslateTransform3D translateTransform3D = new TranslateTransform3D() { OffsetX = ttx, OffsetY = tty, OffsetZ = ttz };

            transform3DGroup.Children.Add(scaleTransform3D);
            transform3DGroup.Children.Add(rotateTransform3D);
            transform3DGroup.Children.Add(translateTransform3D);

            return transform3DGroup;
        }

        private Quaternion GetQuaternionFromEulerAngles(double angleX, double angleY, double angleZ)
        {
            // Convert degrees to radians
            double radX = Math.PI * angleX / 180.0;
            double radY = Math.PI * angleY / 180.0;
            double radZ = Math.PI * angleZ / 180.0;

            // Create quaternions for each axis rotation
            Quaternion qX = new Quaternion(new Vector3D(1, 0, 0), angleX);
            Quaternion qY = new Quaternion(new Vector3D(0, 1, 0), angleY);
            Quaternion qZ = new Quaternion(new Vector3D(0, 0, 1), angleZ);

            // Combine the rotations (order matters here)
            Quaternion combined = qZ * qY * qX;
            combined.Normalize(); // Normalize to avoid rounding errors

            return combined;
        }


    }
}
