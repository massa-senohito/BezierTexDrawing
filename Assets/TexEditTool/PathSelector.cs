using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Point = UnityEngine.Vector3;
public class PathSelector : MonoBehaviour
{
    public enum InOut
    {
        OUT,
        ON,
        IN
    }
    public static bool IsIn(InOut inout)
    {
        return inout == InOut.IN;
    }

    //435
    static float cross( Point p , Point q ) { return p.x * q.y - p.y * q.x; }
    static float dot( Point p , Point q ) { return p.x * q.x + p.y * q.y; }
    const double EPS = 1e-8;

    static int sign( double x )
    {
        if ( x < -EPS ) return -1;
        if ( x > +EPS ) return +1;
        return 0;
    }

    public static InOut Contains( List<Point> ps , Point p )
    {
        bool isIn = false;
        for ( int i = 0 ; i < ps.Count; ++i )
        {
            int j = (i+1 == ps.Count ? 0 : i+1);
            Point a = ps[i] - p, b = ps[j] - p;
            if ( a.y > b.y ) {
                var temp = a;
                a = b;
                b = temp;
            }
            if ( a.y <= 0 && 0 < b.y && cross( a , b ) < 0.0f )
            {
                isIn = !isIn;
            }
            if ( sign( cross( a , b ) ) ==0 && sign( dot( a , b ) ) <= 0 )
                return InOut.ON; // point on the edge
        }
        return isIn ? InOut.IN : InOut.OUT; // point in:out the polygon
    }

    int Size = 16;
    [SerializeField]
    BezierPath Bezier;
#if MOVEAROUNDTEST
    GameObject orig;
    GameObject tenTai;
    void Start( )
    {
        //TestContain( );
        orig = BezierPath.SpawnCube( new Vector3( 2 , 5 , 2 ) );
        tenTai = BezierPath.SpawnCube( Vector3.zero );
    }

    private void FixedUpdate( )
    {
        Vector3 screenPos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        var oPos = orig.transform.position;
        worldPos.z = 2;
        tenTai.transform.position = oPos + oPos - 
            worldPos;
    }
#endif
    void Start( )
    {
        //TestContain( );
    }


    private void TestContain( )
    {
        var poly = new Point[]{
            new Point(3,8) ,
            new Point(10,2) ,
            new Point()
        };
        MakeTestCube( poly.ToList() );
    }

    private void MakeTestCube( List<Point> poly )
    {
        for ( int i = 0 ; i < Size ; i++ )
        {
            for ( int j = 0 ; j < Size ; j++ )
            {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Point vector2 = new Point( i , j );
                cube.transform.position = vector2;
                if ( Contains( poly , vector2 ) == InOut.IN )
                {
                    cube.transform.position = new Vector3( 9 , 9 , 9 );
                }
            }
        }
    }

    // Update is called once per frame
    void Update( )
    {

    }
}
