#define  CUBETEST

using UnityEngine;
using System.Linq;
using UniRx;
using System.Collections.Generic;
using Optional;
using Unit =
    //UnityEngine.GameObject;
    EntityBase;
using Gobj = UnityEngine.GameObject;
using V3 = UnityEngine.Vector3;

public class BezierPath : MonoBehaviour
{

    public class Bezier
    {
        public List<V3> pathPoints;
        private int segments;
        public int pointCount;

        public Bezier( )
        {
            pathPoints = new List<V3>( );
            pointCount = 100;
        }

        public void DeletePath( )
        {
            pathPoints.Clear( );
        }

        V3 BezierPathCalculation( V3 p0 , V3 p1 , V3 p2 , V3 p3 , float t )
        {
            float tt = t * t;
            float ttt = t * tt;
            float u = 1.0f - t;
            float uu = u * u;
            float uuu = u * uu;

            V3 B = new V3();
            B = uuu * p0;
            B += 3.0f * uu * t * p1;
            B += 3.0f * u * tt * p2;
            B += ttt * p3;

            return B;
        }

        public void CreateCurve( List<V3> controlPoints )
        {
            segments = controlPoints.Count / 3;

            for ( int s = 0 ; s < controlPoints.Count - 3 ; s += 3 )
            {
                V3 p0 = controlPoints[s];
                V3 p1 = controlPoints[s+1];
                V3 p2 = controlPoints[s+2];
                V3 p3 = controlPoints[s+3];

                if ( s == 0 )
                {
                    pathPoints.Add( BezierPathCalculation( p0 , p1 , p2 , p3 , 0.0f ) );
                }

                for ( int p = 0 ; p < ( pointCount / segments ) ; p++ )
                {
                    float t = (1.0f / (pointCount/segments)) * p;
                    V3 point = new V3 ();
                    point = BezierPathCalculation( p0 , p1 , p2 , p3 , t );
                    pathPoints.Add( point );
                }
            }
        }
    }

    public static int Len<T>(List<T> list)
    {
        return list.Count;
    }

    public static int Len<T>(T[] list)
    {
        return list.Length;
    }

    private void createLine( float lineSize , Color c )
    {
        LineRenderer lines = GetComponent<LineRenderer>();
        if ( lines == null )
        {
            lines = gameObject.AddComponent<LineRenderer>( );
        }
        
        lines.material = shader; //= new Material( shader );
        //lines.material.color = c;
        lines.useWorldSpace = true;
        lines.startWidth = lineSize;
        lines.endWidth = lineSize;
        lines.positionCount = path.pathPoints.Count -1 ;
        //Debug.Log( "posC : " + lines.positionCount );
        //Debug.Log( "pC : " + path.pointCount );
        for ( int i = 1 ; i < ( path.pointCount ) ; i++ )
        {
            V3 start = path.pathPoints[i-1];
            V3 end = path.pathPoints[i];
            lines.SetPosition( i - 1 , start );
            if(lines.positionCount <= i)
            {
                return;
            }
            lines.SetPosition( i , end );
        }
    }

    private void UpdatePath( )
    {
        List<V3> c = new List<V3>();
        for ( int o = 0 ; o < Len(objects) ; o++ )
        {
            if ( objects[ o ] != null )
            {
                V3 p = objects[o].transform.position;
                c.Add( p );
            }
        }
        path.DeletePath( );
        path.CreateCurve( c );
    }

    private int canvasIndex = 0;
    public Material shader;
    Bezier path = new Bezier();
    public List<Unit> objects;

    Unit NearObject( V3 pos , Unit obj0 , Unit obj1 )
    {
        var near0 = V3.Distance(pos ,
            obj0.transform.position);
        var near1 = V3.Distance(pos ,
            obj1.transform.position);
        var temp = near0 < near1 ? obj0 : obj1;
        return temp;
    }

    // 0 3 6 9 .. がパスの橋
    Option<Unit> NearestHandle( V3 pos )
    {
        var nearestObj = Option.None<Unit>();
        float nearestDistance = 0;

        for ( int o = 0 ; o < objects.Count ; o+=3 )
        {
            var obj0 = objects[ o + 1 ];
            var obj1 = objects[ o + 2 ];
            var obj = NearObject(pos,obj0,obj1);
            //if ( obj1 != null )
            {
                V3 p = objects[o].transform.position;
                float dist = V3.Distance( pos , p );
                if(!nearestObj.HasValue)
                {
                    nearestObj = obj.Some();
                    nearestDistance = dist;
                    continue;
                }

                if(dist < nearestDistance)
                {
                    nearestObj = obj.Some();
                    nearestDistance = dist;
                }
            }
        }
        return nearestObj;
    }

    Option<Unit> NearestObj( V3 pos , out float distance)
    {
        var nearestObj = Option.None<Unit>();
        float nearestDistance = 0;
        distance = nearestDistance;
        for ( int o = 0 ; o < objects.Count ; o++ )
        {
            var obj = objects[ o ];
            //if ( obj1 != null )
            {
                V3 p = objects[o].transform.position;
                float dist = V3.Distance( pos , p );
                if(!nearestObj.HasValue)
                {
                    nearestObj = obj.Some();
                    nearestDistance = dist;
                    distance = dist;
                    continue;
                }

                if(dist < nearestDistance)
                {
                    nearestObj = obj.Some();
                    nearestDistance = dist;
                    distance = dist;
                }
            }
        }
        return nearestObj;
    }

    public static float CameraOffset = 2;

    public static Unit SpawnCube( V3 pos )
    {
        var obj = Gobj.CreatePrimitive(PrimitiveType.Cube);
        obj.transform.position = pos;
        var p = obj.transform.position;
        obj.transform.position = new V3( p.x , p.y , CameraOffset);
        obj.transform.localScale = new V3( 0.1f , 0.1f , 0.1f);
        var unit = obj.AddComponent<Unit>( );
        return unit;
    }

    void MoveAround( V3 world , V3 orig , Unit target )
    {
        target.transform.position = orig + orig - world;
    }

    int Counter = 0;
    class HandleContainer
    {
        Unit[] HandleObjList;
        public HandleContainer(Unit[] objects)
        {
            HandleObjList = objects;
        }

        public void SetActive(bool isActive)
        {
            TryGetPreHandle( ).MatchSome( obj => obj.SetActive( isActive ) );
            var handle = GetNextHandle( );
            handle.SetActive( isActive );
        }

        public Unit Point()
        {
            if(HandleObjList.Length == 2)
            {
                return HandleObjList[ 0 ];
            }
            return HandleObjList[ 1 ];
        }

        public string PointName
        {
            get
            {
                return Point( ).name;
            }
        }

        Option<Unit> TryGetPreHandle()
        {
            if(HandleObjList.Length == 3)
            {
                return HandleObjList[ 0 ].Some();
            }
            return HandleObjList[ 0 ].None( );
        }

        Unit GetNextHandle( )
        {
            if ( HandleObjList.Length == 2 )
            {
                return HandleObjList[ 1 ];
            }
            return HandleObjList[ 2 ];
        }
    }

    // 掴んでいるハンドルからラインを表示するための
    // 通常時ハンドルは隠れている、ポイントをクリックすればハンドルが表示される
    Dictionary< int , HandleContainer > IDHandleMap 
        = new Dictionary< int , HandleContainer >();

    void AddPathPoint(V3 pos)
    {
        // その箇所に橋とハンドル追加、trlお品柄だと点対象位置にハンドルを移動
        // 点対象位置は中心 * 2 - 移動先
        if ( objects.Count == 0 )
        {
            var cpoint1 = SpawnCube(pos);
            cpoint1.name = "cpointA," + Counter;
            var handle01 = SpawnCube(pos);
            handle01.name = "handleB," + Counter;

            Unit[ ] collection = new[ ] {
                cpoint1 ,
                handle01 ,
            };
            AddPointAndHandle( collection );
            return;
        }

        var cpoint = SpawnCube(pos);
        cpoint.name = "cpointC," + Counter;
        var handle0 = SpawnCube(pos);
        handle0.name = "handleD," + Counter;
        var handle1 = SpawnCube(pos);
        handle1.name = "handleE," + Counter;
        Unit[ ] collection1 = new[ ] {
            handle0 ,
            cpoint ,
            handle1 ,
        };
        AddPointAndHandle( collection1 );
    }

    private void AddPointAndHandle( Unit[ ] collection )
    {

        IDHandleMap.Add( Counter , new HandleContainer( collection ) );
        objects.AddRange( collection );
        Counter++;
        // 他のクリックされていないオブジェクトを消す
        UpdateSelection( collection[ 0 ].name );
    }

    void MoveLastHandle(V3 world)
    {
        bool isFirst = Len( objects ) < 3;
        var prev = isFirst ? 1 : 3;
        var nextHandle = Len(objects) - 1;
        var prevHandle = Len(objects) - prev;
        var point = Len(objects) - 2;
        var preObj = objects[prevHandle];
        var obj = objects[point].transform.position;
        MoveAround( world , obj , preObj );
        if ( isFirst )
        {
            return;
        }
        var nexObj = objects[nextHandle];
        nexObj.transform.position = world;
    }

    Option<int> MayActiveBezierId;

    void OnActiveObjChanged(Unit unit)
    {
        var nameIDList = unit.name.Split( ',' );
        if ( nameIDList.Length == 0 )
        {
            Debug.LogError( "name not contain ," + unit.name);
        }
        var nameId = nameIDList.Last( );
        UpdateSelection( nameId );
    }

    private void UpdateSelection( string nameId )
    {
        var id = 0;
        // クリックされてないオブジェクトをすべて消す
        MayActiveBezierId.MatchSome
            ( SetDeactive );

        if ( int.TryParse( nameId , out id ) )
        {
            var activeObjList = IDHandleMap[ id ];
            activeObjList.SetActive( true );
            MayActiveBezierId = id.Some( );
        }
    }

    private void SetDeactive( int oldId )
    {
        HandleContainer handleContainer = IDHandleMap[ oldId ];
        handleContainer.SetActive( false );
        Debug.Log( "SetDeactive " + oldId + " : " + handleContainer.PointName );
    }

    // Use this for initialization
    void Start( )
    {
        //UpdatePath( );
        State = new BezierState( OnAddPoint , OnMoveHandle , OnMoveLastHandle );
        State.ActiveObjectChanged.Subscribe( OnActiveObjChanged );

#if CUBETEST
        TestCubeList = new Unit[ Size , Size ];
        var parent = new Gobj();
        var upar = Unit.SetEntity( parent );
        upar.name = "TestParent";
        for ( int i = 0 ; i < Size ; i++ )
        {
            for ( int j = 0 ; j < Size ; j++ )
            {
                var cube = SpawnCube( new V3 ( i , j ) );
                cube.SetOffset( -5 , -5 , 1.5f );
                cube.SetParent( upar , true );
                cube.SetUnitScale( 0.5f );
                TestCubeList[ i , j ] = cube;
            }
        }
#endif

    }

    public static V3 WorldMousePosition()
    {
        V3 screenPos = Input.mousePosition;
        V3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        return new V3( worldPos.x , worldPos.y , CameraOffset );
    }

    void OnAddPoint(V3 v)
    {
        AddPathPoint( v );
    }

    void OnMoveHandle(MoveHandleValue v)
    {
        foreach(var obj in v.Obj)
        {
            obj.transform.position = v.WorldPos;
        }

    }
    void OnMoveLastHandle(MoveHandleValue v)
    {
        foreach ( var item in v.Obj )
        {
            UpdateSelection( item.name );
        }
        MoveLastHandle( v.WorldPos );
    }

    BezierState State;
    [SerializeField]
    BezierState.BezierInputState InputState;

    private void Update( )
    {
        V3 worldPos = BezierPath.WorldMousePosition();
        float dist = 1;
        var mayNearObj = NearestObj( worldPos ,out dist);
#if DEBUGDIST
        foreach ( var item in objects )
        {
            item.GetComponent<MeshRenderer>( ).material.color = Color.white;
        }
        mayNearObj.MatchSome( ( obj ) => obj.GetComponent<MeshRenderer>( ).material.color = Color.red );
#endif
        State.Tick( mayNearObj , dist );

    }

#if CUBETEST
    const int Size = 16;
    Unit[,] TestCubeList ;
#endif

    public void FixedUpdate( )
    {
        InputState = State.InputState;

        UpdatePath( );
        for ( int i = 1 ; i < ( path.pointCount ) ; i++ )
        {
            int count = path.pathPoints.Count;
            if ( count <= i)
            {
                return;
            }
            V3 startv = path.pathPoints[i-1];
            V3 endv = path.pathPoints[i];
            createLine( 0.05f , Color.blue );
        }
#if CUBETEST
        for ( int i = 0 ; i < Size ; i++ )
        {
            for ( int j = 0 ; j < Size ; j++ )
            {
                var cube = TestCubeList[i , j];
                var point = cube.GetPos();
                if ( PathSelector.IsIn( PathSelector.Contains(path.pathPoints , point ) ) )
                {
                    cube.SetColor(Color.red);
                }
                else
                {
                    cube.SetColor( Color.black);
                }
            }
        }
#endif

    }

    public IEnumerable<V3> PathPolygon( )
    {
        UpdatePath( );
        for ( int i = 1 ; i < ( path.pointCount ) ; i++ )
        {
            V3 startv = path.pathPoints[i-1];
            V3 endv = path.pathPoints[i];
            yield return startv;
            yield return endv;
            //createLine( startv , endv , 0.25f , Color.blue );
        }

    }

    void OnDrawGizmos( )
    {
        // これで消してしまう
        return;
        UpdatePath( );
        for ( int i = 1 ; i < ( path.pointCount )  ; i++ )
        {
            if(path.pathPoints.Count >= i)
            {
                //return;
            }
            V3 startv = path.pathPoints[i-1];
            V3 endv = path.pathPoints[i];
            Gizmos.color = Color.blue;
            Gizmos.DrawLine( startv , endv );
        }
    }
}
