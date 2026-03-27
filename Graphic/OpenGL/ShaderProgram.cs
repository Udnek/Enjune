using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Enjune.Graphic.OpenGL;

public class ShaderProgram
{
    private readonly int _program;
    private readonly int _vertexShader;
    private readonly int _fragmentShader;

    public Uniform Model { get; private set; } = null!;
    public Uniform View { get; private set; } = null!;
    public Uniform Projection { get; private set; } = null!;

    public ShaderProgram()
    {
        _program = GL.CreateProgram();

        _fragmentShader = InitShader(ShaderType.FragmentShader, FragmentContent);
        _vertexShader = InitShader(ShaderType.VertexShader, VertexContent);

        GL.BindFragDataLocation(_program, 0, "fragColor");
        GL.LinkProgram(_program);

        // check init
        GL.GetProgram(_program, GetProgramParameterName.LinkStatus, out int linkStatus);
        if (linkStatus != (int)All.True)
        {
            string infoLog = GL.GetProgramInfoLog(_program);
            throw new Exception($"Shader program linking failed: {infoLog}");
        }
        // use
        GL.UseProgram(_program);

        // init
        InitAttributes();
        InitUniforms();
    }

    private int InitShader(ShaderType type, string source)
    {
        int shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);

        // check compile
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int compileStatus);
        if (compileStatus != (int)All.True)
        {
            var infoLog = GL.GetShaderInfoLog(shader);
            throw new Exception($"Shader compilation failed ({type}): {infoLog}");
        }

        GL.AttachShader(_program, shader);
        return shader;
    }

    private void InitUniforms()
    {
        Model = new Uniform(_program, "model");
        View = new Uniform(_program, "view");
        Projection = new Uniform(_program, "projection");

        // Set default projection matrix
        var defaultProjection = Matrix4.CreatePerspectiveFieldOfView(
            MathF.PI / 2, 1.0f, 0.1f, 1000.0f);
        Projection.SetValue(defaultProjection);
    }

    private void InitAttributes()
    {
        var attributes = new Attributes();
        attributes.Add(new Attribute("position", 3));
        attributes.Add(new Attribute("color", 4));
        attributes.Compile(_program);
    }

    public void Destroy()
    {
        GL.DeleteShader(_fragmentShader);
        GL.DeleteShader(_vertexShader);
        GL.DeleteProgram(_program);
    }

    // Nested classes

    public class Attribute(string name, int elements)
    {
        public readonly string Name = name;
        public readonly int Elements = elements;
    }

    public class Attributes
    {
        private readonly List<Attribute> _attributes = [];

        public void Add(Attribute attribute) => _attributes.Add(attribute);

        public void Compile(int program)
        {
            int stride = _attributes.Sum(a => a.Elements) * sizeof(float);
            int offset = 0;

            foreach (var attr in _attributes) 
            {
                int location = GL.GetAttribLocation(program, attr.Name);
                GL.EnableVertexAttribArray(location);
                GL.VertexAttribPointer(
                    location,
                    attr.Elements,
                    VertexAttribPointerType.Float,
                    false,
                    stride,
                    offset);
                offset += attr.Elements * sizeof(float);
            }
        }
    }

    public class Uniform
    {
        private readonly int _location;

        public Uniform(int program, string name)
        {
            _location = GL.GetUniformLocation(program, name);
            SetValue(Matrix4.Identity);
        }

        public void SetValue(Matrix4 matrix)
        {
            GL.UniformMatrix4(_location, false, ref matrix);
        }
    }
        
    private const string VertexContent = """

                                                     #version 150 core
                                                     
                                                     in vec3 position;
                                                     in vec4 color;
                                                     out vec4 vertexColor;
                                                     
                                                     uniform mat4 model;
                                                     uniform mat4 view;
                                                     uniform mat4 projection;
                                                     
                                                     void main() {
                                                         vertexColor = color;
                                                         mat4 mvp = projection * view * model;
                                                         gl_Position = mvp * vec4(position, 1.0);
                                                     }
                                                 
                                         """;

    private const string FragmentContent = """

                                           #version 150 core
                                           
                                           in vec4 vertexColor;
                                           
                                           out vec4 fragColor;
                                           
                                           void main() {
                                               fragColor = vertexColor;
                                           }
                                                   
                                           """;
}