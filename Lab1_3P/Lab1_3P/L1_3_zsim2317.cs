using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Lab1
{

//  Zoltani Szabolcs 524/2 zsim2317
    internal static class Program
    {
        private static IWindow graphicWindow;

        private static GL Gl;

        private static uint program;

        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
		layout (location = 1) in vec4 vCol;

		out vec4 outCol;
        
        void main()
        {
			outCol = vCol;
            gl_Position = vec4(vPos.x, vPos.y, vPos.z, 1.0);
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
            windowOptions.Title = "Lab1-3 (Bónusz)";
            windowOptions.Size = new Silk.NET.Maths.Vector2D<int>(1200, 1200);

            // Window osztály statikus metódusa
            graphicWindow = Window.Create(windowOptions);

            graphicWindow.Load += GraphicWindow_Load;           // Az ablak megjelenésekor fut le EGYSZER
            graphicWindow.Update += GraphicWindow_Update;       // Folyamatos ciklus, ami sokszor fut le egymás után
            graphicWindow.Render += GraphicWindow_Render;

            graphicWindow.Run();
        }

        private static void GraphicWindow_Load()
        {
            // egszeri beallitasokat
            //Console.WriteLine("Loaded");

            Gl = graphicWindow.CreateOpenGL();

            Gl.ClearColor(System.Drawing.Color.White);

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

            if ((ErrorCode)Gl.GetError() != ErrorCode.NoError)
            {
                throw new Exception("ERROR");
            }

        }

        private static void GraphicWindow_Update(double deltaTime)
        {
            // model stuff
            // NO GL -> nincs rajzolás
            // make it threadsave
            //Console.WriteLine($"Update after {deltaTime} [s]");
        }

        //
        // UNSAFE
        // 
        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            //Console.WriteLine($"Render after {deltaTime} [s]");

            Gl.Clear(ClearBufferMask.ColorBufferBit);
            //Gl.Clear((ClearBufferMask)GLEnum.ColorBufferBit); // 16384

            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            float s1 = 2.8f;
            float s2 = 2.1f;

            float minx = -2.0f / s1; //     -0.71
            float maxx = 2.0f / s1;  //      0.71
            float miny = -1.0f / s2; //     -0.47
            float maxy = 1.0f / s2;  //      0.47
            float upy = +2.0f / 3f;  //      0.66
            float zy = 0.25f;        //      0.25
            float dmaxx = +2 / s1 - 0.04f; // -0.67
            float dminx = -2 / s1 + 0.04f; // -0.67
            float dminy = -1 / s2 + 0.09f; // -0.38
            float dy = -2 / 3f;            // -0.66

            float[] vertexArray = new float[]{
                            0.0f, 0.25f, 0.0f,  // 0 ZERO T
                            0.0f, 0.25f, 0.0f,  // 1 ZERO L
                            0.0f, 0.25f, 0.0f,  // 2 ZERO R
                            0.0f, -2/3f, 0.0f, // 3 DOWN L
                            0.0f, -2/3f, 0.0f, // 4 DOWN L
                            0.0f, -2/3f, 0.0f, // 5 DOWN R
                            0.0f, -2/3f, 0.0f, // 6 DOWN R
                            0.0f, +2/3f, 0.0f, // 7 UP
                            -2/s1, +1/s2, 0f,    // 8 UPPER LEFT T
                            -2/s1, +1/s2, 0f,    // 9 UPPER LEFT T
                            -2/s1, +1/s2, 0f,    // 10 UPPER LEFT L
                            -2/s1, +1/s2, 0f,    // 11 UPPER LEFT L
                            +2/s1, +1/s2, 0f,    // 12 UPPER RIGHT T
                            +2/s1, +1/s2, 0f,    // 13 UPPER RIGHT T
                            +2/s1, +1/s2, 0f,    // 14 UPPER RIGHT R
                            +2/s1, +1/s2, 0f,    // 15 UPPER RIGHT R
                            -2/s1+0.04f, -1/s2+0.09f, 0f,     // 16 LOWER LEFT L
                            +2/s1-0.04f, -1/s2+0.09f, 0f,     // 17 LOWER RIGHT R

                            (0 + minx) / 3f, (maxy - zy) / 3f + zy, 0f,         // 18 TOP DOWN 1 
                            (0 + maxx) / 3f * 2, (upy - maxy) / 3f + maxy, 0f,  // 19 TOP UP 1
                            (0 + minx) / 3f * 2, (maxy - zy) / 3f * 2 + zy, 0f, // 20 TOP DOWN 2
                            (0 + maxx) / 3f, (upy - maxy) / 3f * 2 + maxy, 0f,  // 21 TOP UP 2

                            (0 + minx) / 3f * 2, (upy - maxy) / 3f + maxy, 0f,  // 22 TOP LEFT 1 
                            (0 + maxx) / 3f, (maxy - zy) / 3f + zy, 0f,         // 23 TOP RIGHT 1
                            (0 + minx) / 3f, (upy - maxy) / 3f * 2 + maxy, 0f,  // 24 TOP LEFT 2
                            (0 + maxx) / 3f * 2, (maxy - zy) / 3f * 2 + zy, 0f, // 25 TOP RIGHT 2

                            (0 + dminx) / 3f * 2f, (dminy - dy) / 3f * 2f + dy, 0f,  // 26 LEFT DOWN 1 
                            (0 + minx) / 3f * 2, (maxy - zy) / 3f * 2 + zy, 0f,  // 27 LEFT UP 1
                            (0 + dminx) / 3f, (dminy - dy) / 3f + dy, 0f,        // 28 LEFT DOWN 2
                            (0 + minx) / 3f, (maxy - zy) / 3f + zy, 0f,          // 27 LEFT UP 2

                            (minx - dminx) / 3f * 2f + dminx, (maxy - dminy) / 3f * 2f + dminy, 0f,     // 30 LEFT LEFT 1 
                            0f, (dy - zy) / 3f + zy, 0f,                                                // 31 LEFT RIGHT 1
                            (minx - dminx) / 3f + dminx, (maxy - dminy) / 3f + dminy, 0f,               // 32 LEFT LEFT 2
                            0f, (dy - zy) / 3f * 2 + zy, 0f,                                            // 33 LEFT RIGHT 2
                            
                            (0 + dmaxx) / 3f * 2f, (dminy - dy) / 3f * 2f + dy, 0f,  // 26 RIGTH DOWN 2 
                            (0 + maxx) / 3f * 2f, (maxy - zy) / 3f * 2 + zy, 0f,  // 27 RIGHT UP 1
                            (0 + dmaxx) / 3f, (dminy - dy) / 3f + dy, 0f,        // 28 RIGHT DOWN 1
                            (0 + maxx) / 3f, (maxy - zy) / 3f + zy, 0f,          // 27 RIGHT UP 2

                            0f, (dy - zy) / 3f + zy, 0f,                                        // 30 RIGHT LEFT 1 
                            (maxx - dmaxx) / 3f + dmaxx, (maxy - dminy) / 3f * 2f+ dminy, 0f,       // 31 RIGHT RIGHT 1
                            0f, (dy - zy) / 3f * 2 + zy, 0f,                                    // 32 RIGHT LEFT 2
                            (maxx - dmaxx) / 3f + dmaxx, (maxy - dminy) / 3f + dminy, 0f,       // 33 RIGHT RIGHT 2
                            
                            /*
                            float minx = -2.0f / s1; //     -0.71       bal pontok      X
                            float maxx = 2.0f / s1;  //      0.71       jobb pontok     X
                            float miny = -1.0f / s2; //     -0.47       also pontok     Y
                            float maxy = 1.0f / s2;  //      0.47       also pontok     Y
                            float upy = +2.0f / 3f;  //      0.66       felso pont      Y
                            float zy = 0.25f;        //      0.25       ZERO pont       Y
                            float dmaxx = +2 / s1 - 0.04f; // -0.67     jobb also       X
                            float dminx = -2 / s1 + 0.04f; // -0.67     bal also        X
                            float dminy = -1 / s2 + 0.09f; // -0.38     lenti pontok    Y
                            float dy = -2 / 3f;            // -0.66     legalso pont    Y
                            */
            };

            uint[] indexArray = new uint[] {
                            0, 8, 12,
                            9, 7, 13,
                            1, 10, 3,
                            11, 16, 4,
                            2, 14, 5,
                            15, 17, 6
            };

            float[] colorArray = new float[] {
                            0.95f, 0.95f, 0.95f, 1.0f, // 0    TOP
                            1.0f, 0.0f, 0.0f, 1.0f, // 1    LEFT
                            0.3f, 0.52f, 0.91f, 1.0f, // 2    RIGHT
                            1.0f, 0.0f, 0.0f, 1.0f, // 3    LEFT
                            1.0f, 0.0f, 0.0f, 1.0f, // 4    LEFT
                            0.3f, 0.52f, 0.91f, 1.0f, // 5    RIGHT
                            0.3f, 0.52f, 0.91f, 1.0f, // 6    RIGHT
                            0.95f, 0.95f, 0.95f, 1.0f, // 7    TOP
                            0.95f, 0.95f, 0.95f, 1.0f, // 8    TOP
                            0.95f, 0.95f, 0.95f, 1.0f, // 9    TOP
                            1.0f, 0.0f, 0.0f, 1.0f, // 10   LEFT
                            1.0f, 0.0f, 0.0f, 1.0f, // 11   LEFT
                            0.95f, 0.95f, 0.95f, 1.0f, // 12   TOP
                            0.95f, 0.95f, 0.95f, 1.0f, // 13   TOP
                            0.3f, 0.52f, 0.91f, 1.0f, // 14   RIGHT
                            0.3f, 0.52f, 0.91f, 1.0f, // 15   RIGHT
                            1.0f, 0.0f, 0.0f, 1.0f, // 16   LEFT
                            0.3f, 0.52f, 0.91f, 1.0f, // 17   RIGHT
                            0.0f, 0.0f, 0.0f, 1.0f,             // 18
                            0.0f, 0.0f, 0.0f, 1.0f,
                            0.0f, 0.0f, 0.0f, 1.0f,
                            0.0f, 0.0f, 0.0f, 1.0f,
                            0.0f, 0.0f, 0.0f, 1.0f,
                            0.0f, 0.0f, 0.0f, 1.0f,
                            0.0f, 0.0f, 0.0f, 1.0f,
                            0.0f, 0.0f, 0.0f, 1.0f,
                            0.0f, 0.0f, 0.0f, 1.0f,
                            0.0f, 0.0f, 0.0f, 1.0f,
                            0.0f, 0.0f, 0.0f, 1.0f,
                            0.0f, 0.0f, 0.0f, 1.0f,
                            0.0f, 0.0f, 0.0f, 1.0f,
                            0.0f, 0.0f, 0.0f, 1.0f,
                            0.0f, 0.0f, 0.0f, 1.0f,
                            0.0f, 0.0f, 0.0f, 1.0f,
                            0.0f, 0.0f, 0.0f, 1.0f,
                            0.0f, 0.0f, 0.0f, 1.0f,
                            0.0f, 0.0f, 0.0f, 1.0f,
                            0.0f, 0.0f, 0.0f, 1.0f

            };

            

            uint vertices = Gl.GenBuffer();
            // Felcsereljuk elromlik
            //Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            //Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.BufferData(BufferTargetARB.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), BufferUsageARB.StaticDraw); // Ez igy szebb mint felette

            // 1.) Hányadik elemtől indulunk
            // 2.) Mennyit
            // 3.) Milyen tipus
            // 4.) Normalizalja
            // 5.) Stride
            // 6.) null ok
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(0);

            if ((ErrorCode)Gl.GetError() != ErrorCode.NoError)
            {
                throw new Exception("ERROR: in vertices\n");
            }

            uint colors = Gl.GenBuffer();
            /*            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
                        Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            */

            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            //            Gl.EnableVertexAttribArray(0);
            Gl.EnableVertexAttribArray(1);

            if ((ErrorCode)Gl.GetError() != ErrorCode.NoError)
            {
                throw new Exception("ERROR: in getting in the buffer of the colors\n");
            }

            // read only span -> módosítható miután átadtuk paraméterként, de a read only megállítsa ezt
            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            Gl.UseProgram(program);

            if ((ErrorCode)Gl.GetError() != ErrorCode.NoError)
            {
                throw new Exception("ERROR: in getting in the buffer of the indices\n");
            }

            //  Hogyan kellene rajzolnia?

            Gl.DrawElements(GLEnum.Triangles, (uint)18, GLEnum.UnsignedInt, null); // we used element buffer 
            Gl.DrawArrays(GLEnum.Lines, 18, (uint)(vertexArray.Length - 18));
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(vao);

            if ((ErrorCode)Gl.GetError() != ErrorCode.NoError)
            {
                throw new Exception("ERROR: in drawing elements\n");
            }


            

            Gl.DeleteBuffer(vertices);
            Gl.DeleteBuffer(colors);
            Gl.DeleteBuffer(indices);
            Gl.DeleteVertexArray(vao);

            if ((ErrorCode)Gl.GetError() != ErrorCode.NoError)
            {
                throw new Exception("ERROR: in deleting Buffers\n");
            }
        }
    }
}