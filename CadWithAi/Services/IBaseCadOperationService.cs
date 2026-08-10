using CadWithAi.AI;
using CadWithAi.Models;
using HelixToolkit.Wpf;
using System.ComponentModel;
using System.Windows.Media.Media3D;

namespace CadWithAi.Services;

[AIToolService("3D CAD scene operations such as loading OBJ models, checking model state, rotating the model, and changing standard camera views.")]
public interface IBaseCadOperationService
{
    // UI dependency. This property is not exposed as an AI tool.
    HelixViewport3D? Hvp { get; set; }

    [Description("Load an OBJ 3D model into the current CAD scene. Use when the user asks to open, import, load, or display an OBJ file.")]
    Task<string> LoadObjAsync(
        [Description("Full path of the OBJ file to load, including the .obj extension.")] string objFilePath);

    [AIToolIgnore]
    Task<ModelVisual3D?> GetLoadedModelAsync();

    [Description("Check whether a 3D model is currently loaded in the CAD scene. Use when the user asks whether a model is loaded or before an operation that requires a loaded model.")]
    Task<bool> IsModelLoadedAsync();

    [Description("Rotate the currently loaded 3D model around the X, Y, or Z axis by the specified angle in degrees. Use for requests such as rotate, turn, tilt, or spin the model.")]
    Task<string> RotateObjAsync(
        [Description("Axis to rotate around. Valid values are X, Y, or Z.")] Axis axis,
        [Description("Rotation angle in degrees. Positive and negative values are supported.")] double angle);

    [Description("Set the CAD camera to a standard view. Use when the user requests Front, Back, Left, Right, Top, or Bottom view. An optional clockwise rotation of 0, 90, 180, or 270 degrees can be applied to that view.")]
    Task<string> SetCameraAngleAsync(
        [Description("Standard camera direction: Front, Back, Left, Right, Top, or Bottom.")] string direction,
        [Description("Clockwise rotation of the selected camera view. Use 0 when no additional rotation is requested. Supported values are 0, 90, 180, and 270.")] int rotationAngle);
}
