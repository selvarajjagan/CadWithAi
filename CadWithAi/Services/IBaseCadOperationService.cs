using CadWithAi.Models;
using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace CadWithAi.Services
{
    public interface IBaseCadOperationService
    {
        HelixViewport3D Hvp { get; set; }

        [Description("Loads an OBJ file and returns its loading status as message.")]
        Task<string> LoadObjAsync(
           [Description("File path of the OBJ file to load")] string objFilePath);

        [Description("Returns the loaded 3D model as a ModelVisual3D object.")]
        Task<ModelVisual3D> GetLoadedModelAsync();

        [Description("Rotates the loaded OBJ model by a specified angle. Make sure the model is loaded first. To check the model status, use GetLoadedModelAsync method.")]
        Task<string> RotateObjAsync([Description("The axis to rotate around")] Axis axis, [Description("The angle in degrees to rotate the model")] double angle);

        [Description("Sets the HelixViewport3D camera to a predefined standard view by updating the camera's LookDirection and UpDirection. Use this method when the user requests a Front, Back, Left, Right, Top, or Bottom view, optionally rotated by 0, 90, 180, or 270 degrees. Make sure the model is loaded first. To check the model status, use GetLoadedModelAsync method.")]
        Task<string> SetCameraAngleAsync(
            [Description("The standard camera view. Supported values: Front, Back, Left, Right, Top, Bottom (case-insensitive).")] string direction,
            [Description("Clockwise rotation of the selected view. Supported values: 0, 90, 180, or 270 degrees.")] int rotationAngle);

    }
}
