#define UNI
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNI
using UnityEngine;
#else

#endif
using Scaler = System.Int32;
using System.Linq;
using UnityEngine.Profiling;

#if UNI
#else
using System.Diagnostics;
using Vector2 = System.Numerics.Vector2;
public class MonoBehaviour { }
#endif
public class FillPolygon //: MonoBehaviour
{
    //Vector2[] Poly = new Vector2[]
    //{
    //    new Vector2(185,467),
    //    new Vector2(167,474),
    //    new Vector2(184,476),
    //    new Vector2(227,454),
    //    new Vector2(244,419),
    //    new Vector2(247,382),
    //    new Vector2(263,368),
    //    new Vector2(317,365),
    //    new Vector2(331,370),
    //    new Vector2(350,367),
    //    new Vector2(383,358),
    //    new Vector2(405,341),
    //    new Vector2(436,333),
    //    new Vector2(499,308),
    //    new Vector2(525,308),
    //    new Vector2(582,301),
    //    new Vector2(511,285),
    //    new Vector2(395,311),
    //    new Vector2(365,305),
    //    new Vector2(389,274),
    //    new Vector2(385,265),
    //    new Vector2(359,263),
    //    new Vector2(360,269),
    //    new Vector2(366,270),
    //    new Vector2(350,297),
    //    new Vector2(349,270),
    //    new Vector2(322,264),
    //    new Vector2(322,269),
    //    new Vector2(328,271),
    //    new Vector2(323,297),
    //    new Vector2(292,296),
    //    new Vector2(293,271),
    //    new Vector2(274,267),
    //    new Vector2(271,300),
    //    new Vector2(260,282),
    //    new Vector2(258,265),
    //    new Vector2(233,262),
    //    new Vector2(245,290),
    //    new Vector2(244,309),
    //    new Vector2(229,324),
    //    new Vector2(236,339),
    //    new Vector2(211,370),
    //    new Vector2(222,405),
    //    new Vector2(222,430),
    //    new Vector2(198,453),
    //    new Vector2(170,464),
    //    new Vector2(171,472),
    //    new Vector2(183,468),
    //    new Vector2(185,467),
    //};

    static void printf( string s , params object[ ] p )
    {
#if UNI
        //Debug.Log( string.Format( s , p ) );
#else
        //Trace.WriteLine( string.Format( s , p ) );
#endif
    }

    void glVertex2i( float x , float y , bool IsEdge)
    {
#if UNI
        //Debug.Log(  x + " , " + y );
        //BezierPath.SpawnCube( new Vector3( x / 10.0f , y / 10.0f ) );
        FilledRegion.Add( new Vector3( x , y ) , IsEdge );
#else
        Trace.WriteLine(  x + " , " + y );
#endif
    }

    //https://www.geeksforgeeks.org/scan-line-polygon-filling-using-opengl-c/
    const int maxHt = TexEditor.Size;
    const int maxWd  = 32;
    const int MaxVer = 1000 ;

    FileStream fp;

    // Start from lower left corner 
    class EdgeBucket
    {
        public Scaler ymax;   //max y-coordinate of edge 
        public float xofymin;  //x-coordinate of lowest edge point updated only in aet 
        public float slopeinverse;
        public override string ToString()
        {
            return $"ymax = {ymax} xofymin = {xofymin} slopeinverse = {slopeinverse}";
        }
    }

    class EdgeTableTuple
    {
        // the array will give the scanline number 
        // The edge table (ET) with edges entries sorted  
        // in increasing y and x of the lower end 

        public int countEdgeBucket;    //no. of edgebuckets
        public EdgeBucket[] buckets;
        public EdgeTableTuple( int maxVer )
        {
            buckets = //new EdgeBucket[ maxVer ];
                Enumerable.Range( 0 , maxVer ).Select( i => new EdgeBucket( ) ).ToArray( );
            //System.Array.
            countEdgeBucket = 0;
        }
        public override string ToString()
        {
            return $"countEdgeBucket = {countEdgeBucket} buckets = {buckets.Length}";
        }
    }

    EdgeTableTuple[] EdgeTable ;
    EdgeTableTuple ActiveEdgeTuple = new EdgeTableTuple(MaxVer);

    // Scanline Function 
    void initEdgeTable( )
    {
        int i;
        for ( i = 0 ; i < maxHt ; i++ )
        {
            EdgeTable[ i ].countEdgeBucket = 0;
        }

        ActiveEdgeTuple.countEdgeBucket = 0;
    }


    static void printTuple( EdgeTableTuple tup )
    {
        int j;

        if ( tup.countEdgeBucket > 0 )
        {
            printf( "\nCount {0}-----\n" , tup.countEdgeBucket );
        }

        for ( j = 0 ; j < tup.countEdgeBucket ; j++ )
        {
            printf( " {0} + {1} + {2}" ,
            tup.buckets[ j ].ymax , tup.buckets[ j ].xofymin , tup.buckets[ j ].slopeinverse );
        }
    }

    void printTable( )
    {
        int i,j;

        for ( i = 0 ; i < maxHt ; i++ )
        {
            if ( EdgeTable[ i ].countEdgeBucket > 0 )
                printf( "\nScanline {0}" , i );

            printTuple( EdgeTable[ i ] );
        }
    }


    /* Function to sort an array using insertion sort*/
    void insertionSort( EdgeTableTuple ett )
    {
        int i,j;
        EdgeBucket temp = new EdgeBucket();

        for ( i = 1 ; i < ett.countEdgeBucket ; i++ )
        {
            temp.ymax = ett.buckets[ i ].ymax;
            temp.xofymin = ett.buckets[ i ].xofymin;
            temp.slopeinverse = ett.buckets[ i ].slopeinverse;
            j = i - 1;

            while ( j >= 0 )
            {
                bool v = ( temp.xofymin < ett.buckets[ j ].xofymin );
                if(!v)
                {
                    break;
                }
                ett.buckets[ j + 1 ].ymax = ett.buckets[ j ].ymax;
                ett.buckets[ j + 1 ].xofymin = ett.buckets[ j ].xofymin;
                ett.buckets[ j + 1 ].slopeinverse = ett.buckets[ j ].slopeinverse;
                j = j - 1;
            }
            ett.buckets[ j + 1 ].ymax = temp.ymax;
            ett.buckets[ j + 1 ].xofymin = temp.xofymin;
            ett.buckets[ j + 1 ].slopeinverse = temp.slopeinverse;
        }
    }


    void storeEdgeInTuple( EdgeTableTuple receiver , Scaler ym , Scaler xm , float slopInv )
    {
        // both used for edgetable and active edge table.. 
        // The edge tuple sorted in increasing ymax and x of the lower end. 
        int countEdgeBucket = receiver.countEdgeBucket;
        var buckets = receiver.buckets;
        buckets[ countEdgeBucket ] .ymax = ym;
        buckets[ countEdgeBucket ] .xofymin = xm;
        buckets[ countEdgeBucket ] .slopeinverse = slopInv;

        // sort the buckets 
        insertionSort( receiver );

        ( receiver.countEdgeBucket )++;


    }

    void storeEdgeInTable( Scaler x1 , Scaler y1 , Scaler x2 , Scaler y2 )
    {
        float m,minv;
        Scaler ymaxTS,xwithyminTS, scanline; //ts stands for to store 

        if ( x2 == x1 )
        {
            minv = 0.000000f;
        }
        else
        {
            m = ( ( float )( y2 - y1 ) ) / ( ( float )( x2 - x1 ) );

            // horizontal lines are not stored in edge table 
            if ( y2 == y1 )
                return;

            minv = ( float )1.0 / m;
            //printf( "\nSlope string for {0} {1} & {2} {3}: {4}" , x1 , y1 , x2 , y2 , minv );
        }

        if ( y1 > y2 )
        {
            scanline = y2;
            ymaxTS = y1;
            xwithyminTS = x2;
        }
        else
        {
            scanline = y1;
            ymaxTS = y2;
            xwithyminTS = x1;
        }
        // the assignment part is done..now storage.. 
        //Debug.Log( "storeEdgeInTable scanline " + scanline );
        storeEdgeInTuple( EdgeTable[ scanline ] , ymaxTS , xwithyminTS , minv );


    }

    void removeEdgeByYmax( EdgeTableTuple Tup , int yy )
    {
        int i,j;
        for ( i = 0 ; i < Tup.countEdgeBucket ; i++ )
        {
            if ( Tup.buckets[ i ].ymax == yy )
            {
                //printf( "\nRemoved at {0}" , yy );

                for ( j = i ; j < Tup.countEdgeBucket - 1 ; j++ )
                {
                    Tup.buckets[ j ].ymax = Tup.buckets[ j + 1 ].ymax;
                    Tup.buckets[ j ].xofymin = Tup.buckets[ j + 1 ].xofymin;
                    Tup.buckets[ j ].slopeinverse = Tup.buckets[ j + 1 ].slopeinverse;
                }
                Tup.countEdgeBucket--;
                i--;
            }
        }
    }


    void updatexbyslopeinv( EdgeTableTuple Tup )
    {
        int i;

        for ( i = 0 ; i < Tup.countEdgeBucket ; i++ )
        {
            ( Tup.buckets[ i ] ).xofymin = ( Tup.buckets[ i ] ).xofymin + ( Tup.buckets[ i ] ).slopeinverse;
        }
    }


    void ScanlineFill( )
    {
        /* Follow the following rules: 
        1. Horizontal edges: Do not include in edge table 
        2. Horizontal edges: Drawn either on the bottom or on the top. 
        3. Vertices: If local max or min, then count twice, else count 
            once. 
        4. Either vertices at local minima or at local maxima are drawn.*/


        int i, j, x1, ymax1, x2, ymax2, FillFlag = 0, coordCount;

        // we will start from scanline 0;  
        // Repeat until last scanline: 
        for ( i = 0 ; i < maxHt ; i++ )//4. Increment y by 1 (next scan line) 
        {
            //printf( "Scan {0}" , EdgeTable[ i ].countEdgeBucket );
            // 1. Move from ET bucket y to the 
            // AET those edges whose ymin = y (entering edges) 
            for ( j = 0 ; j < EdgeTable[ i ].countEdgeBucket ; j++ )
            {
                storeEdgeInTuple( ActiveEdgeTuple , EdgeTable[ i ].buckets[ j ].
                         ymax , ( int )EdgeTable[ i ].buckets[ j ].xofymin ,
                        EdgeTable[ i ].buckets[ j ].slopeinverse );
            }
            //printTuple( ActiveEdgeTuple );

            // 2. Remove from AET those edges for  
            // which y=ymax (not involved in next scan line) 
            removeEdgeByYmax( ActiveEdgeTuple , i );

            //sort AET (remember: ET is presorted) 
            insertionSort( ActiveEdgeTuple );

            //printTuple( ActiveEdgeTuple );

            //3. Fill lines on scan line y by using pairs of x-coords from AET 
            j = 0;
            FillFlag = 0;
            coordCount = 0;
            x1 = 0;
            x2 = 0;
            ymax1 = 0;
            ymax2 = 0;
            while ( j < ActiveEdgeTuple.countEdgeBucket )
            {
                if ( coordCount % 2 == 0 )
                {
                    x1 = ( int )( ActiveEdgeTuple.buckets[ j ].xofymin );
                    ymax1 = ActiveEdgeTuple.buckets[ j ].ymax;
                    if ( x1 == x2 )
                    {
                        /*three cases can arrive- 
                            1. lines are towards top of the intersection 
                            2. lines are towards bottom 
                            3. one line is towards top and other is towards bottom 
                        */
                        if ( ( ( x1 == ymax1 ) && ( x2 != ymax2 ) ) || ( ( x1 != ymax1 ) && ( x2 == ymax2 ) ) )
                        {
                            x2 = x1;
                            ymax2 = ymax1;
                        }

                        else
                        {
                            coordCount++;
                        }
                    }

                    else
                    {
                        coordCount++;
                    }
                }
                else
                {
                    x2 = ( int )ActiveEdgeTuple.buckets[ j ].xofymin;
                    ymax2 = ActiveEdgeTuple.buckets[ j ].ymax;

                    FillFlag = 0;

                    // checking for intersection... 
                    if ( x1 == x2 )
                    {
                        /*three cases can arive- 
                            1. lines are towards top of the intersection 
                            2. lines are towards bottom 
                            3. one line is towards top and other is towards bottom 
                        */
                        if ( ( ( x1 == ymax1 ) && ( x2 != ymax2 ) ) || ( ( x1 != ymax1 ) && ( x2 == ymax2 ) ) )
                        {
                            x1 = x2;
                            ymax1 = ymax2;
                        }
                        else
                        {
                            coordCount++;
                            FillFlag = 1;
                        }
                    }
                    else
                    {
                        coordCount++;
                        FillFlag = 1;
                    }


                    if ( FillFlag > 0 )
                    {
                        //drawing actual lines... 
                        //glColor3f( 0.0f , 0.7f , 0.0f );
                        //glVertex2i( 999 , 999 );
                        //glBegin( GL_LINES );
                        glVertex2i( x1 , i , true );
                        glVertex2i( x2 , i , true );
                        //glEnd( );
                        //glFlush( );

                        // printf("\nLine drawn from %d,%d to %d,%d",x1,i,x2,i); 
                    }

                }

                j++;
            }


            // 5. For each nonvertical edge remaining in AET, update x for new y 
            updatexbyslopeinv( ActiveEdgeTuple );
        }
        //printf( "\nScanline filling complete" );

    }

    void drawPolyDino( List<Vector3> poly )
    {
        //glColor3f( 1.0f , 0.0f , 0.0f );
        int count = 0;
        Scaler x1 = 0,y1=0,x2=0,y2=0;
        // rewind( fp );
        //  while ( !feof( fp ) )
        foreach ( var item in poly )
        {
            count++;
            Vector2 p = item;//Poly[ count - 1 ];

            if ( count > 2 )
            {
                x1 = x2;
                y1 = y2;
                count = 2;
            }
            if ( count == 1 )
            {
                //fscanf( fp , "%d,%d" , x1 , y1 );
#if UNI
                x1 = (int)p.x;
                y1 = (int)p.y;
#else
                x1 = (int)p.X;
                y1 = (int)p.Y;
#endif

            }
            else
            {
                //fscanf( fp , "%d,%d" , x2 , y2 );
#if UNI
                x2 = (int)p.x;
                y2 = (int)p.y;
#else
                x2 = (int)p.X;
                y2 = (int)p.Y;
#endif

                //printf( "\n{0},{1}" , x2 , y2 );
                //glBegin( GL_LINES );
                glVertex2i( x1 , y1 , false );
                glVertex2i( x2 , y2 , false );
                //glEnd( );
                storeEdgeInTable( x1 , y1 , x2 , y2 );//storage of edges in edge table. 
                //glFlush( );
            }
        }
    }

    public FilledPoly MakeFilledPoly(List<Vector3> poly)
    {
        FilledRegion.Clear( );

        CustomSampler edgeTablesampler = CustomSampler.Create("initEdgeTable");
        CustomSampler drawDinoSampler = CustomSampler.Create("drawDino");
        CustomSampler scanFillSampler = CustomSampler.Create("scanFillSampler");
        edgeTablesampler.Begin( );
        initEdgeTable( );
        edgeTablesampler.End( );

        drawDinoSampler.Begin( );
        drawPolyDino( poly );
        drawDinoSampler.End( );
        //printf( "\nTable" );
        //printTable( );
        scanFillSampler.Begin( );
        ScanlineFill( );//actual calling of scanline filling.. 
        scanFillSampler.End( );
        return FilledRegion;
    }

    public void drawDino( )
    {
        //var poly3D = Poly.Select( v => new Vector3( v.x , v.y ) ).ToList();
        //MakeFilledPoly( poly3D );
    }


    FilledPoly FilledRegion = new FilledPoly();
    private void Start( )
    {
        EdgeTable = //new EdgeTableTuple[ maxHt ];
            Enumerable.Range( 0 , maxHt ).Select( i => new EdgeTableTuple( MaxVer ) ).ToArray( );
        //drawDino( );
    }
    public FillPolygon()
    {
        Start( );
    }

}
public struct Scanline
{
    public int X;
    public int Y;
    public int W;

    public static List<Scanline> ScanlineList( List<Vector3> edge )
    {
        var list = new List<Scanline>(edge.Count);
        for ( int i = 0 ; i < edge.Count ; i += 2 )
        {
            Vector3 line = edge[ i ];
            Vector3 nextPoint = edge[ i + 1 ];
            //Debug.Log(  "ScanlineList : " + TexEditor.FrameCount + " : " + line.y );
            //Debug.Assert( line.y == nextPoint.y , "linex = " + line.y + "nextPointx = " + nextPoint.y);
            var scan = new Scanline()
            {
                X = (int)line.x,
                Y = (int)line.y,
                W = (int)nextPoint.x - (int)line.x,
            };
            list.Add( scan );
        }
        return list;
    }

    public static Color[ ] FillPolyWithColor( List<Vector3> edge , int Size , bool isFill = true)
    {
        Color[] baseColor = new Color[Size*Size];
        for ( int i = 0 ; i < edge.Count ; i += 2 )
        {
            Vector3 point1 = edge[ i ];
            Vector3 point2 = edge[ i + 1 ];
            var x1 = (int)point1.x;
            var x2 = (int)point2.x;
            var y  = (int)point1.y;
            // 最適化のためにこの中で作成
            // 1024でリアルタイムで濡れるのが目標、1024だとライン出し始めると負荷が出る
            // Yでソートされているので、同じYで2度以上出てきても計算が安い
            if ( isFill )
            {
                for ( int xInd = x1 ; xInd < x2 ; xInd++ )
                {
                    baseColor[ xInd + y * Size ] = Color.white;
                }
            }
            else
            {
                baseColor[ x1 + y * Size ] = Color.white;
                baseColor[ x2 + y * Size ] = Color.white;

            }
        }
        return baseColor;

    }
}

public class FilledPoly
{

    List<Vector3> InsidePoly = new List<Vector3>();
    List<Vector3> EdgePoly = new List<Vector3>();
    public void Add( Vector3 p , bool isInside )
    {
        if ( isInside )
        {
            InsidePoly.Add( p );
        }
        else
        {
            EdgePoly.Add( p );
        }
    }

    public void Clear( )
    {
        InsidePoly.Clear( );
        EdgePoly.Clear( );
    }

    // 遅いはず
    public List<Scanline> InsideLine
    {
        get
        {
            return Scanline.ScanlineList( InsidePoly );
        }
    }
    // 多分高速版
    public Color[ ] InsideLineColor( int Size , bool isFill = true)
    {
        return Scanline.FillPolyWithColor( InsidePoly , Size , isFill );
    }
}


