#define EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class TexEditor : MonoBehaviour {
    Texture2D EditTex;
    [SerializeField]
    Material TexMat;
    int Size = 16;

    string TexPath()
    {
        var path = "";
        var editTex = (Texture2D)TexMat.mainTexture;
#if EDITOR
        path = AssetDatabase.GetAssetPath( editTex );
#endif
        return path;
    }

    void Start( )
    {
        var editTex = (Texture2D)TexMat.mainTexture;
        if ( editTex == null )
        {
            EditTex = new Texture2D( Size , Size );
        }
        else
        {
            EditTex = new Texture2D( editTex.width , editTex.height );

            var path = TexPath();
            var fileData = File.ReadAllBytes( path );
            EditTex.LoadImage( fileData );
        }
        MakeTextTexture( );
    }

    private void MakeTextTexture( )
    {
        var color = Enumerable.Range(0 , Size * Size).Select(i => Random.ColorHSV());
        //Debug.Log( color.Count( ) );
        EditTex.SetPixels(color.ToArray() , 0 );
        EditTex.Apply( );
        TexMat.mainTexture = EditTex;
    }

    // Update is called once per frame
    void FixedUpdate( )
    {
        Vector3 screenPos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        var canvusLocal  = transform.worldToLocalMatrix;
        var canvusCenter = canvusLocal.MultiplyPoint(worldPos);
        var canvusCord = canvusCenter + new Vector3(0.5f , -0.5f , 1.6f);
        //Debug.Log( canvusCord );
        var xPos = canvusCord.x * Size;
        var yPos = canvusCord.y * Size -1.0f;
        Debug.Log( xPos  + "," + yPos);
        EditTex.SetPixel( (int)xPos , (int)yPos , Color.green );
        EditTex.Apply( );
    }
    private void OnDestroy( )
    {
        var png = EditTex.EncodeToPNG( );
        File.WriteAllBytes( "textWriteTex.png" , png );

    }
}
