using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

/* Zoltani Szabolcs
 * 524/2
 * zsim2317
 * */

namespace Szeminarium1_24_02_17_2
{
    internal static class Program
    {
        private static CameraDescriptor cameraDescriptor = new();

        private static CubeArrangementModel cubeArrangementModel = new();

        private static IWindow window;
        private static IInputContext input;
        private static IKeyboard primaryKeyboard;
        

        private static GL Gl;

        private static uint program;

        private static List<GlCube> glCubes;

        private static int DoneRotateTic = 50;
        private static int OriginalDoneRotateTic = 50;
        private static int FasterDoneRotateTic = 15;

        private static bool isRotating = false;
        private static bool isScramble = false;
        private static float epsi = 0.01f;
        private static Queue<char> queueRotation = new Queue<char>();
        private const int ROTATIONSTOSCRAMLE = 30;
        private static bool isDone = false;

        private static float[] TOP_COLOR = [0.95f, 0.95f, 0.95f, 1.0f];   // TOP
        private static float[] FRONT_COLOR = [1.0f, 0.0f, 0.0f, 1.0f];      // FRONT
        private static float[] LEFT_COLOR = [0.0f, 1.0f, 0.0f, 1.0f];      // LEFT
        private static float[] DOWN_COLOR = [1.0f, 1.0f, 0.0f, 1.0f];      // DOWN
        private static float[] BACK_COLOR = [1.0f, 0.6f, 0.0f, 1.0f];      // BACK
        private static float[] RIGHT_COLOR = [0.3f, 0.52f, 0.91f, 1.0f];      // RIGHT
        private static float[] BLACK_COLOR = [0f, 0f, 0f, 1.0f];
        private static float[] TRANSLATION = [0.0f, 0.0f, 0.0f];

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
            windowOptions.Size = new Vector2D<int>(1200, 1200);

            // on some systems there is no depth buffer by default, so we need to make sure one is created
            windowOptions.PreferredDepthBufferBits = 24;

            window = Window.Create(windowOptions);

            window.Load += Window_Load;
            window.Update += Window_Update;
            window.Render += Window_Render;
            window.Closing += Window_Closing;

            window.Run();

            window.Dispose();
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
            Console.WriteLine($"Keyboars count: {inputContext.Keyboards.Count} \nIf the keyboard is not working try unplugging some");
            if (primaryKeyboard != null)
            {
                primaryKeyboard.KeyDown += Keyboard_KeyDown;
            }

            for (int i = 0; i < inputContext.Mice.Count; i++)
            {
                inputContext.Mice[i].Cursor.CursorMode = CursorMode.Raw;
                inputContext.Mice[i].MouseMove += OnMouseMove;
                inputContext.Mice[i].Scroll += OnMouseWheel;
            }

            Gl = window.CreateOpenGL();
            Gl.ClearColor(System.Drawing.Color.White);

            SetUpObjects();

            LinkProgram();

            Gl.Enable(EnableCap.CullFace);

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);
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
                case Key.Q:
                    RotateSide('q');
                    break;
                case Key.A:
                    RotateSide('a');
                    break;
                case Key.Z:
                    RotateSide('z');
                    break;
                case Key.W:
                    RotateSide('w');
                    break;
                case Key.S:
                    RotateSide('s');
                    break;
                case Key.X:
                    RotateSide('x');
                    break;
                case Key.U:
                    RotateSide('u');
                    break;
                case Key.I:
                    RotateSide('i');
                    break;
                case Key.O:
                    RotateSide('o');
                    break;
                case Key.J:
                    RotateSide('j');
                    break;
                case Key.K:
                    RotateSide('k');
                    break;
                case Key.L:
                    RotateSide('l');
                    break;
                case Key.F:
                    RotateSide('f');
                    break;
                case Key.G:
                    RotateSide('g');
                    break;
                case Key.H:
                    RotateSide('h');
                    break;
                case Key.V:
                    RotateSide('v');
                    break;
                case Key.B:
                    RotateSide('b');
                    break;
                case Key.N:
                    RotateSide('n');
                    break;
                case Key.R:
                    Scramble(ROTATIONSTOSCRAMLE);
                    break;
                case Key.Enter:
                    EndPulse();
                    break;
            }
        }

        private static unsafe void OnMouseMove(IMouse mouse, Vector2 position)
        {
            cameraDescriptor.LookAtMouse(mouse, position);
        }

        private static unsafe void OnMouseWheel(IMouse mouse, ScrollWheel scrollWheel)
        {
            cameraDescriptor.ZoomMouseWheel(mouse, scrollWheel);
        }

        private static void EndPulse()
        {
            cubeArrangementModel.AnimationEnabeld = false;
            cubeArrangementModel.ResetScale();
            isScramble = false;
            isRotating = false;
            isDone = false;
        }

        public static bool AreArraysEqual(float[] array1, float[] array2)
        {
            if (array1 == null || array2 == null || array1.Length != array2.Length)
            {
                return false;
            }

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }
            return true;
        }
        private static bool IsDone()
        {
            if (isScramble)
            {
                return false;
            }
            float[] TopColor = TOP_COLOR;
            float[] FrontColor = FRONT_COLOR;
            float[] LeftColor = LEFT_COLOR;
            float[] DownColor = DOWN_COLOR;
            float[] BackColor = BACK_COLOR;
            float[] RightColor = RIGHT_COLOR;
            foreach (GlCube glCube in glCubes)
            {
                TopColor = glCube.TopColor;
                FrontColor = glCube.FrontColor;
                LeftColor = glCube.LeftColor;
                DownColor = glCube.DownColor;
                BackColor = glCube.BackColor;
                RightColor = glCube.RightColor;

                if (glCube.Translation[1] == 1 && !AreArraysEqual(TopColor, TOP_COLOR))
                {
                    return false;
                }
                if (glCube.Translation[1] == -1 && !AreArraysEqual(DownColor, DOWN_COLOR))
                {
                    return false;
                }
                if (glCube.Translation[0] == 1 && !AreArraysEqual(RightColor, RIGHT_COLOR))
                {
                    return false;
                }
                if (glCube.Translation[0] == -1 && !AreArraysEqual(LeftColor, LEFT_COLOR))
                {
                    return false;
                }
                if (glCube.Translation[2] == 1 && !AreArraysEqual(FrontColor, FRONT_COLOR))
                {
                    return false;
                }
                if (glCube.Translation[2] == -1 && !AreArraysEqual(BackColor, BACK_COLOR))
                {
                    return false;
                }
            }
            isDone = true;
            return true;
        }

        private static unsafe void Scramble(int rots)
        {
            if (isScramble == true)
            {
                Console.WriteLine("Already scrambling");
                return;
            }
            isScramble = true;
            char[] randomRotations = new char[rots];
            string possibleRotations = "qazwsxfghvbnuiojkl";
            Random r = new Random();

            for (int i = 0; i < rots; i++)
            {
                queueRotation.Enqueue(possibleRotations[r.Next(possibleRotations.Length)]);
            }
        }

        private static unsafe void RotateSide(char key)
        {
            if (isRotating)
            {
                return;
            }
            isRotating = true;
            //Console.WriteLine("Rotating side");
            int nul = -2;
            int x = nul, y = nul, z = nul;
            int clockwise = 0;
            switch (key)
            {
                case 'q':
                    y = 1;
                    break;
                case 'a':
                    y = 0;
                    break;
                case 'z':
                    y = -1;
                    break;
                case 'w':
                    y = 1;
                    break;
                case 's':
                    y = 0;
                    break;
                case 'x':
                    y = -1;
                    break;
                case 'u':
                    x = -1;
                    break;
                case 'i':
                    x = 0;
                    break;
                case 'o':
                    x = 1;
                    break;
                case 'j':
                    x = -1;
                    break;
                case 'k':
                    x = 0;
                    break;
                case 'l':
                    x = 1;
                    break;
                case 'f':
                    z = 1;
                    break;
                case 'g':
                    z = 0;
                    break;
                case 'h':
                    z = -1;
                    break;
                case 'v':
                    z = 1;
                    break;
                case 'b':
                    z = 0;
                    break;
                case 'n':
                    z = -1;
                    break;
            }
            if ("qazuiofgh".Contains(key))
            {
                clockwise = 1;
            }
            else
            {
                clockwise = -1;
            }
            List<GlCube> rotCubes = new List<GlCube>();
            foreach (GlCube cube in glCubes)
            {
                if (x != nul && cube.Translation[0] > x - epsi && cube.Translation[0] < x + epsi)
                {
                    rotCubes.Add(cube);
                }
                if (y != nul && cube.Translation[1] > y - epsi && cube.Translation[1] < y + epsi)
                {
                    rotCubes.Add(cube);
                }
                if (z != nul && cube.Translation[2] > z - epsi && cube.Translation[2] < z + epsi)
                {
                    rotCubes.Add(cube);
                }
            }
            foreach (GlCube cube in rotCubes)
            {
                if (x != nul && cube.Translation[0] > x - epsi && cube.Translation[0] < x + epsi)
                {
                    cube.RotateTicX = clockwise;
                }
                if (y != nul && cube.Translation[1] > y - epsi && cube.Translation[1] < y + epsi)
                {
                    cube.RotateTicY = clockwise;
                }
                if (z != nul && cube.Translation[2] > z - epsi && cube.Translation[2] < z + epsi)
                {
                    cube.RotateTicZ = clockwise;
                }

                //      USEFULL

                //Console.WriteLine("x=" + cube.Translation[0] + " , y=" + cube.Translation[1] + " , z=" + cube.Translation[2]);
            }
        }

        private static void Window_Update(double deltaTime)
        {
            //Console.WriteLine($"Update after {deltaTime} [s].");
            // multithreaded
            // make sure it is threadsafe
            // NO GL calls
            if (isRotating == false && queueRotation.Count > 0)
            {
                DoneRotateTic = FasterDoneRotateTic;
                RotateSide(queueRotation.First());
                queueRotation.Dequeue();
            }
            if (queueRotation.Count == 0 && isRotating == false)
            {
                isScramble = false;
                DoneRotateTic = OriginalDoneRotateTic;
            }
            cubeArrangementModel.AdvanceTime(deltaTime);

            var moveSpeed = 2.5f * (float)deltaTime;

            if (primaryKeyboard.IsKeyPressed(Key.Keypad5))
            {
                //Move forwards
                cameraDescriptor.MoveForward(moveSpeed);
                
            }
            if (primaryKeyboard.IsKeyPressed(Key.Keypad2))
            {
                //Move backwards
                cameraDescriptor.MoveBackward(moveSpeed);
            }
            if (primaryKeyboard.IsKeyPressed(Key.Keypad1))
            {
                //Move left
                cameraDescriptor.MoveLeft(moveSpeed);
            }
            if (primaryKeyboard.IsKeyPressed(Key.Keypad3))
            {
                //Move right
                cameraDescriptor.MoveRight(moveSpeed);
            }
            if (primaryKeyboard.IsKeyPressed(Key.Keypad7))
            {
                //Move right
                cameraDescriptor.MoveUp(moveSpeed);
            }
            if (primaryKeyboard.IsKeyPressed(Key.Keypad4))
            {
                //Move right
                cameraDescriptor.MoveDown(moveSpeed);
            }
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
            Vector3D<float> zeroPoint = new Vector3D<float>(0.0f, 0.0f, 0.0f);
            Matrix4X4<float> rotx;
            Matrix4X4<float> roty;
            Matrix4X4<float> rotz;
            
            Matrix4X4<float> modelMatrix;
            //Quaternion<float> rotation;
            //Vector3D<float> axisX = new Vector3D<float>(1, 0, 0);
            //Vector3D<float> axisY = new Vector3D<float>(0, 1, 0);
            //Vector3D<float> axisZ = new Vector3D<float>(0, 0, 1);

            Matrix4X4<float> rotInnerX;
            Matrix4X4<float> rotInnerY;
            Matrix4X4<float> rotInnerZ;

            float[] TopColor = TOP_COLOR;
            float[] FrontColor = FRONT_COLOR;
            float[] LeftColor = LEFT_COLOR;
            float[] DownColor = DOWN_COLOR;
            float[] BackColor = BACK_COLOR;
            float[] RightColor = RIGHT_COLOR;
            float[] Translation = TRANSLATION;

            bool finishedRotation = false;

            //foreach (GlCube glCube in glCubes)
            //foreach (var (glCube, i) in glCubes.Select((glCube, i) => (glCube, i)))
            for (int i = 0; i < glCubes.Count; i++)
            {
                GlCube glCube = glCubes[i];
                finishedRotation = false;
                var scale = Matrix4X4.CreateScale((float)cubeArrangementModel.CubesScale);
                Matrix4X4<float> trans = Matrix4X4.CreateTranslation(glCube.Translation[0], glCube.Translation[1], glCube.Translation[2]);

                zeroPoint.X = -1 * glCube.Translation[0];
                zeroPoint.Y = -1 * glCube.Translation[1];
                zeroPoint.Z = -1 * glCube.Translation[2];

                //zeroPoint.X = 0;
                //zeroPoint.Y = 0;
                //zeroPoint.Z = 0;

                rotx = Matrix4X4.CreateRotationX(glCube.angleX);// + glCube.rotx;
                roty = Matrix4X4.CreateRotationY(glCube.angleY);// + glCube.roty;
                rotz = Matrix4X4.CreateRotationZ(glCube.angleZ);// + glCube.rotz;
                {
                    //rotation = Quaternion<float>.CreateFromRotationMatrix(Matrix4X4.CreateRotationZ(0.0f));

                    //if (glCube.angleX != 0)
                    //{
                    //    rotation = Quaternion<float>.CreateFromAxisAngle(axisX, glCube.angleX);
                    //}
                    //if (glCube.angleY != 0)
                    //{
                    //    rotation = Quaternion<float>.CreateFromAxisAngle(axisY, glCube.angleY);
                    //}
                    //if (glCube.angleZ != 0)
                    //{
                    //    rotation = Quaternion<float>.CreateFromAxisAngle(axisZ, glCube.angleZ);
                    //}
                    //if(glCube.angleX != 0 || glCube.angleY != 0 || glCube.angleZ != 0)
                    //{
                    //    glCube.CurrentRotation = rotation;
                    //}

                    //if(glCube.Rotations.Count > 0)
                    //{
                    //    glCube.Rotations.RemoveAt(glCube.Rotations.Count - 1);
                    //    Console.WriteLine("rot count: " + glCube.Rotations.Count);
                    //}
                    //glCube.Rotations.Add(rotation);

                    //rotx = Matrix4X4.CreateRotationX(-glCube.angleX);// + glCube.rotx;
                    //roty = Matrix4X4.CreateRotationY(-glCube.angleY);// + glCube.roty;
                    //rotz = Matrix4X4.CreateRotationZ(-glCube.angleZ);// + glCube.rotz;

                    //zeroPoint.X = 0;
                    //zeroPoint.Y = 0;
                    //zeroPoint.Z = 0;
                }
                rotInnerX = Matrix4X4.CreateRotationX((float)cubeArrangementModel.CubesAngle * 2, zeroPoint);// + glCube.rotx;
                rotInnerY = Matrix4X4.CreateRotationY((float)cubeArrangementModel.CubesAngle * 2, zeroPoint);// + glCube.roty;
                rotInnerZ = Matrix4X4.CreateRotationZ((float)cubeArrangementModel.CubesAngle * 2, zeroPoint);// + glCube.rotz;

                //rotxAxis = Matrix4X4.CreateRotationX(glCube.angleX, zeroPoint);// + glCube.rotx;
                //rotyAxis = Matrix4X4.CreateRotationY(glCube.angleY, zeroPoint);// + glCube.roty;
                //rotzAxis = Matrix4X4.CreateRotationZ(glCube.angleZ, zeroPoint);// + glCube.rotz;

                //-------------------------------------------------------------------------------------------------------------------
                //------------------------------------------------------ XXXXXX -----------------------------------------------------
                //-------------------------------------------------------------------------------------------------------------------

                if (glCube.RotateTicX != 0)
                {
                    if (glCube.RotateTicX > 0)
                    {
                        glCube.RotateTicX++;
                        glCube.angleX += (float)Math.PI / 2f * (1.0f / (float)DoneRotateTic);
                    }
                    else
                    {
                        glCube.RotateTicX--;
                        glCube.angleX -= (float)Math.PI / 2f * (1.0f / (float)DoneRotateTic);
                    }

                    if (glCube.RotateTicX == -1 * (DoneRotateTic + 1)) // CLOCKWISE
                    {
                        finishedRotation = true;

                        RightColor = glCube.RightColor;
                        LeftColor = glCube.LeftColor;
                        TopColor = glCube.FrontColor;
                        BackColor = glCube.TopColor;
                        DownColor = glCube.BackColor;
                        FrontColor = glCube.DownColor;

                        glCube.RotateTicX = 0;
                        isRotating = false;

                        {
                            //glCube.angleInnerX += glCube.angleX;

                            //rotation = Quaternion<float>.CreateFromAxisAngle(axisX, glCube.angleX);
                            //glCube.Rotations.Add(rotation);
                            //glCube.CurrentRotation = new Quaternion<float>(0, 0, 0, 1);

                            //glCube.angleInnerY = 0;
                            //glCube.angleInnerZ = 0;
                            //if (glCube.initTranslation[1] == 1)
                            //{
                            //    glCube.angleInnerX += glCube.angleX;
                            //}
                            //if (glCube.initTranslation[0] == 1)
                            //{
                            //    glCube.angleInnerX += glCube.angleX;
                            //}
                            //else
                            //{
                            //    glCube.angleInnerX += glCube.angleX;
                            //}
                        }
                        glCube.angleX = 0;

                        if (glCube.Translation[1] == 1 && glCube.Translation[2] == 1)
                        {
                            glCube.Translation[2] = -1.0f;
                        }
                        else
                        {
                            if (glCube.Translation[1] == 1 && glCube.Translation[2] == 0)
                            {
                                glCube.Translation[1] = 0.0f;
                                glCube.Translation[2] = -1.0f;
                            }
                            else
                            {
                                if (glCube.Translation[1] == 1 && glCube.Translation[2] == -1)
                                {
                                    glCube.Translation[1] = -1.0f;
                                }
                                else
                                {
                                    if (glCube.Translation[1] == 0 && glCube.Translation[2] == 1)
                                    {
                                        glCube.Translation[1] = 1.0f;
                                        glCube.Translation[2] = 0.0f;
                                    }
                                    else
                                    {
                                        if (glCube.Translation[1] == 0 && glCube.Translation[2] == -1)
                                        {
                                            glCube.Translation[1] = -1.0f;
                                            glCube.Translation[2] = 0.0f;
                                        }
                                        else
                                        {
                                            if (glCube.Translation[1] == -1 && glCube.Translation[2] == 1)
                                            {
                                                glCube.Translation[1] = 1.0f;
                                            }
                                            else
                                            {
                                                if (glCube.Translation[1] == -1 && glCube.Translation[2] == 0)
                                                {
                                                    glCube.Translation[1] = 0.0f;
                                                    glCube.Translation[2] = 1.0f;
                                                }
                                                else
                                                {
                                                    if (glCube.Translation[1] == -1 && glCube.Translation[2] == -1)
                                                    {
                                                        glCube.Translation[2] = 1.0f;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (glCube.RotateTicX == DoneRotateTic + 1)      // COUNTER CLOCKWISE
                    {
                        finishedRotation = true;
                        glCube.RotateTicX = 0;

                        RightColor = glCube.RightColor;
                        LeftColor = glCube.LeftColor;
                        TopColor = glCube.BackColor;
                        FrontColor = glCube.TopColor;
                        DownColor = glCube.FrontColor;
                        BackColor = glCube.DownColor;

                        isRotating = false;
                        //glCube.angleInnerX += glCube.angleX;

                        //rotation = Quaternion<float>.CreateFromAxisAngle(axisX, glCube.angleX);
                        //glCube.Rotations.Add(rotation);
                        //glCube.CurrentRotation = new Quaternion<float>(0, 0, 0, 1);

                        //if (glCube.initTranslation[1] == 1)
                        //{
                        //    glCube.angleInnerZ += glCube.angleX;
                        //    glCube.angleInnerX = 0;
                        //    glCube.angleInnerY = 0;
                        //}
                        //if (glCube.initTranslation[0] == 1)
                        //{
                        //    glCube.angleInnerX += glCube.angleX;
                        //}
                        //else
                        //{
                        //    glCube.angleInnerX += glCube.angleX;
                        //}
                        //glCube.angleInnerZ = 0;
                        glCube.angleX = 0;

                        if (glCube.Translation[1] == 1 && glCube.Translation[2] == 1)
                        {
                            glCube.Translation[1] = -1.0f;
                        }
                        else
                        {
                            if (glCube.Translation[1] == 1 && glCube.Translation[2] == 0)
                            {
                                glCube.Translation[1] = 0.0f;
                                glCube.Translation[2] = 1.0f;
                            }
                            else
                            {
                                if (glCube.Translation[1] == 1 && glCube.Translation[2] == -1)
                                {
                                    glCube.Translation[2] = 1.0f;
                                }
                                else
                                {
                                    if (glCube.Translation[1] == 0 && glCube.Translation[2] == 1)
                                    {
                                        glCube.Translation[1] = -1.0f;
                                        glCube.Translation[2] = 0.0f;
                                    }
                                    else
                                    {
                                        if (glCube.Translation[1] == 0 && glCube.Translation[2] == -1)
                                        {
                                            glCube.Translation[1] = 1.0f;
                                            glCube.Translation[2] = 0.0f;
                                        }
                                        else
                                        {
                                            if (glCube.Translation[1] == -1 && glCube.Translation[2] == 1)
                                            {
                                                glCube.Translation[2] = -1.0f;
                                            }
                                            else
                                            {
                                                if (glCube.Translation[1] == -1 && glCube.Translation[2] == 0)
                                                {
                                                    glCube.Translation[1] = 0.0f;
                                                    glCube.Translation[2] = -1.0f;
                                                }
                                                else
                                                {
                                                    if (glCube.Translation[1] == -1 && glCube.Translation[2] == -1)
                                                    {
                                                        glCube.Translation[1] = 1.0f;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //-------------------------------------------------------------------------------------------------------------------
                //------------------------------------------------------ YYYYYY -----------------------------------------------------
                //-------------------------------------------------------------------------------------------------------------------

                if (glCube.RotateTicY != 0)
                {
                    if (glCube.RotateTicY > 0)
                    {
                        glCube.RotateTicY++;
                        glCube.angleY += (float)Math.PI / 2f * (1.0f / (float)DoneRotateTic);
                    }
                    else
                    {
                        glCube.RotateTicY--;
                        glCube.angleY -= (float)Math.PI / 2f * (1.0f / (float)DoneRotateTic);
                    }
                    if (glCube.RotateTicY == -1 * (DoneRotateTic + 1)) // CLOCKWISE
                    {
                        finishedRotation = true;

                        TopColor = glCube.TopColor;
                        DownColor = glCube.DownColor;
                        FrontColor = glCube.RightColor;
                        LeftColor = glCube.FrontColor;
                        BackColor = glCube.LeftColor;
                        RightColor = glCube.BackColor;

                        glCube.RotateTicY = 0;
                        isRotating = false;
                        //glCube.angleInnerY += glCube.angleY;
                        //glCube.angleY = 0;

                        //rotation = Quaternion<float>.CreateFromAxisAngle(axisY, glCube.angleY);
                        //glCube.Rotations.Add(rotation);
                        //glCube.CurrentRotation = new Quaternion<float>(0, 0, 0, 1);

                        if (glCube.Translation[0] == -1 && glCube.Translation[2] == 1)
                        {
                            glCube.Translation[2] = -1.0f;
                        }
                        else
                        {
                            if (glCube.Translation[0] == 0 && glCube.Translation[2] == 1)
                            {
                                glCube.Translation[0] = -1.0f;
                                glCube.Translation[2] = 0.0f;
                            }
                            else
                            {
                                if (glCube.Translation[0] == 1 && glCube.Translation[2] == 1)
                                {
                                    glCube.Translation[0] = -1.0f;
                                }
                                else
                                {
                                    if (glCube.Translation[0] == -1 && glCube.Translation[2] == 0)
                                    {
                                        glCube.Translation[0] = 0.0f;
                                        glCube.Translation[2] = -1.0f;
                                    }
                                    else
                                    {
                                        if (glCube.Translation[0] == 1 && glCube.Translation[2] == 0)
                                        {
                                            glCube.Translation[0] = 0.0f;
                                            glCube.Translation[2] = 1.0f;
                                        }
                                        else
                                        {
                                            if (glCube.Translation[0] == -1 && glCube.Translation[2] == -1)
                                            {
                                                glCube.Translation[0] = 1.0f;
                                            }
                                            else
                                            {
                                                if (glCube.Translation[0] == 0 && glCube.Translation[2] == -1)
                                                {
                                                    glCube.Translation[0] = 1.0f;
                                                    glCube.Translation[2] = 0.0f;
                                                }
                                                else
                                                {
                                                    if (glCube.Translation[0] == 1 && glCube.Translation[2] == -1)
                                                    {
                                                        glCube.Translation[2] = 1.0f;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (glCube.RotateTicY == DoneRotateTic + 1)      // COUNTER CLOCKWISE
                    {
                        finishedRotation = true;

                        TopColor = glCube.TopColor;
                        DownColor = glCube.DownColor;
                        FrontColor = glCube.LeftColor;
                        RightColor = glCube.FrontColor;
                        BackColor = glCube.RightColor;
                        LeftColor = glCube.BackColor;

                        glCube.RotateTicY = 0;
                        isRotating = false;
                        //glCube.angleInnerY += glCube.angleY;
                        //glCube.angleY = 0;

                        //rotation = Quaternion<float>.CreateFromAxisAngle(axisY, glCube.angleY);
                        //glCube.Rotations.Add(rotation);
                        //glCube.CurrentRotation = new Quaternion<float>(0, 0, 0, 1);

                        if (glCube.Translation[0] == -1 && glCube.Translation[2] == 1)
                        {
                            glCube.Translation[0] = 1.0f;
                        }
                        else
                        {
                            if (glCube.Translation[0] == 0 && glCube.Translation[2] == 1)
                            {
                                glCube.Translation[0] = 1.0f;
                                glCube.Translation[2] = 0.0f;
                            }
                            else
                            {
                                if (glCube.Translation[0] == 1 && glCube.Translation[2] == 1)
                                {
                                    glCube.Translation[2] = -1.0f;
                                }
                                else
                                {
                                    if (glCube.Translation[0] == -1 && glCube.Translation[2] == 0)
                                    {
                                        glCube.Translation[0] = 0.0f;
                                        glCube.Translation[2] = 1.0f;
                                    }
                                    else
                                    {
                                        if (glCube.Translation[0] == 1 && glCube.Translation[2] == 0)
                                        {
                                            glCube.Translation[0] = 0.0f;
                                            glCube.Translation[2] = -1.0f;
                                        }
                                        else
                                        {
                                            if (glCube.Translation[0] == -1 && glCube.Translation[2] == -1)
                                            {
                                                glCube.Translation[2] = 1.0f;
                                            }
                                            else
                                            {
                                                if (glCube.Translation[0] == 0 && glCube.Translation[2] == -1)
                                                {
                                                    glCube.Translation[0] = -1.0f;
                                                    glCube.Translation[2] = 0.0f;
                                                }
                                                else
                                                {
                                                    if (glCube.Translation[0] == 1 && glCube.Translation[2] == -1)
                                                    {
                                                        glCube.Translation[0] = -1.0f;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        {
                            //for (int i = -1; i < 2; i++)
                            //{
                            //    if (glCube.Translation[0] > i - epsi && glCube.Translation[0] < i + epsi)
                            //    {
                            //        glCube.Translation[0] = i;
                            //    }
                            //    if (glCube.Translation[1] > i - epsi && glCube.Translation[1] < i + epsi)
                            //    {
                            //        glCube.Translation[1] = i;
                            //    }
                            //    if (glCube.Translation[2] > i - epsi && glCube.Translation[2] < i + epsi)
                            //    {
                            //        glCube.Translation[2] = i;
                            //    }
                            //}


                            //Console.WriteLine("x = " + glCube.Translation[0] + " , y = " + glCube.Translation[1] + " , z = " + glCube.Translation[2]);
                            //Console.WriteLine("angleX = " + glCube.angleX + " , angleY = " + glCube.angleY + " , angleZ = " + glCube.angleZ);
                        }
                    }
                }

                //-------------------------------------------------------------------------------------------------------------------
                //------------------------------------------------------ ZZZZZZ -----------------------------------------------------
                //-------------------------------------------------------------------------------------------------------------------

                if (glCube.RotateTicZ != 0)
                {
                    if (glCube.RotateTicZ > 0)
                    {
                        glCube.RotateTicZ++;
                        glCube.angleZ += (float)Math.PI / 2f * (1.0f / (float)DoneRotateTic);
                    }
                    else
                    {
                        glCube.RotateTicZ--;
                        glCube.angleZ -= (float)Math.PI / 2f * (1.0f / (float)DoneRotateTic);
                    }
                    //if (glCube.RotateTicZ == DoneRotateTic + 1 || glCube.RotateTicZ == -1 * (DoneRotateTic + 1))
                    //{
                    //    glCube.RotateTicZ = 0;
                    //    isRotating = false;
                    //}

                    if (glCube.RotateTicZ == -1 * (DoneRotateTic + 1)) // CLOCKWISE
                    {
                        finishedRotation = true;

                        FrontColor = glCube.FrontColor;
                        BackColor = glCube.BackColor;
                        TopColor = glCube.LeftColor;
                        RightColor = glCube.TopColor;
                        DownColor = glCube.RightColor;
                        LeftColor = glCube.DownColor;

                        //Console.WriteLine(i + " cube FROM x = " + glCube.Translation[0] + " , y = " + glCube.Translation[1] + " , z = " + glCube.Translation[2]);

                        glCube.RotateTicZ = 0;
                        isRotating = false;
                        //glCube.angleInnerY += glCube.angleY;
                        //glCube.angleY = 0;

                        //rotation = Quaternion<float>.CreateFromAxisAngle(axisY, glCube.angleY);
                        //glCube.Rotations.Add(rotation);
                        //glCube.CurrentRotation = new Quaternion<float>(0, 0, 0, 1);

                        if (glCube.Translation[0] == -1 && glCube.Translation[1] == 1)
                        {
                            glCube.Translation[0] = 1.0f;
                        }
                        else
                        {
                            if (glCube.Translation[0] == 0 && glCube.Translation[1] == 1)
                            {
                                glCube.Translation[0] = 1.0f;
                                glCube.Translation[1] = 0.0f;
                            }
                            else
                            {
                                if (glCube.Translation[0] == 1 && glCube.Translation[1] == 1)
                                {
                                    glCube.Translation[1] = -1.0f;
                                }
                                else
                                {
                                    if (glCube.Translation[0] == -1 && glCube.Translation[1] == 0)
                                    {
                                        glCube.Translation[0] = 0.0f;
                                        glCube.Translation[1] = 1.0f;
                                    }
                                    else
                                    {
                                        if (glCube.Translation[0] == 1 && glCube.Translation[1] == 0)
                                        {
                                            glCube.Translation[0] = 0.0f;
                                            glCube.Translation[1] = -1.0f;
                                        }
                                        else
                                        {
                                            if (glCube.Translation[0] == -1 && glCube.Translation[1] == -1)
                                            {
                                                glCube.Translation[1] = 1.0f;
                                            }
                                            else
                                            {
                                                if (glCube.Translation[0] == 0 && glCube.Translation[1] == -1)
                                                {
                                                    glCube.Translation[0] = -1.0f;
                                                    glCube.Translation[1] = 0.0f;
                                                }
                                                else
                                                {
                                                    if (glCube.Translation[0] == 1 && glCube.Translation[1] == -1)
                                                    {
                                                        glCube.Translation[0] = -1.0f;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //Console.WriteLine(i + " cube turned to x = " + glCube.Translation[0] + " , y = " + glCube.Translation[1] + " , z = " + glCube.Translation[2] + "\n");
                        //Console.WriteLine(i + " cube angleX = " + glCube.angleX + " , angleY = " + glCube.angleY + " , angleZ = " + glCube.angleZ);
                    }
                    if (glCube.RotateTicZ == DoneRotateTic + 1)      // COUNTER CLOCKWISE
                    {
                        finishedRotation = true;

                        FrontColor = glCube.FrontColor;
                        BackColor = glCube.BackColor;
                        TopColor = glCube.RightColor;
                        LeftColor = glCube.TopColor;
                        DownColor = glCube.LeftColor;
                        RightColor = glCube.DownColor;

                        glCube.RotateTicZ = 0;
                        isRotating = false;
                        //glCube.angleInnerY += glCube.angleY;
                        //glCube.angleY = 0;

                        //rotation = Quaternion<float>.CreateFromAxisAngle(axisY, glCube.angleY);
                        //glCube.Rotations.Add(rotation);
                        //glCube.CurrentRotation = new Quaternion<float>(0, 0, 0, 1);

                        if (glCube.Translation[0] == -1 && glCube.Translation[1] == 1)
                        {
                            glCube.Translation[1] = -1.0f;
                        }
                        else
                        {
                            if (glCube.Translation[0] == 0 && glCube.Translation[1] == 1)
                            {
                                glCube.Translation[0] = -1.0f;
                                glCube.Translation[1] = 0.0f;
                            }
                            else
                            {
                                if (glCube.Translation[0] == 1 && glCube.Translation[1] == 1)
                                {
                                    glCube.Translation[0] = -1.0f;
                                }
                                else
                                {
                                    if (glCube.Translation[0] == -1 && glCube.Translation[1] == 0)
                                    {
                                        glCube.Translation[0] = 0.0f;
                                        glCube.Translation[1] = -1.0f;
                                    }
                                    else
                                    {
                                        if (glCube.Translation[0] == 1 && glCube.Translation[1] == 0)
                                        {
                                            glCube.Translation[0] = 0.0f;
                                            glCube.Translation[1] = 1.0f;
                                        }
                                        else
                                        {
                                            if (glCube.Translation[0] == -1 && glCube.Translation[1] == -1)
                                            {
                                                glCube.Translation[0] = 1.0f;
                                            }
                                            else
                                            {
                                                if (glCube.Translation[0] == 0 && glCube.Translation[1] == -1)
                                                {
                                                    glCube.Translation[0] = 1.0f;
                                                    glCube.Translation[1] = 0.0f;
                                                }
                                                else
                                                {
                                                    if (glCube.Translation[0] == 1 && glCube.Translation[1] == -1)
                                                    {
                                                        glCube.Translation[1] = 1.0f;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        //Console.WriteLine(i + " cube x = " + glCube.Translation[0] + " , y = " + glCube.Translation[1] + " , z = " + glCube.Translation[2]);
                        //Console.WriteLine(i + " cube angleX = " + glCube.angleX + " , angleY = " + glCube.angleY + " , angleZ = " + glCube.angleZ);
                    }
                }

                if (finishedRotation)
                {
                    modelMatrix = scale * trans * rotx * roty * rotz;
                    SetModelMatrix(modelMatrix);
                    Gl.BindVertexArray(glCube.Vao);
                    Gl.DrawElements(GLEnum.Triangles, glCube.IndexArrayLength, GLEnum.UnsignedInt, null);
                    Gl.BindVertexArray(0);

                    Translation = glCube.Translation;
                    GlCube newGLCube = GlCube.CreateCubeWithFaceColors(Gl, TopColor, FrontColor, LeftColor, DownColor, BackColor, RightColor, Translation);
                    //Console.WriteLine("Deleting: " + glCube.Translation[0] + " " + glCube.Translation[1] + " " + glCube.Translation[2] + " at index: " + i);
                    glCubes[i] = newGLCube;

                    modelMatrix = scale * trans;
                    SetModelMatrix(modelMatrix);
                    Gl.BindVertexArray(newGLCube.Vao);
                    Gl.DrawElements(GLEnum.Triangles, newGLCube.IndexArrayLength, GLEnum.UnsignedInt, null);
                    Gl.BindVertexArray(0);
                    glCube.ReleaseGlCube();

                    if (IsDone())
                    {
                        Console.WriteLine("DONE BRO");
                        cubeArrangementModel.AnimationEnabeld = true;
                        isRotating = true;
                        isScramble = true;
                    }
                    //else
                    //{
                    //    Console.WriteLine("\nKeep rolling mate\n");
                    //}
                    //}
                }
                else
                {
                    modelMatrix = scale * rotInnerX * rotInnerY * rotInnerZ * trans * rotx * roty * rotz;
                    SetModelMatrix(modelMatrix);
                    Gl.BindVertexArray(glCube.Vao);
                    Gl.DrawElements(GLEnum.Triangles, glCube.IndexArrayLength, GLEnum.UnsignedInt, null);
                    Gl.BindVertexArray(0);
                }
                { 
                //Quaternion<float> combinedRotation = new Quaternion<float>(0,0,0,1); // Kezdőérték

                //foreach (Quaternion<float> currentRotation in glCube.Rotations)
                //{
                //    //combinedRotation = Quaternion.Multiply(combinedRotation, rotation);
                //    combinedRotation = combinedRotation * currentRotation;
                //    Console.WriteLine(combinedRotation.ToString());
                //}

                //if(glCube.CurrentRotation != new Quaternion<float>(0, 0, 0, 1))
                //{
                //    combinedRotation = combinedRotation * glCube.CurrentRotation;
                //}

                //Matrix4X4<float> rotationMatrix = Matrix4X4.CreateFromQuaternion<float>(combinedRotation);
                //modelMatrix = scale * rotInnerZ * rotInnerY * rotInnerX * trans * rotz * roty * rotx;

                //modelMatrix = scale * trans * rotx * roty * rotz;

                //modelMatrix = rotxAxis * rotzAxis * scale * trans * rotyAxis;
                //Matrix4X4<float> modelMatrix = scale * rotGlobY * trans;

                //SetModelMatrix(modelMatrix);
                //Gl.BindVertexArray(glCube.Vao);
                //Gl.DrawElements(GLEnum.Triangles, glCube.IndexArrayLength, GLEnum.UnsignedInt, null);
                //Gl.BindVertexArray(0);

            }
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

            //float[] TOP_COLOR = [0.95f, 0.95f, 0.95f, 1.0f];   // TOP
            //float[] FRONT_COLOR = [1.0f, 0.0f, 0.0f, 1.0f];      // FRONT
            //float[] LEFT_COLOR = [0.0f, 1.0f, 0.0f, 1.0f];      // LEFT
            //float[] DOWN_COLOR = [1.0f, 1.0f, 0.0f, 1.0f];      // DOWN
            //float[] BACK_COLOR = [1.0f, 0.6f, 0.0f, 1.0f];      // BACK
            //float[] RIGHT_COLOR = [0.3f, 0.52f, 0.91f, 1.0f];      // RIGHT
            //float[] BLACK_COLOR = [0f, 0f, 0f, 1.0f];

            //            GlCube glCube = GlCube.CreateCubeWithFaceColors(Gl, TOP_COLOR, FRONT_COLOR, LEFT_COLOR, DOWN_COLOR, BACK_COLOR, RIGHT_COLOR,translation);


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

            //var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)Math.PI / 4f, 1024f / 768f, 0.1f, 100);
            var projectionMatrix = cameraDescriptor.getProjection(window.Size);
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
            //var viewMatrix = Matrix4X4.CreateLookAt(cameraDescriptor.Position, cameraDescriptor.Target, cameraDescriptor.UpVector);
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