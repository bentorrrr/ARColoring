using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;
using Image = Vuforia.Image;

public class UvMapping : MonoBehaviour
{  
    public GameObject RawIm;
    public GameObject TextureIm;
    public GameObject ImageTarget;
    public GameObject MainCam;
    private Camera cam;
    private Texture ExtractedTexture;
    public float imageHeight, imageWidth;
    public SkinnedMeshRenderer Renderer;
    private Matrix4x4 originalProjection, inversedProjection;
    private List<Vector2> Coord2D = new List<Vector2>();
    public List<GameObject> Coord = new List<GameObject>(); // 0 = LowerLeft, 1 = LowerRight, 2 = UpperRight, 3 = UpperLeft
    
    private int[,] Permutation = {{-1,-1}, {1,-1}, {1,1}, {-1,1}};
    
    void Start()
    {
        cam = MainCam.GetComponent<Camera>();
        originalProjection = cam.projectionMatrix;
        inversedProjection = originalProjection.inverse;

        for( int i = 0; i < 4; i++)
        {
            Debug.Log(cam.WorldToScreenPoint(Coord[i].transform.position));
            Coord[i].transform.position = new Vector3(ImageTarget.transform.position.x + (imageWidth * Permutation[i,0] * 0.0001f), ImageTarget.transform.position.y, ImageTarget.transform.position.z + (imageHeight * Permutation[i,1] * 0.0001f));
        }
    }

    void Update()
    {        
        for( int i = 0; i < 4; i++)
        {
            Coord[i].transform.position = new Vector3(ImageTarget.transform.position.x + (imageWidth * Permutation[i,0] * 0.0001f), ImageTarget.transform.position.y, ImageTarget.transform.position.z + (imageHeight * Permutation[i,1] * 0.0001f));
        }
        #region Calculating Projection
        for( int i = 0; i < 4; i++)
        {
            var temp = cam.WorldToScreenPoint(Coord[i].transform.position);
            temp.x = temp.x ; // / (Screen.width);
            temp.y = temp.y ; // / (Screen.height);
            Debug.Log("Temp Value: " + temp);
            Debug.Log("Temp Diveded: " + temp.x/640 + " " + temp.y/480);
            Coord2D.Insert(i, new Vector2(temp.x, temp.y)); // Inserting coordinates in 2D (x,y) to array
        }
        #endregion
        
        for( int i = 0; i < 4; i++)
        {
            RawImage rawImage = RawIm.GetComponent<RawImage>();
            Texture2D texture = (rawImage.texture as Texture2D); // Casting Texture to Texture2D
            // texture.Reinitialize(1920,1080); // Trying to resize texture but didn't have any effect on the render texture
            Debug.Log("Texture Size: " + texture.width + " " + texture.height);

            //             
            var Rectangle = RectFromCoordinates(Coord2D.ToArray());
            Debug.Log("Rect: " + Rectangle);
            texture.ReadPixels((Rectangle), 0, 0, true);
            texture.Apply();

            // Casting an processed texture to another raw image;
            RawImage TextIm = TextureIm.GetComponent<RawImage>();
            TextIm.texture = texture;
            TextIm.material.mainTexture = texture;

            Renderer.material.mainTexture = texture;
        }
    
        // var croppedTexture = texture;
        // // ExtractedTexture = RawIm.GetComponent<RawImage>().texture;

        // // var croppedTexture = new Texture2D( (int)ExtractedTexture.width, (int)ExtractedTexture.height );

        // // var pixels = ExtractedTexture.GetPixels( Coord2D[0], Coord2D[1], Coord2D[2], Coord2D[3]);

        // // croppedTexture.SetPixels( pixels );
        // // croppedTexture.Apply();
    }


    Rect RectFromCoordinates(Vector2[] coords)
    {
        // Find the minimum and maximum x and y values
        float minX = Mathf.Min(coords[0].x, coords[1].x, coords[2].x, coords[3].x);
        float maxX = Mathf.Max(coords[0].x, coords[1].x, coords[2].x, coords[3].x);
        float minY = Mathf.Min(coords[0].y, coords[1].y, coords[2].y, coords[3].y);
        float maxY = Mathf.Max(coords[0].y, coords[1].y, coords[2].y, coords[3].y);

        float width = maxX - minX;
        float height = maxY - minY;

        return new Rect(minX, minY, width, height);
    }

    public Vector2 ProjectTo2DWithScale(Vector3 point3D)
    {
        float nearPlaneDistance = Camera.main.nearClipPlane;
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(point3D);
        return screenPoint;
    }

    
}
