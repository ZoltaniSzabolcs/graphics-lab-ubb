using System;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

/*  Zoltani Szabolcs
 *  zsim2317
 *  524/2*/

namespace Lab1
{
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
            windowOptions.Title = "1. szeminárium -  repülő háromszög";
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

            float[] vertexArray = new float[] {
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
                            -2/s1+0.09f, -1/s2+0.09f, 0f,    // 16 LOWER LEFT L
                            +2/s1-0.09f, -1/s2+0.09f, 0f     // 17 LOWER RIGHT R

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
                            192/255f, 192/255f, 192/255f, 1.0f, // TOP
                            128/255f, 128/255f, 128/255f, 1.0f, // LEFT
                            204/255f, 229/255f, 255/255f, 1.0f, // RIGHT
                            128/255f, 128/255f, 128/255f, 1.0f, // LEFT
                            128/255f, 128/255f, 128/255f, 1.0f, // LEFT
                            204/255f, 229/255f, 255/255f, 1.0f, // RIGHT
                            204/255f, 229/255f, 255/255f, 1.0f, // RIGHT
                            192/255f, 192/255f, 192/255f, 1.0f, // TOP
                            192/255f, 192/255f, 192/255f, 1.0f, // TOP
                            192/255f, 192/255f, 192/255f, 1.0f, // TOP
                            128/255f, 128/255f, 128/255f, 1.0f, // LEFT
                            128/255f, 128/255f, 128/255f, 1.0f, // LEFT
                            192/255f, 192/255f, 192/255f, 1.0f, // TOP
                            192/255f, 192/255f, 192/255f, 1.0f, // TOP
                            204/255f, 229/255f, 255/255f, 1.0f, // RIGHT
                            204/255f, 229/255f, 255/255f, 1.0f, // RIGHT
                            128/255f, 128/255f, 128/255f, 1.0f, // LEFT
                            204/255f, 229/255f, 255/255f, 1.0f // RIGHT
                            
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

            Gl.DrawElements(GLEnum.Triangles, (uint)indexArray.Length, GLEnum.UnsignedInt, null); // we used element buffer
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(vao);

            if ((ErrorCode)Gl.GetError() != ErrorCode.NoError)
            {
                throw new Exception("ERROR: in drawing elements\n");
            }

            // always unbound the vertex buffer first, so no halfway results are displayed by accident
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