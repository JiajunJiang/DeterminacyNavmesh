using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Linq;
using Google.Protobuf;

public class NewBehaviourScript
{
    /// <summary>
    /// 精度
    /// </summary>
    private static int Precision { get;  } = 100;

    [MenuItem("Tools/GenerateNavMesh")]
    public static void GenerateNavMesh()
    {
        var rawMesh = NavMesh.CalculateTriangulation();
        var rawMeshVertices = rawMesh.vertices;
        var rawMeshIndices = rawMesh.indices;
        var verticeList = new List<Point3D>();
        var indiceList = new List<int>();
        var removeList = new List<int>();

        for (int i = 0; i < rawMeshVertices.Length - 1; i++)
        {
            for (int j = i + 1; j < rawMeshVertices.Length; j++)
            {
                if ((rawMeshVertices[i] - rawMeshVertices[j]).magnitude * Precision > 1)
                    continue;
                if (removeList.Contains(j))
                    continue;
                for (int k = 0; k < rawMeshIndices.Length; k++)
                {
                    if (rawMeshIndices[k] == j)
                        rawMeshIndices[k] = i;
                }

                removeList.Add(j);
            }
        }

        for (var k = 0; k < rawMeshIndices.Length; k++)
        {
            rawMeshIndices[k] = rawMeshIndices[k] - removeList.Count(x => x < rawMeshIndices[k]);
            indiceList.Add(rawMeshIndices[k]);
        }

        for (var i = 0; i < rawMeshVertices.Length; i++)
        {
            if (removeList.Contains(i)) continue;
            verticeList.Add(new Point3D(rawMeshVertices[i] * Precision));
        }

        var info = new NavMeshFileInfo();

        foreach (var point3D in verticeList)
        {
            info.Vertices.Add(new Point()
            {
                X = point3D.x,
                Y = point3D.y,
                Z = point3D.z
            });
        }

        foreach (var indice in indiceList)
        {
            info.Indices.Add(indice);
        }

        var bytes = info.ToByteArray();

        var scene = SceneManager.GetActiveScene();
        string outPath = Application.dataPath + "/Resources/NavMesh/" + scene.name + ".bytes";
        if (File.Exists(outPath))
        {
            File.Delete(outPath);
        }

        FileStream fs = new FileStream(outPath, FileMode.OpenOrCreate);
        fs.Write(bytes, 0, bytes.Length);
        fs.Flush();
        fs.Close();
        fs.Dispose();
        Debug.Log($"{outPath} GenerateNavMesh Successful");

        AssetDatabase.Refresh();
    }
}