using Optional;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unit = EntityBase;
using Gobj = UnityEngine.GameObject;
using V3 = UnityEngine.Vector3;
using UniRx;
using AddPointValue = 
    UnityEngine.Vector3;

public class MoveHandleValue
{
    public List<Unit> Obj;
    public V3 WorldPos;
    public bool IsMoved( V3 v )
    {
        return v != WorldPos;
    }
}

public class BezierState
{
    List<Unit> Holding = new List<Unit>();
    public enum BezierInputState
    {
        Normal,
        MoveLastHandle,
        ObjSelect,
        ObjMove,
    }

    public BezierInputState InputState
    {
        get;
        private set;
    }

    void MoveState(BezierInputState state)
    {
        if(InputState == state)
        {
            return;
        }
        //Debug.Log( "MoveState : " + state );
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

        bool isHolding = Holding.Count > 0;
        float nearDist = 0.08f;
        var obj = mayObj.ValueOr(()=>null);

        switch ( InputState )
        {
            case BezierInputState.Normal:
                bool isNear = dist < nearDist;
                if ( isNear )
                {
                    isNear = mayObj.HasValue;
                }
                // 近くにオブジェクトがあるなら移動モードへ遷移
                if ( mayObj.HasValue && isNear && onclick)
                {
                    MoveState( BezierInputState.ObjSelect);
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
            case BezierInputState.ObjSelect:
                OnObjSelect( mayObj , onclick , obj );
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

    private void OnObjSelect( Option<Unit> mayObj , bool onclick , Unit obj )
    {
        // ドラッグ時、すぐにポイントが移動すると操作しにくい
        // １度ボタンから指が離されていてActiveなオブジェクトが再度ドラッグされたら
        if ( onclick )
        {
            if ( mayObj.HasValue )
            {
                if ( Holding.Contains( obj ) )
                {
                    MoveState( BezierInputState.ObjMove );
                }
                else
                {
                    ActiveObjectChanged.OnNext( obj );
                }
            }
        }
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
