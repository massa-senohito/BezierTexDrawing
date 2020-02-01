using OpenCvSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoordValue = System.Int32;
// C:\Cv241\opencv\sources\modules\core\src\drawing.cpp
// ThickLine( Mat& img, Point p0, Point p1, const void* color,
// 参考にしてもいい気がするがちょっと長そうなので

public class OCVStrokePath 
    //: MonoBehaviour
{

    public static void MakePixel( List<Vector3> edge , int lineWidth ,Texture2D texture )
    {
        Mat mat = OpenCvSharp.Unity.TextureToMat(texture);
        for ( int i = 0 ; i < edge.Count - 1 ; i++ )
        {
            Vector3 point1 = edge[ i ];
            Vector3 point2 = edge[ i + 1 ];
            var x1 = (CoordValue)point1.x;
            var x2 = (CoordValue)point2.x;
            var y1 = texture.height - (CoordValue)point1.y;
            var y2 = texture.height - (CoordValue)point2.y;
            {
                Cv2.Line( mat , x1 , y1 , x2 , y2 , Scalar.White , lineWidth , LineTypes.AntiAlias );
            }
        }
        OpenCvSharp.Unity.MatToTexture (mat , texture);
    }

}
