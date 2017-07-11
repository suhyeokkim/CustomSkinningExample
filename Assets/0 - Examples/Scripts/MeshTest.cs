using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTest : MonoBehaviour
{
    [SerializeField]
    private bool isCube;

    public void ModifyMesh(Mesh mesh)
    {
        if (isCube)
        {
            /*

                            (0,1,1) 6           7 (1,1,1)
                                    * --------- *
                                / .         / |
                                /   .       /   |
                            /     .     /     |
                            /       .   /       |
                (0,1,0) 4 / (1,1,0) 5 /         |
                        * --------- * 2 (1,0,1) *
                        |         . |         / 3 (1,0,1)
                        |       .   |       /  
                        |     .     |     /    
                        |   .       |   /      
                        | .         | /        
                        * --------- *
                (0,0,0) 0           1 (1,0,0)

            */

            mesh.vertices =
                        new Vector3[]
                        {
                            new Vector3(0f, 0f, 0f),
                            new Vector3(1f, 0f, 0f),
                            new Vector3(0f, 0f, 1f),
                            new Vector3(1f, 0f, 1f),
                            new Vector3(0f, 1f, 0f),
                            new Vector3(1f, 1f, 0f),
                            new Vector3(0f, 1f, 1f),
                            new Vector3(1f, 1f, 1f),
                        };


            mesh.triangles = new int[]
                                {
                                    // left
                                    0, 6, 4,
                                    0, 2, 6,
                                    // right
                                    1, 5, 7,
                                    1, 7, 3,
                                    // bottom
                                    0, 3, 2,
                                    0, 1, 3,
                                    // top
                                    4, 6, 7,
                                    4, 7, 5,
                                    // back
                                    0, 4, 5,
                                    0, 5, 1,
                                    // front
                                    2, 7, 6,
                                    2, 3, 7,
                                };

            mesh.uv = new Vector2[]
                      {
                          new Vector2(0f, 0f),
                          new Vector2(1f, 0f),
                          new Vector2(0f, 1f),
                          new Vector2(1f, 1f),
                          new Vector2(0f, 0f),
                          new Vector2(1f, 0f),
                          new Vector2(0f, 1f),
                          new Vector2(1f, 1f),
                      };
        }
        else
        {
            /*

                (0,0,1) 2   3 (1,0,1)
                        * - *
                        | / |
                        * - *
                (0,0,0) 0   1 (1,0,0)

            */
            mesh.vertices =
                        new Vector3[]
                        {
                        new Vector3(0f, 0f, 0f),
                        new Vector3(1f, 0f, 0f),
                        new Vector3(0f, 0f, 1f),
                        new Vector3(1f, 0f, 1f)
                        };


            mesh.triangles = new int[]
                                {
                                0, 2, 3,
                                0, 3, 1,
                                };

            mesh.uv = new Vector2[]
                      {
                          new Vector2(0f, 0f),
                          new Vector2(1f, 0f),
                          new Vector2(0f, 1f),
                          new Vector2(1f, 1f),
                      };
        }
    }
}
