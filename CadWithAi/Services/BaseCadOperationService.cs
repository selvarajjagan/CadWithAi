using CadWithAi.Models;
using HelixToolkit.Wpf;
using System.Windows.Media.Media3D;

namespace CadWithAi.Services;

public class BaseCadOperationService : IBaseCadOperationService
{
    public HelixViewport3D? Hvp { get; set; }

    public Task<ModelVisual3D?> GetLoadedModelAsync()
    {
        if (Hvp is null)
            return Task.FromResult<ModelVisual3D?>(null);

        ModelVisual3D? model = Hvp.Children.OfType<ModelVisual3D>().FirstOrDefault();
        return Task.FromResult(model);
    }

    public Task<bool> IsModelLoadedAsync()
    {
        return Task.FromResult(Hvp?.Children.OfType<ModelVisual3D>().Any() == true);
    }

    public Task<string> LoadObjAsync(string objFilePath)
    {
        if (string.IsNullOrWhiteSpace(objFilePath))
            return Task.FromResult("Error: OBJ file path is empty.");

        if (!File.Exists(objFilePath))
            return Task.FromResult($"Error: OBJ file '{objFilePath}' does not exist.");

        if (Hvp is null)
            return Task.FromResult("Error: HelixViewport3D is not initialized.");

        try
        {
            ObjReader objReader = new();
            Model3DGroup model3DGroup = objReader.Read(objFilePath);

            if (model3DGroup is null)
                return Task.FromResult("Error: The OBJ file did not contain a readable 3D model.");

            ModelVisual3D modelVisual3D = new()
            {
                Content = model3DGroup,
                Transform = CreateTransform3DGroup()
            };

            Hvp.Children.Clear();
            Hvp.Children.Add(new SunLight());
            Hvp.Children.Add(modelVisual3D);
            Hvp.ZoomExtents();

            return Task.FromResult("OBJ model loaded successfully.");
        }
        catch (Exception ex)
        {
            return Task.FromResult($"Error loading OBJ model: {ex.Message}");
        }
    }

    public async Task<string> RotateObjAsync(Axis axis, double angle)
    {
        if (Hvp is null)
            return "Error: HelixViewport3D is not initialized.";

        if (!await IsModelLoadedAsync())
            return "Error: No 3D model is currently loaded.";

        ApplyRotationToViewport(axis, angle);
        return $"Model rotated {angle:0.##} degrees around the {axis} axis.";
    }

    public Task<string> SetCameraAngleAsync(string direction, int rotationAngle)
    {
        if (Hvp is null)
            return Task.FromResult("Error: HelixViewport3D is not initialized.");

        if (!Enum.TryParse(direction, true, out CameraDirection cameraDirection))
            return Task.FromResult("Error: Camera direction must be Front, Back, Left, Right, Top, or Bottom.");

        if (rotationAngle is not (0 or 90 or 180 or 270))
            return Task.FromResult("Error: Camera rotation must be 0, 90, 180, or 270 degrees.");

        if (!Hvp.Children.OfType<ModelVisual3D>().Any())
            return Task.FromResult("Error: No 3D model is currently loaded.");

        Vector3D lookDirection;
        Vector3D upDirection;

        switch (cameraDirection)
        {
            case CameraDirection.Front:
                lookDirection = new Vector3D(0, 0, -1);
                upDirection = new Vector3D(0, 1, 0);
                break;
            case CameraDirection.Back:
                lookDirection = new Vector3D(0, 0, 1);
                upDirection = new Vector3D(0, 1, 0);
                break;
            case CameraDirection.Left:
                lookDirection = new Vector3D(1, 0, 0);
                upDirection = new Vector3D(0, 1, 0);
                break;
            case CameraDirection.Right:
                lookDirection = new Vector3D(-1, 0, 0);
                upDirection = new Vector3D(0, 1, 0);
                break;
            case CameraDirection.Top:
                lookDirection = new Vector3D(0, -1, 0);
                upDirection = new Vector3D(0, 0, -1);
                break;
            case CameraDirection.Bottom:
                lookDirection = new Vector3D(0, 1, 0);
                upDirection = new Vector3D(0, 0, 1);
                break;
            default:
                return Task.FromResult("Error: Unsupported camera direction.");
        }

        if (rotationAngle != 0)
        {
            Quaternion rotation = new(lookDirection, rotationAngle);
            Matrix3D matrix = Matrix3D.Identity;
            matrix.Rotate(rotation);
            upDirection = matrix.Transform(upDirection);
        }

        Rect3D bounds = Hvp.Children
            .OfType<ModelVisual3D>()
            .Select(x => x.Content?.Bounds ?? Rect3D.Empty)
            .Where(x => !x.IsEmpty)
            .Aggregate(Rect3D.Empty, UnionBounds);

        Point3D target = bounds.IsEmpty
            ? new Point3D()
            : bounds.Location + new Vector3D(bounds.SizeX / 2, bounds.SizeY / 2, bounds.SizeZ / 2);

        double distance = Math.Max(Math.Max(bounds.SizeX, bounds.SizeY), Math.Max(bounds.SizeZ, 1)) * 2.5;
        Point3D position = target - lookDirection * distance;

        Hvp.SetView(position, lookDirection, upDirection, 0);
        return Task.FromResult($"Camera set to {cameraDirection} view with {rotationAngle} degree rotation.");
    }

    private static Rect3D UnionBounds(Rect3D first, Rect3D second)
    {
        if (first.IsEmpty) return second;
        if (second.IsEmpty) return first;
        first.Union(second);
        return first;
    }

    private void ApplyRotationToViewport(Axis axis, double angle)
    {
        Vector3D axisVector = axis switch
        {
            Axis.X => new Vector3D(1, 0, 0),
            Axis.Y => new Vector3D(0, 1, 0),
            Axis.Z => new Vector3D(0, 0, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(axis))
        };

        RotateTransform3D rotation = new(new AxisAngleRotation3D(axisVector, angle));

        foreach (ModelVisual3D model in Hvp!.Children.OfType<ModelVisual3D>())
        {
            if (model.Transform is Transform3DGroup group)
            {
                if (group.Children.Count > 1)
                    group.Children[1] = rotation;
                else
                    group.Children.Add(rotation);
            }
            else
            {
                model.Transform = rotation;
            }
        }
    }

    private static Transform3DGroup CreateTransform3DGroup(
        double ttx = 0, double tty = 0, double ttz = 0,
        double stx = 1, double sty = 1, double stz = 1,
        double rtx = 0, double rty = 0, double rtz = 0)
    {
        Transform3DGroup group = new();
        group.Children.Add(new ScaleTransform3D(stx, sty, stz));

        Quaternion qX = new(new Vector3D(1, 0, 0), rtx);
        Quaternion qY = new(new Vector3D(0, 1, 0), rty);
        Quaternion qZ = new(new Vector3D(0, 0, 1), rtz);
        Quaternion combined = qZ * qY * qX;
        combined.Normalize();

        group.Children.Add(new RotateTransform3D(new QuaternionRotation3D(combined)));
        group.Children.Add(new TranslateTransform3D(ttx, tty, ttz));
        return group;
    }

    private enum CameraDirection
    {
        Front,
        Back,
        Left,
        Right,
        Top,
        Bottom
    }
}
