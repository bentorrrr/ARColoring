using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;
using Image = Vuforia.Image;

public class UvMapping : MonoBehaviour
{
    public GameObject RawIm;
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
            temp.x = temp.x / (Screen.width/192);
            temp.y = temp.y / (Screen.height/108);
            Debug.Log(temp);
            Coord2D.Insert(i, new Vector2(temp.x, temp.y));
        }
        #endregion
        
        for( int i = 0; i < 4; i++)
        {
            if(Coord2D[i].x < 0 || Coord2D[i].x > 192 || Coord2D[i].y < 0 || Coord2D[i].y > 108)
            {
                return;
            }          
            RawImage rawImage = RawIm.GetComponent<RawImage>();
            Texture2D texture = new Texture2D((int)rawImage.rectTransform.rect.width, (int)rawImage.rectTransform.rect.height);
            var Rectangle = RectFromCoordinates(Coord2D.ToArray());
            Debug.Log(Rectangle);

            texture.ReadPixels((Rectangle), 0, 0, true);
            texture.Apply();

            Renderer.material.mainTexture = texture;
        }
    
        // var croppedTexture = texture;
        // // ExtractedTexture = RawIm.GetComponent<RawImage>().texture;

        // // var croppedTexture = new Texture2D( (int)ExtractedTexture.width, (int)ExtractedTexture.height );

        // // var pixels = ExtractedTexture.GetPixels( Coord2D[0], Coord2D[1], Coord2D[2], Coord2D[3]);

        // // croppedTexture.SetPixels( pixels );
        // // croppedTexture.Apply();
    }

    // Texture2D RawImageToTexture2D(RawImage rawImage)
    // {
    //     Texture2D texture = new Texture2D((int)rawImage.rectTransform.rect.width, (int)rawImage.rectTransform.rect.height);
    //     texture.ReadPixels(new Rect(0, 0, rawImage.rectTransform.rect.width, rawImage.rectTransform.rect.height), 0, 0);
    //     texture.Apply();
    //     return texture;
    // }

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
