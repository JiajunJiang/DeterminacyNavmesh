namespace DefaultNamespace
{
    public class NavmeshSystem : Singleton<NavmeshSystem>
    {
        /// <summary>
        /// Same to Editor Precision
        /// </summary>
        public static int Precision = 100;
        
        public Point3D[] vertices;
        public int[] indices;
        public int[] lines;

        public void Init(byte[] bytes)
        {
            NavMeshFileInfo info = NavMeshFileInfo.Parser.ParseFrom(bytes);
            vertices = new Point3D[info.Vertices.Count];
            indices = new int[info.Indices.Count];
            lines = new int[info.Lines.Count];

            for (var index = 0; index < info.Vertices.Count; index++)
            {
                var vertex = info.Vertices[index];
                vertices[index] = new Point3D(vertex.X, vertex.Y, vertex.Z);
            }

            for (var index = 0; index < info.Indices.Count; index++)
            {
                var infoIndex = info.Indices[index];
                indices[index] = infoIndex;
            }

            for (var index = 0; index < info.Lines.Count; index++)
            {
                var line = info.Lines[index];
                lines[index] = line;
            }
            
            //todo nodes
        }
    }
}