using System.Globalization;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace zsim2317_lab4_1
{
    internal class ObjResourceReader
    {

        private static bool hasFaceNormals { get; set; } = false;
        public static unsafe GlObject Create3DObjectWithColor(GL Gl, float[] faceColor, string objName)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            List<float[]> objVertices;
            List<float[]> objVertexNormals;
            List<int[]> objFaces;
            List<int[]> objFaceNormals;
            ReadObjDataForObject(out objVertices, out objVertexNormals, out objFaces, out objFaceNormals, objName);

            List<float> glVertices = new List<float>();
            List<float> glColors = new List<float>();
            List<uint> glIndices = new List<uint>();

            if (hasFaceNormals)
                CreateGlArraysFromObjArraysWithNormals(faceColor, objVertices, objVertexNormals, objFaces, objFaceNormals, glVertices, glColors, glIndices);
            else
                CreateGlArraysFromObjArrays(faceColor, objVertices, objFaces, glVertices, glColors, glIndices);
            return CreateOpenGlObject(Gl, vao, glVertices, glColors, glIndices);
        }

        private static unsafe GlObject CreateOpenGlObject(GL Gl, uint vao, List<float> glVertices, List<float> glColors, List<uint> glIndices)
        {
            uint offsetPos = 0;
            uint offsetNormal = offsetPos + (3 * sizeof(float));
            uint vertexSize = offsetNormal + (3 * sizeof(float));

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)glVertices.ToArray().AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
            Gl.EnableVertexAttribArray(0);

            Gl.EnableVertexAttribArray(2);
            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetNormal);

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)glColors.ToArray().AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)glIndices.ToArray().AsSpan(), GLEnum.StaticDraw);

            // release array buffer
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            uint indexArrayLength = (uint)glIndices.Count;

            return new GlObject(vao, vertices, colors, indices, indexArrayLength, Gl);
        }

        private static unsafe void CreateGlArraysFromObjArraysWithNormals(float[] faceColor, List<float[]> objVertices, List<float[]> objVertexNormals, List<int[]> objFaces, List<int[]> objFaceNormals, List<float> glVertices, List<float> glColors, List<uint> glIndices)
        {
            Dictionary<string, int> glVertexIndices = new Dictionary<string, int>();

            foreach (var pair in objFaces.Zip(objFaceNormals, (face, normal) => new { Face = face, Normal = normal }))
            {
                var aObjVertex = objVertices[pair.Face[0] - 1];
                var a = new Vector3D<float>(aObjVertex[0], aObjVertex[1], aObjVertex[2]);
                var bObjVertex = objVertices[pair.Face[1] - 1];
                var b = new Vector3D<float>(bObjVertex[0], bObjVertex[1], bObjVertex[2]);
                var cObjVertex = objVertices[pair.Face[2] - 1];
                var c = new Vector3D<float>(cObjVertex[0], cObjVertex[1], cObjVertex[2]);

                var aObjVertexNormal = objVertices[pair.Normal[0] - 1];
                var aNormal = new Vector3D<float>(aObjVertexNormal[0], aObjVertexNormal[1], aObjVertexNormal[2]);
                var bObjVertexNormal = objVertexNormals[pair.Normal[1] - 1];
                var bNormal = new Vector3D<float>(bObjVertexNormal[0], bObjVertexNormal[1], bObjVertexNormal[2]);
                var cObjVertexNormal = objVertexNormals[pair.Normal[2] - 1];
                var cNormal = new Vector3D<float>(cObjVertexNormal[0], cObjVertexNormal[1], cObjVertexNormal[2]);


                // process 3 vertices
                for (int i = 0; i < pair.Face.Length; ++i)
                {
                    var objVertex = objVertices[pair.Face[i] - 1];
                    var objVertexNormal = objVertices[pair.Normal[i] - 1];

                    // create gl description of vertex
                    List<float> glVertex = new List<float>();
                    glVertex.AddRange(objVertex);
                    glVertex.AddRange(objVertexNormal);
                    // add textrure, color

                    // check if vertex exists
                    var glVertexStringKey = string.Join(" ", glVertex);
                    if (!glVertexIndices.ContainsKey(glVertexStringKey))
                    {
                        glVertices.AddRange(glVertex);
                        glColors.AddRange(faceColor);
                        glVertexIndices.Add(glVertexStringKey, glVertexIndices.Count);
                    }

                    // add vertex to triangle indices
                    glIndices.Add((uint)glVertexIndices[glVertexStringKey]);
                }
            }
        }

        private static unsafe void CreateGlArraysFromObjArrays(float[] faceColor, List<float[]> objVertices, List<int[]> objFaces, List<float> glVertices, List<float> glColors, List<uint> glIndices)
        {
            Dictionary<string, int> glVertexIndices = new Dictionary<string, int>();

            foreach (var objFace in objFaces)
            {
                var aObjVertex = objVertices[objFace[0] - 1];
                var a = new Vector3D<float>(aObjVertex[0], aObjVertex[1], aObjVertex[2]);
                var bObjVertex = objVertices[objFace[1] - 1];
                var b = new Vector3D<float>(bObjVertex[0], bObjVertex[1], bObjVertex[2]);
                var cObjVertex = objVertices[objFace[2] - 1];
                var c = new Vector3D<float>(cObjVertex[0], cObjVertex[1], cObjVertex[2]);

                var normal = Vector3D.Normalize(Vector3D.Cross(b - a, c - a));

                // process 3 vertices
                for (int i = 0; i < objFace.Length; ++i)
                {
                    var objVertex = objVertices[objFace[i] - 1];

                    // create gl description of vertex
                    List<float> glVertex = new List<float>();
                    glVertex.AddRange(objVertex);
                    glVertex.Add(normal.X);
                    glVertex.Add(normal.Y);
                    glVertex.Add(normal.Z);
                    // add textrure, color

                    // check if vertex exists
                    var glVertexStringKey = string.Join(" ", glVertex);
                    if (!glVertexIndices.ContainsKey(glVertexStringKey))
                    {
                        glVertices.AddRange(glVertex);
                        glColors.AddRange(faceColor);
                        glVertexIndices.Add(glVertexStringKey, glVertexIndices.Count);
                    }

                    // add vertex to triangle indices
                    glIndices.Add((uint)glVertexIndices[glVertexStringKey]);
                }
            }
        }

        private static unsafe void ReadObjDataForObject(out List<float[]> objVertices, out List<float[]> objVertexNormals, out List<int[]> objFaces, out List<int[]> objFaceNormals, string objName)
        {
            objVertices = new List<float[]>();
            objVertexNormals = new List<float[]>();
            objFaces = new List<int[]>();
            objFaceNormals = new List<int[]>();
            using (Stream objStream = typeof(ObjResourceReader).Assembly.GetManifestResourceStream(objName))
            using (StreamReader objReader = new StreamReader(objStream))
            {
                while (!objReader.EndOfStream)
                {
                    var line = objReader.ReadLine();

                    if (line.Length > 1 && !line.StartsWith('#') && (line[0] == 'f' || line[0] == 'v'))
                    {
                        var lineClassifier = line.Substring(0, line.IndexOf(' '));
                        var lineData = line.Substring(lineClassifier.Length).Trim().Split(' ');

                        switch (lineClassifier)
                        {
                            case "v":
                                float[] vertex = new float[3];
                                for (int i = 0; i < vertex.Length; ++i)
                                    vertex[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                                objVertices.Add(vertex);
                                break;
                            case "f":
                                int[] face = new int[3];
                                int[] faceNormal = new int[3];
                                for (int i = 0; i < face.Length; ++i)
                                {
                                    if (lineData[i].Split('/').Length == 3)
                                    {
                                        hasFaceNormals = true;
                                        face[i] = int.Parse(lineData[i].Split('/')[0]);
                                        faceNormal[i] = int.Parse(lineData[i].Split('/')[2]);
                                    }
                                    else
                                    {
                                        face[i] = int.Parse(lineData[i]);
                                    }
                                }
                                objFaces.Add(face);
                                objFaceNormals.Add(faceNormal);
                                break;
                            case "vn":
                                hasFaceNormals = true;
                                float[] vertexNormal = new float[3];
                                for (int i = 0; i < vertexNormal.Length; ++i)
                                    vertexNormal[i] = float.Parse(lineData[i], CultureInfo.InvariantCulture);
                                objVertexNormals.Add(vertexNormal);
                                break;
                        }
                    }
                }
            }
        }
    }
}
