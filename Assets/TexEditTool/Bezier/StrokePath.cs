//#define DEBUG_STROKE
using System;
using System.Collections.Generic;
using UnityEngine;
using CoordValue =
    System.Single;
    //System.Int32;
public class StrokePath
#if DEBUG_STROKE
    : MonoBehaviour 
#endif
{
    // http://members.chello.at/~easyfilter/bresenham.html
    static CoordValue abs( CoordValue x )
    {
        return Math.Abs( x );
    }

    static float sqrt( float x )
    {
        double v = Math.Sqrt( x );
        return ( float )v;
    }

    static float round( float x )
    {
        double v = Math.Round( x );
        return ( float )v;
    }

    static float max( float x , float y )
    {
        return Math.Max( x , y );
    }

    static bool equ(float a , float b)
    {
        return //Mathf.Approximately( a , b );
        Mathf.Abs( a - b ) < 0.5f;
    }

    Action<int,int,float> OnSetPixel;

    Color[ ] PixelList;
    int Size;

    public StrokePath(
        //Action<int,int,float> onSetPixel
        int size
        )
    {
        //OnSetPixel = onSetPixel;
        PixelList = new Color[size * size];
        Size = size;
    }

    void setPixelColor( CoordValue x , CoordValue y , float v )
    {
        //OnSetPixel( x , y , v );
        //Trace.WriteLine( $"setPixelSample {x} {y} {v}" );
        var v1plot = (int)x + (int)y * Size;
        if( PixelList.Length <= v1plot || v1plot < 0)
        {
            return;
        }
        PixelList[ v1plot ] = Color.white * (v / 255.0f);

#if DEBUG_STROKE
        Debug.Log( $"setPixelSample {x} {y} {v}" );
        var unit = BezierPath.SpawnCube( new UnityEngine.Vector3( x , y , 2 ) );
        var alp = (v / 255.0f);
        unit.SetTransparent( );
        unit.SetColor( new UnityEngine.Color( 122 , 1 , 1 , alp ) );
#else
        //Debug.Log( $"setPixelSample {x} {y} {v}" );
#endif
    }

    public void plotLineWidth( CoordValue x0 , CoordValue y0 , CoordValue x1 , CoordValue y1 , float wd )//, Color[] pixel , int size)
    {
        var dx = abs(x1-x0);
        var sx = x0 < x1 ? 1 : -1;
        var dy = abs(y1-y0);
        var sy = y0 < y1 ? 1 : -1;
        var err = dx-dy;
        CoordValue e2, x2, y2;                          /* error value e_xy */
        float ed = dx+dy == 0 ? 1 : sqrt((float)dx*dx+(float)dy*dy);

        for ( wd = ( wd + 1 ) / 2 ; ; )
        {                                   /* pixel loop */
            float color = 255 * ( abs( err - dx + dy ) / ed - wd + 1 );
            //pixel[x0+ size * y0 ]
            setPixelColor( x0 , y0 , max( 0 , color ) );
            e2 = err; x2 = x0;
            float e2Color = 255 * ( abs( e2 ) / ed - wd + 1 );
            if ( 2 * e2 >= -dx )
            {                                           /* x step */
                y2 = y0;
                bool v = ( y1 != y2 || dx > dy );
                for ( e2 += dy ; e2 < ed * wd && v ; e2 += dx )
                {
                    CoordValue y = y2 += sy;
                    setPixelColor( x0 , y , max( 0 , e2Color ) );
                }
                if ( x0 == x1 ) break;
                e2 = err; err -= dy; x0 += sx;
            }
            if ( 2 * e2 <= dy )
            {                                            /* y step */
                bool v = ( x1 != x2 || dx < dy );
                for ( e2 = dx - e2 ; e2 < ed * wd && v ; e2 += dy )
                {
                    CoordValue x = x2 += sx;
                    setPixelColor( x , y0 , max( 0 , e2Color ) );
                }
                if ( y0 == y1 ) break;
                err += dx; y0 += sy;
            }
        }
    }

    public Color[ ] MakePixel( List<Vector3> edge , bool isThickPath , int lineWidth)
    {
        Array.Clear( PixelList , 0 , Size * Size );
        // 偶数のときも奇数のときもある
        var loopMinus = edge.Count % 2 == 0 ? 0 : 1;
        for ( int i = 0 ; i < edge.Count - 1; i ++ )
        {
            Vector3 point1 = edge[ i ];
            Vector3 point2 = edge[ i + 1 ];
            var x1 = (CoordValue)point1.x;
            var x2 = (CoordValue)point2.x;
            var y1 = (CoordValue)point1.y;
            var y2 = (CoordValue)point2.y;
            if ( isThickPath )
            {
                plotLineWidth2( x1 , y1 , x2 , y2 , lineWidth );
            }
            else
            {
                var v1plot = (int)x1 + (int)y1 * Size;
                var v2plot = (int)x2 + (int)y2 * Size;
                PixelList[ v1plot ] = Color.white;
                PixelList[ v2plot ] = Color.white;
            }

            //plotLineWidth( x1 , y1 , x2 , y2 , lineWIdth , baseColor , size);

        }
        return PixelList;

    }

    void plotLineAA( CoordValue x0 , CoordValue y0 , CoordValue x1 , CoordValue y1 )
    {             /* draw a black (0) anti-aliased line on white (255) background */
        CoordValue dx = abs(x1-x0), sx = x0 < x1 ? 1 : -1;
        CoordValue dy = abs(y1-y0), sy = y0 < y1 ? 1 : -1;
        CoordValue err = dx-dy, e2, x2;                               /* error value e_xy */
        var ed = equ(dx+dy , 0) ? 1 : sqrt(dx*dx+dy*dy);

        for ( ; ; )
        {                                                 /* pixel loop */
            setPixelColor( x0 , y0 , 255.0f * abs( err - dx + dy ) / ed );
            e2 = err; x2 = x0;
            if ( 2 * e2 >= -dx )
            {                                            /* x step */
                if ( equ(x0 , x1 ) ) break;
                if ( e2 + dy < ed ) setPixelColor( x0 , y0 + sy , 255.0f * ( e2 + dy ) / ed );
                err -= dy; x0 += sx;
            }
            if ( 2.0f * e2 <= dy )
            {                                             /* y step */
                if ( equ(y0 , y1 )) break;
                if ( dx - e2 < ed ) setPixelColor( x2 + sx , y0 , 255.0f * ( dx - e2 ) / ed );
                err += dx; y0 += sy;
            }
        }
    }
    public void plotLineWidth2( CoordValue x0 , CoordValue y0 , CoordValue x1 , CoordValue y1 , CoordValue th )
    {                              /* plot an anti-aliased line of width th pixel */
        CoordValue dx = abs(x1-x0), sx = x0 < x1 ? 1 : -1;
        CoordValue dy = abs(y1-y0), sy = y0 < y1 ? 1 : -1;
        CoordValue err, e2 = (CoordValue)sqrt(dx*dx+dy*dy);                            /* length */

        if ( th <= 1 || equ(e2 , 0 ) )
        {
            plotLineAA( x0 , y0 , x1 , y1 );         /* assert */
            return;
        }
        dx *= (CoordValue)255.0f / e2;
        dy *= (CoordValue)255.0f / e2;
        th  = (CoordValue)255.0f * ( th - (CoordValue)1.0f );               /* scale values */

        if ( dx < dy )
        {                                               /* steep line */
            x1 = (CoordValue)round( ( e2 + th / 2.0f ) / dy );                          /* start offset */
            err = x1 * dy - th / 2.0f;                  /* shift error value to offset width */
            for ( x0 -= x1 * sx ; ; y0 += sy )
            {
                setPixelColor( x1 = x0 , y0 , err );                  /* aliasing pre-pixel */
                for ( e2 = dy - err - th ; e2 + dy < 255 ; e2 += dy )
                    setPixelColor( x1 += sx , y0 , 255 );                      /* pixel on the line */
                setPixelColor( x1 + sx , y0 , e2 );                    /* aliasing post-pixel */
                if ( equ(y0 , y1 ) ) break;
                err += dx;                                                 /* y-step */
                if ( err > 255 ) { err -= dy; x0 += sx; }                    /* x-step */
            }
        }
        else
        {                                                      /* flat line */
            y1 = (CoordValue)round( ( e2 + th / 2.0f ) / dx );                          /* start offset */
            err = y1 * dx - th / 2.0f;                  /* shift error value to offset width */
            for ( y0 -= y1 * sy ; ; x0 += sx )
            {
                setPixelColor( x0 , y1 = y0 , err );                  /* aliasing pre-pixel */
                for ( e2 = dx - err - th ; e2 + dx < 255 ; e2 += dx )
                    setPixelColor( x0 , y1 += sy , 255 );                      /* pixel on the line */
                setPixelColor( x0 , y1 + sy , e2 );                    /* aliasing post-pixel */
                if ( equ( x0 , x1 ) ) break;
                err += dy;                                                 /* x-step */
                if ( err > 255 ) { err -= dx; y0 += sy; }                    /* y-step */
            }
        }
    }
#if DEBUG_STROKE
    private void Start( )
    {
        var ten = 10;
        var twen = ten * 2;
        var thir = ten * 3;
        plotLineWidth2( ten , ten , twen , thir , 5 );

    }
#endif
}
