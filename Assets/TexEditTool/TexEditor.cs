//#define MOUSEDEBUG
#define SCANLINE
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using UnityEngine.Profiling;

public class TexEditor : MonoBehaviour {
    Texture2D EditTex;

    [SerializeField]
    Material TexMat;
    [SerializeField]
    Material ModelMat;

    public const int Size = 
        //16;
        //2048;
        //1024;
        512;
    private const string Path1 = "svgExported.txt";

    //256;
    //32;

    [SerializeField]
    BezierPath Path;

    string TexPath()
    {
        var path = "";
        var editTex = (Texture2D)TexMat.mainTexture;
#if UNITY_EDITOR
        path = AssetDatabase.GetAssetPath( editTex );
#endif
        return path;
    }

#region FPS_COUNT
    // https://www.sejuku.net/blog/82841
    int frameCount;
    float prevTime;
    float fps;
#endregion
    void Start( )
    {
        #region FPS_COUNT
        frameCount = 0;
        prevTime = 0.0f;
        #endregion

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

        TexDataList = new Color[ EditTex.width * EditTex.height ];
        BaseColor = Enumerable.Range(0 , Size * Size).Select(i => Color.black).ToArray();
        FillColor =  Enumerable.Range(0 , Size * Size).Select(i => Color.green).ToArray();
        //MakeTextTexture( );
        TexMat.mainTexture = EditTex;
        ModelMat.mainTexture = EditTex;
    }

    // テスト用
    private void MakeTextTexture( )
    {
        var color = Enumerable.Range(0 , Size * Size).Select(i => Random.ColorHSV());
        //Debug.Log( color.Count( ) );
        EditTex.SetPixels(color.ToArray() , 0 );
        EditTex.Apply( );
        TexMat.mainTexture = EditTex;
    }

    Vector3 PixelToWorld(Vector3 pos)
    {
        var worldMat = transform.localToWorldMatrix;

        var worldPos = worldMat.MultiplyPoint(pos / Size);
        var offset = new Vector3
            (
            (   worldPos.x - 1.0f ) ,
            ( - worldPos.y + 1.0f ) , 0);

        return offset;
    }

    Vector3 WorldToPixel( Vector3 pos )
    {
        // ポリゴン化したパスのワールド座標が来る
        // テクスチャのピクセル座標系に射影する
        var localMat = transform.worldToLocalMatrix;
        var localPos = localMat.MultiplyPoint(pos);

        // 左上 -0.5 0.5 migisita 0.5 -0.5
        var offset = new Vector3
            (
            ( localPos.x + 0.5f ) * Size,
            ( localPos.y + 0.5f ) * Size, 0);
        //Debug.Log( "Mapper offset : " + offset );
        return offset;
    }

    Color[] BaseColor;
    Color[] FillColor;
    Color[] TexDataList;
    public static int FrameCount;
    // 1024なら5がちょうど
    int SkipCount = 1;
    [SerializeField]
    bool IsFill;

    // Update is called once per frame
    void FixedUpdate( )
    {
        #region FrameSkip
        FrameCount++;
        // 最初の200フレームはロードで負荷がかかる
        bool isSlow = fps < 30.0f;
        if( isSlow 
            //&& FrameCount > 30
            )
        {
            if ( FrameCount % 15 == 0 )
            {
                SkipCount++;
                //Debug.Log( $"TexEditor SkipAdd : {SkipCount} fps {fps} FrameCount {FrameCount}" );
            }
        }
        if(FrameCount % SkipCount != 0)
        {
            return;
        }
        #endregion

#if MOUSEDEBUG
        Vector3 screenPos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        Debug.Log( worldPos );
        DrawTexByWorld(worldPos);
#endif
#if SCANLINE

        CustomSampler sampler = CustomSampler.Create("fillpoly");
        CustomSampler setPixelSample = CustomSampler.Create("setPixelSample");
        //"パスをピクセル空間に持っていく"
        // AddPointするときに持っていく -> AddPoint座標系が複雑になる
        // ここらで32倍 -> 2回Addしちゃうが大きく負荷はなかった
        if ( IsFill )
        {
            sampler.Begin( );
            var colorList =
                Path.FillPoly( WorldToPixel );
                //Path.StrokePath( WorldToPixel );
            sampler.End( );

            setPixelSample.Begin( );
#if NOOPE
        EditTex.SetPixels( BaseColor , 0 );
        for ( int i = 0 ; i < enumerable.Count ; i++ )
        {
            Scanline item = enumerable[ i ];
            //Debug.Log( "itemW =" + item.W );
            //var colorList = Enumerable.Range( 0 , item.W ).Select( i => Color.green ).ToArray();

            int v = item.X + item.W;
            bool xInBound = 0 < item.X && v < Size;
            bool yInBound = 0 < item.Y && item.Y < Size;

            if ( xInBound && yInBound )
            {
                EditTex.SetPixels( item.X , item.Y , item.W , 1 , FillColor );
            }
        }
        //EditTex.SetPixels( BaseColor , 0 );
#else
            EditTex.SetPixels( colorList , 0 );
#endif
            setPixelSample.End( );
#else
        int height = EditTex.height;
        // 1の大きさになる
        for ( int y = 0 ; y < height ; y++ )
        {
            int width = EditTex.width;
            for ( int x = 0 ; x < width ; x++ )
            {
                var widOff = x / (float)width;
                var heiOff = y / (float)height;
                var localPos = new Vector3(widOff - 0.5f , heiOff - 0.5f , 0);
                var worldMat = transform.localToWorldMatrix;
                var worldPos = worldMat.MultiplyPoint(localPos);
                var drawColor = Color.green;
                if ( Path.IsContain( worldPos ) )
                {
                    drawColor = Color.red;
                }
                TexDataList[ x + height * y ] = drawColor;
            }
        }
        EditTex.SetPixels( TexDataList );
#endif
            EditTex.Apply( );
        }
        else
        {
            EditTex.SetPixels( BaseColor );
            Path.StrokePathOCV( WorldToPixel , EditTex );
        }
    }

    void Update( )
    {
        #region FPS_COUNT
        frameCount++;
        float time = Time.realtimeSinceStartup - prevTime;

        if ( time >= 0.5f )
        {
            fps = frameCount / time;
            //Debug.Log( fps );

            frameCount = 0;
            prevTime = Time.realtimeSinceStartup;
        }
        #endregion

        if ( Input.GetKeyDown( KeyCode.S ) )
        {
            File.WriteAllText( Path1 , Path.SVGData( WorldToPixel ) );
        }
        if ( Input.GetKeyDown( KeyCode.L ) )
        {
            OnLoadSVG( Path1 );
        }
    }

    private void OnPos( string csv )
    {
        var posList = csv.Split( ',' ).Select(float.Parse).ToArray();
        var pos = PixelToWorld( 
            new Vector3( posList[ 0 ] , posList[ 1 ] , 0 ) 
            )
            ;
        BezierPath.SpawnCube( pos );
    }
    void OnLoadSVG(string path)
    {
        //var allText = File.ReadAllText( path );
        //var indOfFirstM = allText.IndexOf( "d=\"" );
        //if ( indOfFirstM == -1 )
        //{
        //    Debug.Log( "svg not contain d element. load failed." );
        //    return;
        //}
        var maySv = Svg.load( path );
        if(maySv.IsRight)
        {
            Debug.Log( "loaded" );
        }
        maySv.Case( ls=> Svg.mapData(ls, OnPos), er => Debug.Log( er.ToString( ) ) );
    }

    private void DrawTexByWorld( Vector3 worldPos )
    {
        Vector3 canvasCord = GetCanvasCord( worldPos );
        //Debug.Log( canvusCord );
        var xPos = canvasCord.x * Size;
        var yPos = canvasCord.y * Size -1.0f;
        //Debug.Log( xPos + "," + yPos );
        EditTex.SetPixel( ( int )xPos , ( int )yPos , Color.green );
    }

    private Vector3 GetCanvasCord( Vector3 worldPos )
    {
        var canvasLocal  = transform.worldToLocalMatrix;
        var canvasCenter = canvasLocal.MultiplyPoint(worldPos);
        var canvasCord = canvasCenter + new Vector3(0.5f , -0.5f , 1.6f);
        return canvasCord;
    }

    private void OnDestroy( )
    {
        var png = EditTex.EncodeToPNG( );
        File.WriteAllBytes( "textWriteTex.png" , png );
    }
}
