using System.Numerics;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace Szeminarium1_24_02_17_2
{
    internal static class Program
    {
        private static CameraDescriptor cameraDescriptor = new();

        private static CubeArrangementModel cubeArrangementModel = new();

        private static IWindow window;

        private static IInputContext inputContext;

        private static GL Gl;

        private static ImGuiController controller;

        private static uint program;

        private static GlCube glCubeCentered;

        private static GlCube glCubeRotating;


        // --------------------------------- fenybeallitashoz valtozok --------------------------------------------
        private static float Shininess = 50;
        private static Vector3D<float> AmbientStrength = new Vector3D<float>(0.2f, 0.3f, 0.4f);
        private static Vector3D<float> SpecularStrength = new Vector3D<float>(0.5f, 0.5f, 0.5f);
        private static Vector3D<float> DiffuseStrength = new Vector3D<float>(0.3f, 0.3f, 0.3f);
        private static float ambientRed = 1f;
        private static float ambientGreen = 1f;
        private static float ambientBlue = 1f;

        // kocka bal oldalanak szine
        //private static float[] balOldalSzine = [0.6f, 0.4f, 0.8f, 1f];
        private static float[] balOldalSzine = [0.0f, 0.0f, 0.0f, 1f];
        private static int selectedItem = 0;

        private const string ModelMatrixVariableName = "uModel";
        private const string NormalMatrixVariableName = "uNormal";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";

        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
		layout (location = 1) in vec4 vCol;
        layout (location = 2) in vec3 vNorm;

        uniform mat4 uModel;
        uniform mat3 uNormal;
        uniform mat4 uView;
        uniform mat4 uProjection;

		out vec4 outCol;
        out vec3 outNormal;
        out vec3 outWorldPosition;
        
        void main()
        {
			outCol = vCol;
            gl_Position = uProjection*uView*uModel*vec4(vPos.x, vPos.y, vPos.z, 1.0);
            outNormal = uNormal*vNorm;
            outWorldPosition = vec3(uModel*vec4(vPos.x, vPos.y, vPos.z, 1.0));
        }
        ";

        private const string LightColorVariableName = "lightColor";
        private const string LightPositionVariableName = "lightPos";
        private const string ViewPosVariableName = "viewPos";
        private const string ShininessVariableName = "shininess";

        private const string AmbientStrengthVariableName = "ambientStrength";
        private const string SpecularStrengthVariableName = "specularStrength";
        private const string DiffuseStrengthVariableName = "diffuseStrength";

        /*
		A fragment shader elsődleges feladata, hogy meghatározza az egyes pixelek színét és más tulajdonságait a képernyőn.
		Itt végezhetők el az olyan pixel-szintű számítások, mint a színkeverés, a textúrázás, a fényhatások és a színárnyalatok
		kezelése. Alapvető szerepe van a végső megjelenés meghatározásában.
		*/
        private static readonly string FragmentShaderSource = @"
        #version 330 core
        
        uniform vec3 lightColor;
        uniform vec3 lightPos;
        uniform vec3 viewPos;
        uniform float shininess;

		uniform vec3 ambientStrength;  // kornyezeti fenyerosseg
		uniform vec3 specularStrength; // tukrozodes
		uniform vec3 diffuseStrength;  // egyenletes szorodasa a fenynek minden iranyba


        out vec4 FragColor;

		in vec4 outCol;
        in vec3 outNormal;
        in vec3 outWorldPosition;

        void main()
        {
            vec3 ambient = ambientStrength * lightColor;

            vec3 norm = normalize(outNormal);
            vec3 lightDir = normalize(lightPos - outWorldPosition);  // a feny iranya
            float diff = max(dot(norm, lightDir), 0.0);  // a normal es a feny iranyanak skalaris szorzata lekorlatozva 0-ra hogy nel legyen negativ
            vec3 diffuse = diff * lightColor * diffuseStrength;

            vec3 viewDir = normalize(viewPos - outWorldPosition);	 // nezet iranya a kamera es a fragment kozott
            vec3 reflectDir = reflect(-lightDir, norm);				 // a beerkezo feny iranyanak tukrozese a felulet normalisa menten
            float spec = pow(max(dot(viewDir, reflectDir), 0.0), shininess) / max(dot(norm,viewDir), -dot(norm,lightDir));
            vec3 specular = specularStrength * spec * lightColor;  

            vec3 result = (ambient + diffuse + specular) * outCol.xyz; // az osszes vilagitasi komponens szorzasa a bejovo szinnel
            FragColor = vec4(result, outCol.w); // a vegso fragment szine, amely a 'result' RGB ertekeit tartalmazza es az eredeti alfa erteket az outCol.w
        }
        ";

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "2 szeminárium";
            windowOptions.Size = new Vector2D<int>(1080, 1080);

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
            inputContext = window.CreateInput();
            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;
            }

            Gl = window.CreateOpenGL();

            controller = new ImGuiController(Gl, window, inputContext);

            // Handle resizes
            window.FramebufferResize += s =>
            {
                // Adjust the viewport to the new window size
                Gl.Viewport(s);
            };


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
            switch (key)
            {
                case Key.Space:
                    cubeArrangementModel.AnimationEnabeld = !cubeArrangementModel.AnimationEnabeld;
                    break;
            }
        }

        private static void Window_Update(double deltaTime)
        {
            //Console.WriteLine($"Update after {deltaTime} [s].");
            // multithreaded
            // make sure it is threadsafe
            // NO GL calls
            cubeArrangementModel.AdvanceTime(deltaTime);

            var keyboard = window.CreateInput().Keyboards[0];
            if (keyboard.IsKeyPressed(Key.W))
            {
                cameraDescriptor.DecreaseZXAngle();
            }
            if (keyboard.IsKeyPressed(Key.A))
            {
                cameraDescriptor.IncreaseZYAngle();
            }
            if (keyboard.IsKeyPressed(Key.S))
            {
                cameraDescriptor.IncreaseZXAngle();
            }
            if (keyboard.IsKeyPressed(Key.D))
            {
                cameraDescriptor.DecreaseZYAngle();
            }
            if (keyboard.IsKeyPressed(Key.Down))
            {
                cameraDescriptor.IncreaseDistance();
            }
            if (keyboard.IsKeyPressed(Key.Up))
            {
                cameraDescriptor.DecreaseDistance();
            }

            controller.Update((float)deltaTime);
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

            SetLightColor();
            SetLightPosition();
            SetViewerPosition();
            SetShininess();

            // feny effektus beallitasai
            SetAmbientStrength();
            SetSpecularStrength();
            SetDiffuseStrength();

            DrawPulsingCenterCube();

            DrawRevolvingCube();

            //ImGuiNET.ImGui.ShowDemoWindow();
            // beallitja hogy egybol latszodjanak ne egy lenyilo ablakocska legyen
            ImGuiNET.ImGui.Begin("Lighting properties", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);

            ImGuiNET.ImGui.SliderFloat("Shininess", ref Shininess, 1, 200);

            ImGui.Text("Ambient Light Strength");
            ImGuiNET.ImGui.SliderFloat("X##Ambient", ref AmbientStrength.X, 0, 1);  // X##Ambient double # means the ID
            ImGuiNET.ImGui.SliderFloat("Y##Ambient", ref AmbientStrength.Y, 0, 1); // X Y Z makes diff
            ImGuiNET.ImGui.SliderFloat("Z##Ambient", ref AmbientStrength.Z, 0, 1); // Changing x changes all x

            ImGui.Text("Specular Light Strength");
            ImGuiNET.ImGui.SliderFloat("X##Specular", ref SpecularStrength.X, 0, 1);
            ImGuiNET.ImGui.SliderFloat("Y##Specular", ref SpecularStrength.Y, 0, 1);
            ImGuiNET.ImGui.SliderFloat("Z##Specular", ref SpecularStrength.Z, 0, 1);

            ImGui.Text("Diffuse Light Strength");
            ImGuiNET.ImGui.SliderFloat("X##Diffuse", ref DiffuseStrength.X, 0, 1);
            ImGuiNET.ImGui.SliderFloat("Y##Diffuse", ref DiffuseStrength.Y, 0, 1);
            ImGuiNET.ImGui.SliderFloat("Z##Diffuse", ref DiffuseStrength.Z, 0, 1);

            ImGui.Text("Ambient Light Color");
            ImGuiNET.ImGui.SliderFloat("Red", ref ambientRed, 0, 1);
            ImGuiNET.ImGui.SliderFloat("Green", ref ambientGreen, 0, 1);
            ImGuiNET.ImGui.SliderFloat("Blue", ref ambientBlue, 0, 1);
            // shows the selected color and the amount that makes it
            Vector3 color = new Vector3(ambientRed, ambientGreen, ambientBlue);
            ImGui.ColorEdit3("color", ref color);

            string[] items = new string[]
            {
                "Turquoise",
                "Amethyst",
                "Coral",
                "Chartreuse",
                "Periwinkle",
                "Salmon"
            };

            float[][] itemsColors = new float[][]
            {
                new float[] { 0.251f, 0.878f, 0.816f, 1f },  // Turquoise
                new float[] { 0.6f, 0.4f, 0.8f, 1f },       // Amethyst
                new float[] { 1.0f, 0.498f, 0.314f, 1f },   // Coral
                new float[] { 0.498f, 1.0f, 0.0f, 1f },     // Chartreuse
                new float[] { 0.8f, 0.8f, 1.0f, 1f },       // Periwinkle
                new float[] { 0.98f, 0.5f, 0.447f, 1f }     // Salmon
            };


            if (ImGui.Combo("Combo", ref selectedItem, items, items.Length))
            {
                Console.WriteLine($"Selected: {items[selectedItem]}");
                Console.WriteLine(selectedItem);
                balOldalSzine = itemsColors[selectedItem];
                SetUpObjects();
            }

            ImGuiNET.ImGui.End();

            controller.Render();
        }

        private static unsafe void SetLightColor()
        {
            int location = Gl.GetUniformLocation(program, LightColorVariableName);

            if (location == -1)
            {
                throw new Exception($"{LightColorVariableName} uniform not found on shader.");
            }

            Gl.Uniform3(location, ambientRed, ambientGreen, ambientBlue);
            CheckError();
        }

        private static unsafe void SetLightPosition()
        {
            int location = Gl.GetUniformLocation(program, LightPositionVariableName);

            if (location == -1)
            {
                throw new Exception($"{LightPositionVariableName} uniform not found on shader.");
            }

            Gl.Uniform3(location, 0f, 2f, 0f);
            CheckError();
        }

        private static unsafe void SetViewerPosition()
        {
            int location = Gl.GetUniformLocation(program, ViewPosVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewPosVariableName} uniform not found on shader.");
            }

            Gl.Uniform3(location, cameraDescriptor.Position.X, cameraDescriptor.Position.Y, cameraDescriptor.Position.Z);
            CheckError();
        }

        private static unsafe void SetShininess()
        {
            int location = Gl.GetUniformLocation(program, ShininessVariableName);

            if (location == -1)
            {
                throw new Exception($"{ShininessVariableName} uniform not found on shader.");
            }

            Gl.Uniform1(location, Shininess);
            CheckError();
        }

        // ------------------------------------------- fenybeallitasok ---------------------------------------------------------
        private static unsafe void SetAmbientStrength()
        {
            int location = Gl.GetUniformLocation(program, AmbientStrengthVariableName);
            if (location == -1)
            {
                throw new Exception($"{AmbientStrengthVariableName} uniform not found on shader.");
            }
            Gl.Uniform3(location, AmbientStrength.X, AmbientStrength.Y, AmbientStrength.Z);
            CheckError();
        }

        private static unsafe void SetSpecularStrength()
        {
            int location = Gl.GetUniformLocation(program, SpecularStrengthVariableName);
            if (location == -1)
            {
                throw new Exception($"{SpecularStrengthVariableName} uniform not found on shader.");
            }
            Gl.Uniform3(location, SpecularStrength.X, SpecularStrength.Y, SpecularStrength.Z);
            CheckError();
        }

        private static unsafe void SetDiffuseStrength()
        {
            int location = Gl.GetUniformLocation(program, DiffuseStrengthVariableName);
            if (location == -1)
            {
                throw new Exception($"{DiffuseStrength} uniform not found on shader.");
            }
            Gl.Uniform3(location, DiffuseStrength.X, DiffuseStrength.Y, DiffuseStrength.Z);
            CheckError();
        }

        private static unsafe void DrawRevolvingCube()
        {
            // set material uniform to metal

            Matrix4X4<float> diamondScale = Matrix4X4.CreateScale(0.25f);
            Matrix4X4<float> rotx = Matrix4X4.CreateRotationX((float)Math.PI / 4f);
            Matrix4X4<float> rotz = Matrix4X4.CreateRotationZ((float)Math.PI / 4f);
            Matrix4X4<float> rotLocY = Matrix4X4.CreateRotationY((float)cubeArrangementModel.DiamondCubeAngleOwnRevolution);
            Matrix4X4<float> trans = Matrix4X4.CreateTranslation(1f, 1f, 0f);
            Matrix4X4<float> rotGlobY = Matrix4X4.CreateRotationY((float)cubeArrangementModel.DiamondCubeAngleRevolutionOnGlobalY);
            Matrix4X4<float> modelMatrix = diamondScale * rotx * rotz * rotLocY * trans * rotGlobY;

            SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(glCubeRotating.Vao);
            Gl.DrawElements(GLEnum.Triangles, glCubeRotating.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);
        }

        private static unsafe void DrawPulsingCenterCube()
        {
            // set material uniform to rubber

            var modelMatrixForCenterCube = Matrix4X4.CreateScale((float)cubeArrangementModel.CenterCubeScale);
            SetModelMatrix(modelMatrixForCenterCube);
            Gl.BindVertexArray(glCubeCentered.Vao);
            Gl.DrawElements(GLEnum.Triangles, glCubeCentered.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);
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

            var modelMatrixWithoutTranslation = new Matrix4X4<float>(modelMatrix.Row1, modelMatrix.Row2, modelMatrix.Row3, modelMatrix.Row4);
            modelMatrixWithoutTranslation.M41 = 0;
            modelMatrixWithoutTranslation.M42 = 0;
            modelMatrixWithoutTranslation.M43 = 0;
            modelMatrixWithoutTranslation.M44 = 1;

            Matrix4X4<float> modelInvers;
            Matrix4X4.Invert<float>(modelMatrixWithoutTranslation, out modelInvers);
            Matrix3X3<float> normalMatrix = new Matrix3X3<float>(Matrix4X4.Transpose(modelInvers));
            location = Gl.GetUniformLocation(program, NormalMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{NormalMatrixVariableName} uniform not found on shader.");
            }
            Gl.UniformMatrix3(location, 1, false, (float*)&normalMatrix);
            CheckError();
        }

        private static unsafe void SetUpObjects()
        {

            float[] face1Color = [1.0f, 0.0f, 0.0f, 1.0f];
            float[] face2Color = [0.0f, 1.0f, 0.0f, 1.0f];
            float[] face3Color = [0.0f, 0.0f, 1.0f, 1.0f];
            float[] face4Color = [1.0f, 0.0f, 1.0f, 1.0f];
            float[] face5Color = [0.0f, 1.0f, 1.0f, 1.0f];
            float[] face6Color = [1.0f, 1.0f, 0.0f, 1.0f];

            glCubeCentered = GlCube.CreateCubeWithFaceColors(Gl, face1Color, face2Color, balOldalSzine, face4Color, face5Color, face6Color);

            glCubeRotating = GlCube.CreateCubeWithFaceColors(Gl, face1Color, face1Color, face1Color, face1Color, face1Color, face1Color);
        }



        private static void Window_Closing()
        {
            glCubeCentered.ReleaseGlCube();
            glCubeRotating.ReleaseGlCube();
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
            var viewMatrix = Matrix4X4.CreateLookAt(cameraDescriptor.Position, cameraDescriptor.Target, cameraDescriptor.UpVector);
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