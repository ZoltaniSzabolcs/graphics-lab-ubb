using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
 
namespace Lab2_2
{
    internal static class Program
    {
        private static CameraDescriptor cameraDescriptor = new();

        private static CubeArrangementModel cubeArrangementModel = new();

        private static IWindow window;
        private static IKeyboard primaryKeyboard;

        private static GL Gl;

        private static uint program;

        private static List<GlCube> glCubes;

        private static int FinishRotation = 80;

        private const string ModelMatrixVariableName = "uModel";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";

        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
		layout (location = 1) in vec4 vCol;

        uniform mat4 uModel;
        uniform mat4 uView;
        uniform mat4 uProjection;

		out vec4 outCol;
        
        void main()
        {
			outCol = vCol;
            gl_Position = uProjection*uView*uModel*vec4(vPos.x, vPos.y, vPos.z, 1.0);
        }
        ";


        private static readonly string FragmentShaderSource = @"
        #version 330 core
        out vec4 FragColor;

		in vec4 outCol;

        void main()
        {
            FragColor = outCol;
        }
        ";

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "2 szeminárium";
            windowOptions.Size = new Vector2D<int>(800, 800);

            // on some systems there is no depth buffer by default, so we need to make sure one is created
            windowOptions.PreferredDepthBufferBits = 24;

            window = Window.Create(windowOptions);

            window.Load += Window_Load;
            window.Update += Window_Update;
            window.Render += Window_Render;
            window.Closing += Window_Closing;

            window.Run();
        }

        private static void Window_Load()
        {
            //Console.WriteLine("Load");

            // set up input handling
            IInputContext inputContext = window.CreateInput();
            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;
                primaryKeyboard = keyboard;
            }

            if (primaryKeyboard != null)
            {
                primaryKeyboard.KeyDown += Keyboard_KeyDown;
            }

            for (int i = 0; i < inputContext.Mice.Count; i++)
            {
                inputContext.Mice[i].Cursor.CursorMode = CursorMode.Raw;
                inputContext.Mice[i].MouseMove += OnMouseMove;
            }

            Gl = window.CreateOpenGL();
            Gl.ClearColor(System.Drawing.Color.White);

            SetUpObjects();

            LinkProgram();

            Gl.Enable(EnableCap.CullFace);

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);
        }

        private static unsafe void OnMouseMove(IMouse mouse, Vector2 position)
        {
            cameraDescriptor.LookAtMouse(mouse, position);
        }

        private static void LinkProgram()
        {
            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, VertexShaderSource);
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, FragmentShaderSource);
            Gl.CompileShader(fshader);

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);
            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }
            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);
        }

        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            //Console.WriteLine("Key pressed");
            switch (key)
            {
                case Key.Space:
                    cubeArrangementModel.AnimationEnabeld = !cubeArrangementModel.AnimationEnabeld;
                    break;
                case Key.Q:
                    RotateSide('q');
                    break;
                case Key.W:
                    RotateSide('w');
                    break;
            }
        }

        private static unsafe void RotateSide(char key)
        {
            int nul = -10;
            int x = nul, y = nul, z = nul;
            int clockwise = 0;
            switch (key)
            {
                case 'q':
                    y = 1;
                    break;
                case 'w':
                    y = 1;
                    break;
            }
            if ("q".Contains(key))
            {
                clockwise = 1;
            }
            else
            {
                clockwise = -1;
            }
            List<GlCube> rotCubes = new List<GlCube>();
            foreach(GlCube cube in glCubes)
            {
                if (y != nul && cube.Translation[1] == y && cube.CurrentRotateY == 0)
                {
                    rotCubes.Add(cube);
                }
            }
            foreach(GlCube cube in rotCubes)
            {
                //Console.WriteLine("x=" + cube.Translation[0] + " , y=" + cube.Translation[1] + " , z=" + cube.Translation[2]);
                cube.CurrentRotateY = clockwise;
            }
            
        }

        private static void Window_Update(double deltaTime)
        {
            //Console.WriteLine($"Update after {deltaTime} [s].");
            // multithreaded
            // make sure it is threadsafe
            // NO GL calls

            var moveSpeed = 2.5f * (float)deltaTime;

            if (primaryKeyboard.IsKeyPressed(Key.Up))
            {
                //Move forwards
                cameraDescriptor.MoveForward(moveSpeed);

            }
            if (primaryKeyboard.IsKeyPressed(Key.Down))
            {
                //Move backwards
                cameraDescriptor.MoveBackward(moveSpeed);
            }
            if (primaryKeyboard.IsKeyPressed(Key.Left))
            {
                //Move left
                cameraDescriptor.MoveLeft(moveSpeed);
            }
            if (primaryKeyboard.IsKeyPressed(Key.Right))
            {
                //Move right
                cameraDescriptor.MoveRight(moveSpeed);
            }
            if (primaryKeyboard.IsKeyPressed(Key.Tab))
            {
                //Move up
                cameraDescriptor.MoveUp(moveSpeed);
            }
            if (primaryKeyboard.IsKeyPressed(Key.ShiftLeft))
            {
                //Move down
                cameraDescriptor.MoveDown(moveSpeed);
            }

            cubeArrangementModel.AdvanceTime(deltaTime);
        }

        private static unsafe void Window_Render(double deltaTime)
        {
            //Console.WriteLine($"Render after {deltaTime} [s].");

            // GL here
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.Clear(ClearBufferMask.DepthBufferBit);


            Gl.UseProgram(program);

            SetViewMatrix();
            SetProjectionMatrix();

            DrawGLCubes();

        }
        private static unsafe void DrawGLCubes()
        {
//            Console.WriteLine("Drawing");
            Vector3D<float> zeroPoint = new Vector3D<float>(0.0f, 0.0f, 0.0f);
            foreach (GlCube glCube in glCubes)
            {
                var scale = Matrix4X4.CreateScale((float)cubeArrangementModel.CubeScale);
                Matrix4X4<float> trans = Matrix4X4.CreateTranslation(glCube.Translation[0], glCube.Translation[1], glCube.Translation[2]);
                Matrix4X4<float> workInRotatetingY = Matrix4X4.CreateRotationY((float)((Math.PI / 2f) * ((float)glCube.CurrentRotateY / (float)FinishRotation)), zeroPoint);
                Matrix4X4<float> rotateInAngleY = Matrix4X4.CreateRotationY((float)((Math.PI / 2f) * (float)glCube.RotateAngleY), zeroPoint);
                if (glCube.CurrentRotateY != 0) {
                    if (glCube.CurrentRotateY > 0)
                    {
                        glCube.CurrentRotateY++;
                    }
                    else
                    {
                        glCube.CurrentRotateY--;
                    }
                    if (glCube.CurrentRotateY == FinishRotation + 1 )
                    {
                        glCube.CurrentRotateY = 0;
                        glCube.RotateAngleY += 1;
                    }
                    if(glCube.CurrentRotateY == -1 * (FinishRotation + 1))
                    {
                        glCube.CurrentRotateY = 0;
                        glCube.RotateAngleY += -1;
                    }

                }
                Matrix4X4<float> modelMatrix = scale * trans * rotateInAngleY * workInRotatetingY;
                //Matrix4X4<float> modelMatrix = scale * rotGlobY * trans;
                SetModelMatrix(modelMatrix);
                Gl.BindVertexArray(glCube.Vao);
                Gl.DrawElements(GLEnum.Triangles, glCube.IndexArrayLength, GLEnum.UnsignedInt, null);
                Gl.BindVertexArray(0);
            }
        }

        private static unsafe void SetModelMatrix(Matrix4X4<float> modelMatrix)
        {

            int location = Gl.GetUniformLocation(program, ModelMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{ModelMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&modelMatrix);
            CheckError();
        }

        private static unsafe void SetUpObjects()
        {
            glCubes = new List<GlCube>();

            float[] TOP_COLOR = [0.95f, 0.95f, 0.95f, 1.0f];   // TOP
            float[] FRONT_COLOR = [1.0f, 0.0f, 0.0f, 1.0f];      // FRONT
            float[] LEFT_COLOR = [0.0f, 1.0f, 0.0f, 1.0f];      // LEFT
            float[] DOWN_COLOR = [1.0f, 1.0f, 0.0f, 1.0f];      // DOWN
            float[] BACK_COLOR = [1.0f, 0.6f, 0.0f, 1.0f];      // BACK
            float[] RIGHT_COLOR = [0.3f, 0.52f, 0.91f, 1.0f];      // RIGHT
            float[] BLACK_COLOR = [0f, 0f, 0f, 1.0f];

//            GlCube glCube = GlCube.CreateCubeWithFaceColors(Gl, TOP_COLOR, FRONT_COLOR, LEFT_COLOR, DOWN_COLOR, BACK_COLOR, RIGHT_COLOR, translation);


            //-------------------------------------------------------------------------------------------------------------------
            //------------------------------------------------------ BOTTOM -----------------------------------------------------
            //-------------------------------------------------------------------------------------------------------------------


            //------------------------------------------------ BOTTOM FRONT LEFT ------------------------------------------------
            float[] translation = [-1f, -1f, 1f];
            glCubes.Add(GlCube.CreateCubeWithFaceColors(Gl, BLACK_COLOR, FRONT_COLOR, LEFT_COLOR, DOWN_COLOR, BLACK_COLOR, BLACK_COLOR, translation));

            //------------------------------------------------ BOTTOM FRONT MIDDLE ------------------------------------------------
            translation = [0f, -1f, 1f];
            glCubes.Add(GlCube.CreateCubeWithFaceColors(Gl, BLACK_COLOR, FRONT_COLOR, BLACK_COLOR, DOWN_COLOR, BLACK_COLOR, BLACK_COLOR, translation));

            //------------------------------------------------ BOTTOM FRONT RIGHT ------------------------------------------------
            translation = [1f, -1f, 1f];
            glCubes.Add(GlCube.CreateCubeWithFaceColors(Gl, BLACK_COLOR, FRONT_COLOR, BLACK_COLOR, DOWN_COLOR, BLACK_COLOR, RIGHT_COLOR, translation));

            //------------------------------------------------ BOTTOM MIDDLE LEFT ------------------------------------------------
            translation = [-1f, -1f, 0f];
            glCubes.Add(GlCube.CreateCubeWithFaceColors(Gl, BLACK_COLOR, BLACK_COLOR, LEFT_COLOR, DOWN_COLOR, BLACK_COLOR, BLACK_COLOR, translation));

            //------------------------------------------------ BOTTOM MIDDLE MIDDLE ------------------------------------------------
            translation = [0f, -1f, 0f];
            glCubes.Add(GlCube.CreateCubeWithFaceColors(Gl, BLACK_COLOR, BLACK_COLOR, BLACK_COLOR, DOWN_COLOR, BLACK_COLOR, BLACK_COLOR, translation));

            //------------------------------------------------ BOTTOM MIDDLE RIGHT ------------------------------------------------
            translation = [1f, -1f, 0f];
            glCubes.Add(GlCube.CreateCubeWithFaceColors(Gl, BLACK_COLOR, BLACK_COLOR, BLACK_COLOR, DOWN_COLOR, BLACK_COLOR, RIGHT_COLOR, translation));

            //------------------------------------------------ BOTTOM BACK LEFT ------------------------------------------------
            translation = [-1f, -1f, -1f];
            glCubes.Add(GlCube.CreateCubeWithFaceColors(Gl, BLACK_COLOR, BLACK_COLOR, LEFT_COLOR, DOWN_COLOR, BACK_COLOR, BLACK_COLOR, translation));

            //------------------------------------------------ BOTTOM BACK MIDDLE ------------------------------------------------
            translation = [0f, -1f, -1f];
            glCubes.Add(GlCube.CreateCubeWithFaceColors(Gl, BLACK_COLOR, BLACK_COLOR, BLACK_COLOR, DOWN_COLOR, BACK_COLOR, BLACK_COLOR, translation));

            //------------------------------------------------ BOTTOM BACK RIGHT ------------------------------------------------
            translation = [1f, -1f, -1f];
            glCubes.Add(GlCube.CreateCubeWithFaceColors(Gl, BLACK_COLOR, BLACK_COLOR, BLACK_COLOR, DOWN_COLOR, BACK_COLOR, RIGHT_COLOR, translation));


            //-------------------------------------------------------------------------------------------------------------------
            //------------------------------------------------------ MIDDLE -----------------------------------------------------
            //-------------------------------------------------------------------------------------------------------------------


            //------------------------------------------------ MIDDLE FRONT LEFT ------------------------------------------------
            translation = [-1f, 0f, 1f];
            glCubes.Add(GlCube.CreateCubeWithFaceColors(Gl, BLACK_COLOR, FRONT_COLOR, LEFT_COLOR, BLACK_COLOR, BLACK_COLOR, BLACK_COLOR, translation));

            //------------------------------------------------ MIDDLE FRONT MIDDLE ------------------------------------------------
            translation = [0f, 0f, 1f];
            glCubes.Add(GlCube.CreateCubeWithFaceColors(Gl, BLACK_COLOR, FRONT_COLOR, BLACK_COLOR, BLACK_COLOR, BLACK_COLOR, BLACK_COLOR, translation));

            //------------------------------------------------ MIDDLE FRONT RIGHT ------------------------------------------------
            translation = [1f, 0f, 1f];
            glCubes.Add(GlCube.CreateCubeWithFaceColors(Gl, BLACK_COLOR, FRONT_COLOR, BLACK_COLOR, BLACK_COLOR, BLACK_COLOR, RIGHT_COLOR, translation));

            //------------------------------------------------ MIDDLE MIDDLE LEFT ------------------------------------------------
            translation = [-1f, 0f, 0f];
            glCubes.Add(GlCube.CreateCubeWithFaceColors(Gl, BLACK_COLOR, BLACK_COLOR, LEFT_COLOR, BLACK_COLOR, BLACK_COLOR, BLACK_COLOR, translation));

            //------------------------------------------------ MIDDLE MIDDLE RIGHT ------------------------------------------------
            translation = [1f, 0f, 0f];
            glCubes.Add(GlCube.CreateCubeWithFaceColors(Gl, BLACK_COLOR, BLACK_COLOR, BLACK_COLOR, BLACK_COLOR, BLACK_COLOR, RIGHT_COLOR, translation));

            //------------------------------------------------ MIDDLE BACK LEFT ------------------------------------------------
            translation = [-1f, 0f, -1f];
            glCubes.Add(GlCube.CreateCubeWithFaceColors(Gl, BLACK_COLOR, BLACK_COLOR, LEFT_COLOR, BLACK_COLOR, BACK_COLOR, BLACK_COLOR, translation));

            //------------------------------------------------ MIDDLE BACK MIDDLE ------------------------------------------------
            translation = [0f, 0f, -1f];
            glCubes.Add(GlCube.CreateCubeWithFaceColors(Gl, BLACK_COLOR, BLACK_COLOR, BLACK_COLOR, BLACK_COLOR, BACK_COLOR, BLACK_COLOR, translation));

            //------------------------------------------------ MIDDLE BACK RIGHT ------------------------------------------------
            translation = [1f, 0f, -1f];
            glCubes.Add(GlCube.CreateCubeWithFaceColors(Gl, BLACK_COLOR, BLACK_COLOR, BLACK_COLOR, BLACK_COLOR, BACK_COLOR, RIGHT_COLOR, translation));


            //-------------------------------------------------------------------------------------------------------------------
            //------------------------------------------------------ TOP---------------------------------------------------------
            //-------------------------------------------------------------------------------------------------------------------


            //------------------------------------------------ TOP FRONT LEFT ------------------------------------------------
            translation = [-1f, 1f, 1f];
            glCubes.Add(GlCube.CreateCubeWithFaceColors(Gl, TOP_COLOR, FRONT_COLOR, LEFT_COLOR, BLACK_COLOR, BLACK_COLOR, BLACK_COLOR, translation));

            //------------------------------------------------ TOP FRONT MIDDLE ------------------------------------------------
            translation = [0f, 1f, 1f];
            glCubes.Add(GlCube.CreateCubeWithFaceColors(Gl, TOP_COLOR, FRONT_COLOR, BLACK_COLOR, BLACK_COLOR, BLACK_COLOR, BLACK_COLOR, translation));

            //------------------------------------------------ TOP  FRONT RIGHT ------------------------------------------------
            translation = [1f, 1f, 1f];
            glCubes.Add(GlCube.CreateCubeWithFaceColors(Gl, TOP_COLOR, FRONT_COLOR, BLACK_COLOR, BLACK_COLOR, BLACK_COLOR, RIGHT_COLOR, translation));

            //------------------------------------------------ TOP  MIDDLE LEFT ------------------------------------------------
            translation = [-1f, 1f, 0f];
            glCubes.Add(GlCube.CreateCubeWithFaceColors(Gl, TOP_COLOR, BLACK_COLOR, LEFT_COLOR, BLACK_COLOR, BLACK_COLOR, BLACK_COLOR, translation));

            //------------------------------------------------ TOP MIDDLE MIDDLE ------------------------------------------------
            translation = [0f, 1f, 0f];
            glCubes.Add(GlCube.CreateCubeWithFaceColors(Gl, TOP_COLOR, BLACK_COLOR, BLACK_COLOR, BLACK_COLOR, BLACK_COLOR, BLACK_COLOR, translation));

            //------------------------------------------------ TOP  MIDDLE RIGHT ------------------------------------------------
            translation = [1f, 1f, 0f];
            glCubes.Add(GlCube.CreateCubeWithFaceColors(Gl, TOP_COLOR, BLACK_COLOR, BLACK_COLOR, BLACK_COLOR, BLACK_COLOR, RIGHT_COLOR, translation));

            //------------------------------------------------ TOP  BACK LEFT ------------------------------------------------
            translation = [-1f, 1f, -1f];
            glCubes.Add(GlCube.CreateCubeWithFaceColors(Gl, TOP_COLOR, BLACK_COLOR, LEFT_COLOR, BLACK_COLOR, BACK_COLOR, BLACK_COLOR, translation));

            //------------------------------------------------ TOP BACK MIDDLE ------------------------------------------------
            translation = [0f, 1f, -1f];
            glCubes.Add(GlCube.CreateCubeWithFaceColors(Gl, TOP_COLOR, BLACK_COLOR, BLACK_COLOR, BLACK_COLOR, BACK_COLOR, BLACK_COLOR, translation));

            //------------------------------------------------ TOP  BACK RIGHT ------------------------------------------------
            translation = [1f, 1f, -1f];
            glCubes.Add(GlCube.CreateCubeWithFaceColors(Gl, TOP_COLOR, BLACK_COLOR, BLACK_COLOR, BLACK_COLOR, BACK_COLOR, RIGHT_COLOR, translation));
        }



        private static void Window_Closing()
        {
            foreach (GlCube glCube in glCubes)
            {
                glCube.ReleaseGlCube();
            }
        }

        private static unsafe void SetProjectionMatrix()
        {
            var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)Math.PI / 4f, 1024f / 768f, 0.1f, 100);
            int location = Gl.GetUniformLocation(program, ProjectionMatrixVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&projectionMatrix);
            CheckError();
        }

        private static unsafe void SetViewMatrix()
        {
            var viewMatrix = cameraDescriptor.getView();
            int location = Gl.GetUniformLocation(program, ViewMatrixVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&viewMatrix);
            CheckError();
        }

        public static void CheckError()
        {
            var error = (ErrorCode)Gl.GetError();
            if (error != ErrorCode.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }
    }
}