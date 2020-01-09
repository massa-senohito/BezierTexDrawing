using Optional;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unit = UnityEngine.GameObject;
using Gobj = UnityEngine.GameObject;
using V3 = UnityEngine.Vector3;
using UniRx;
using AddPointValue = 
    UnityEngine.Vector3;

public class MoveHandleValue
{
    public List<Gobj> Obj;
    public V3 WorldPos;
    public bool IsMoved( V3 v )
    {
        return v != WorldPos;
    }
}

public class BezierState
{
    List<Unit> Holding = new List<Unit>();
    enum BezierInputState
    {
        Normal,
        MoveLastHandle,
        ObjMove,
    }

    BezierInputState InputState;

    void MoveState(BezierInputState state)
    {
        if(InputState == state)
        {
            return;
        }
        Debug.Log( "MoveState : " + state );
        InputState = state;
    }

    bool IsPressedOld;

    MoveHandleValue OldMoved = new MoveHandleValue();
    //イベントを発行する核となるインスタンス
    public Subject<AddPointValue> AddPointSubject = new Subject<AddPointValue>();
    public Subject< MoveHandleValue > MoveHandleSubject =
        new Subject< MoveHandleValue >();
    public Subject< MoveHandleValue > MoveLastHandleSubject =
        new Subject< MoveHandleValue >();
    public Subject< Unit > ActiveObjectChanged =
        new Subject< Unit >();

    public BezierState(
        System.Action<AddPointValue> addPoint ,
        System.Action<MoveHandleValue> moveHandle ,
        System.Action<MoveHandleValue> moveLastHandle
        )
    {
        AddPointSubject.Subscribe( addPoint );
        MoveHandleSubject.Subscribe( moveHandle );
        MoveLastHandleSubject.Subscribe( moveLastHandle );
    }

    public void Tick(Option<Unit> mayObj , float dist)
    {
        bool onclick = Input.GetMouseButtonDown( 0 );
        bool clicking = Input.GetMouseButton( 0 );
        bool isCtrl = Input.GetKey(KeyCode.LeftControl);
        V3 worldPos = BezierPath.WorldMousePosition();
#if OLD
        //if ( onclick && !isCtrl && !isNearObj )
        {
            AddPathPoint( worldPos );
        }
        if ( onclick && isCtrl )
        {
            // 近いのを動かす
        }
       // if(clicking && !isNearObj)
        {
            //MoveHandle( worldPos );
        }
#endif

        bool isHolding = Holding.Count > 0;
        float nearDist = 0.08f;
        switch ( InputState )
        {
            case BezierInputState.Normal:
                // クリックしたとき、周りに何もない
                bool isNear = dist < nearDist;
                if ( isNear )
                {
                    isNear = mayObj.HasValue;
                }
                if ( mayObj.HasValue && isNear && onclick)
                {
                    var obj = mayObj.ValueOr(()=>null);
                    MoveState( BezierInputState.ObjMove );
                    ActiveObjectChanged.OnNext( obj );
                    Debug.Log( "StartMove : " + obj.name );
                    Holding.Add( obj );
                }
                if ( onclick && !isNear )
                {
                    AddPointSubject.OnNext( worldPos );
                    MoveState( BezierInputState.MoveLastHandle );
                }
                break;
            case BezierInputState.MoveLastHandle:
            case BezierInputState.ObjMove:
                Moving( clicking , worldPos );
                break;
            default:
                break;
        }
        IsPressedOld = clicking;
    }

    private void Moving( bool clicking , V3 worldPos )
    {
        if ( OldMoved.IsMoved( worldPos ) )
        {
            MoveHandleValue value = new MoveHandleValue( )
            {
                Obj = Holding ,
                WorldPos = worldPos
            };
            if ( InputState == BezierInputState.ObjMove )
            {
                MoveHandleSubject.OnNext( value );
            }
            else
            {
                MoveLastHandleSubject.OnNext( value );
            }
            OldMoved = value;
        }
        bool isDragEnd = IsPressedOld && !clicking;

        // はなされたらNormalに戻る
        if ( isDragEnd )
        {
            Holding.Clear( );
            MoveState( BezierInputState.Normal );
        }

        // ドラッグされていて、持っているオブジェクトがあるなら
    }
}
