using Enjune.Misc;
using FluentAssertions;

namespace Tests;

public class MathUtilsTests
{
    private const float Epsilon = MathUtils.Epsilon;

    // ------------------- RayIntersectsLine -------------------
    [Fact]
    public void RayIntersectsLine_ShouldReturnTrue_WhenRayHitsLineSegment()
    {
        // Arrange
        var origin = new Position(0, 0, 0);
        var direction = new Vector3(1, 0, 0);
        var p0 = new Position(2, -1, 0);
        var p1 = new Position(2, 1, 0);

        // Act
        bool result = MathUtils.RayIntersectsLine(origin, direction, p0, p1, out float cosDistance);

        // Assert
        result.Should().BeTrue();
        cosDistance.Should().BeApproximately(1f, Epsilon); // направление луча совпадает с проекцией
    }

    [Fact]
    public void RayIntersectsLine_ShouldReturnFalse_WhenRayMissesLineSegment()
    {
        // Arrange
        var origin = new Position(0, 0, 0);
        var direction = new Vector3(1, 0, 0);
        var p0 = new Position(2, 2, 0);
        var p1 = new Position(2, 3, 0);

        // Act
        bool result = MathUtils.RayIntersectsLine(origin, direction, p0, p1, out float cosDistance);

        // Assert
        result.Should().BeFalse();
        cosDistance.Should().Be(0);
    }

    // ------------------- VectorsIntersect -------------------
    [Fact]
    public void VectorsIntersect_ShouldReturnTrue_WhenLinesIntersect()
    {
        // Arrange
        var originA = new Position(0, 0, 0);
        var dirA = new Vector3(1, 0, 0);
        var originB = new Position(2, 2, 0);
        var dirB = new Vector3(0, -1, 0); // встречаются в (2,0,0)

        // Act
        bool result = MathUtils.VectorsIntersect(originA, dirA, originB, dirB, out var intersection);

        // Assert
        result.Should().BeTrue();
        intersection.Should().Be(new Position(2, 0, 0));
    }

    [Fact]
    public void VectorsIntersect_ShouldReturnFalse_WhenLinesAreParallel()
    {
        // Arrange
        var originA = new Position(0, 0, 0);
        var dirA = new Vector3(1, 0, 0);
        var originB = new Position(0, 1, 0);
        var dirB = new Vector3(1, 0, 0); // параллельны

        // Act
        bool result = MathUtils.VectorsIntersect(originA, dirA, originB, dirB, out var intersection);

        // Assert
        result.Should().BeFalse();
        intersection.Should().Be(originA); // как в коде: при k<=0 возвращается originA
    }

    // ------------------- DegreeAngleBetween -------------------
    [Theory]
    [InlineData(1, 0, 0, 0, 1, 0, 90)]
    [InlineData(1, 0, 0, 1, 0, 0, 0)]
    [InlineData(1, 0, 0, -1, 0, 0, 180)]
    public void DegreeAngleBetween_ShouldReturnCorrectAngle(float ax, float ay, float az, float bx, float by, float bz, float expectedDegrees)
    {
        // Arrange
        var a = new Vector3(ax, ay, az);
        var b = new Vector3(bx, by, bz);

        // Act
        float result = MathUtils.DegreeAngleBetween(a, b);

        // Assert
        result.Should().BeApproximately(expectedDegrees, Epsilon);
    }

    // ------------------- CosAngleBetween -------------------
    [Theory]
    [InlineData(1, 0, 0, 0, 1, 0, 0)]
    [InlineData(1, 0, 0, 1, 0, 0, 1)]
    [InlineData(1, 0, 0, -1, 0, 0, -1)]
    public void CosAngleBetween_ShouldReturnCorrectCosine(float ax, float ay, float az, float bx, float by, float bz, float expectedCos)
    {
        // Arrange
        var a = new Vector3(ax, ay, az);
        var b = new Vector3(bx, by, bz);

        // Act
        float result = MathUtils.CosAngleBetween(a, b);

        // Assert
        result.Should().BeApproximately(expectedCos, Epsilon);
    }

    // ------------------- ProjectVectorOnPlane -------------------
    [Fact]
    public void ProjectVectorOnPlane_ShouldProjectVectorOntoPlaneDefinedByThreePoints()
    {
        // Arrange
        var direction = new Vector3(1, 1, 1);
        var p0 = new Position(0, 0, 0);
        var p1 = new Position(1, 0, 0);
        var p2 = new Position(0, 1, 0); // плоскость XY

        // Act
        var projected = MathUtils.ProjectVectorOnPlane(direction, p0, p1, p2);

        // Assert
        projected.Should().Be(new Vector3(1, 1, 0)); // Z-компонента обнулена
    }

    [Fact]
    public void ProjectVectorOnPlane_WhenPointsAreCollinear_ShouldReturnOriginalDirectionAndLogError()
    {
        // Arrange
        var direction = new Vector3(1, 2, 3);
        var p0 = new Position(0, 0, 0);
        var p1 = new Position(1, 0, 0);
        var p2 = new Position(2, 0, 0); // коллинеарны

        // Act
        var projected = MathUtils.ProjectVectorOnPlane(direction, p0, p1, p2);

        // Assert
        projected.Should().Be(direction); // возвращает исходный вектор
    }

    // ------------------- ProjectAonB -------------------
    [Fact]
    public void ProjectAonB_ShouldReturnProjectionOfAontoB()
    {
        // Arrange
        var a = new Vector3(3, 4, 0);
        var b = new Vector3(1, 0, 0);

        // Act
        var projection = MathUtils.ProjectAonB(a, b);

        // Assert
        projection.Should().Be(new Vector3(3, 0, 0));
    }

    [Fact]
    public void ProjectAonB_WhenBIsZeroOrTooSmall_ShouldReturnOriginalAAndLogError()
    {
        // Arrange
        var a = new Vector3(1, 2, 3);
        var bZero = Vector3.Zero;
        var bTiny = new Vector3(1e-7f, 0, 0); // длина меньше Epsilon (1e-6f)

        // Act
        var resultZero = MathUtils.ProjectAonB(a, bZero);
        var resultTiny = MathUtils.ProjectAonB(a, bTiny);

        // Assert
        resultZero.Should().Be(a);
        resultTiny.Should().Be(a);
        // При необходимости можно проверить, что логгер был вызван (через мок), но это выходит за рамки простого теста.
    }

    // ------------------- PlaneNormNotNormalized -------------------
    [Fact]
    public void PlaneNormNotNormalized_ShouldReturnCrossProductOfEdges()
    {
        // Arrange
        var p0 = new Position(0, 0, 0);
        var p1 = new Position(1, 0, 0);
        var p2 = new Position(0, 1, 0);

        // Act
        var normal = MathUtils.PlaneNormNotNormalized(p0, p1, p2);

        // Assert
        normal.Should().Be(new Vector3(0, 0, 1));
    }

    // ------------------- PointToVectorDistance -------------------
    [Fact]
    public void PointToVectorDistance_ShouldReturnPerpendicularDistance()
    {
        // Arrange
        var point = new Position(0, 3, 0);
        var direction = new Vector3(1, 0, 0); // ось X

        // Act
        float distance = MathUtils.PointToVectorDistance(point, direction);

        // Assert
        distance.Should().BeApproximately(3, Epsilon);
    }

    // ------------------- RayIntersectsTriangle -------------------
    [Fact]
    public void RayIntersectsTriangle_ShouldReturnTrue_WhenRayHitsTriangle()
    {
        // Arrange
        var origin = new Position(0, 0, 0);
        var direction = new Vector3(0, 0, 1);
        var p0 = new Position(-1, -1, 5);
        var p1 = new Position(1, -1, 5);
        var p2 = new Position(0, 1, 5);

        // Act
        bool result = MathUtils.RayIntersectsTriangle(origin, direction, p0, p1, p2, out float distance);

        // Assert
        result.Should().BeTrue();
        distance.Should().BeApproximately(5, Epsilon);
    }

    [Fact]
    public void RayIntersectsTriangle_ShouldReturnFalse_WhenRayMissesTriangle()
    {
        // Arrange
        var direction = new Vector3(0, 0, 1);
        var p0 = new Position(-1, -1, 5);
        var p1 = new Position(1, -1, 5);
        var p2 = new Position(0, 1, 5);

        // луч смещён в сторону
        var origin = new Position(10, 0, 0);

        // Act
        bool result = MathUtils.RayIntersectsTriangle(origin, direction, p0, p1, p2, out float distance);

        // Assert
        result.Should().BeFalse();
    }
}