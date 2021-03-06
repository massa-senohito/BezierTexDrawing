﻿using Optional;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityBase : MonoBehaviour
{
    //public abstract void OnJustClick( HandBoneController controller);
    // マウスクリック反応を実装するなら
    public virtual void OnJustClick( ) { }

    public static string __Function( )
    {
#if DEBUG
        var stackTrace = new System.Diagnostics.StackTrace();
        return stackTrace.GetFrame( 1 ).GetMethod( ).Name;
#else
        return "";
#endif
    }

    public void SetTransparent()
    {

        var mayMat = GetMaterial( );
        var trans = Shader.Find("UI/Lit/Transparent");
        mayMat.Match( m => m.shader = trans , () => Debug.Log("shader not found"));
    }

    public Option<Material> GetMaterial()
    {
        var renderer = GetComponent<MeshRenderer>( );
        if ( renderer )
        {

            return renderer.material.Some();
        }
        var skinRend = GetComponent<SkinnedMeshRenderer>();
        if(skinRend)
        {
            return skinRend.material.Some();
        }
        return Option.None<Material>( );
    }

    public void SetColor(Color color)
    {
        var renderer = GetComponent<MeshRenderer>( );
        if ( renderer )
        {
            renderer.material.color = color;
            return;
        }
        var skinRend = GetComponent<SkinnedMeshRenderer>();
        if(skinRend)
        {
            skinRend.material.color = color;
        }
    }

    public static EntityBase SetEntity(GameObject gobj)
    {
        var ent = gobj.AddComponent<EntityBase>( );
        return ent;
    }

    public void Show()
    {
        SetActive( true );
    }

    public void Hide()
    {
        SetActive( false );
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive( isActive );
    }

    public void SetParent(EntityBase parent, bool stay)
    {
        transform.SetParent( parent.transform , stay );
    }

    public Vector3 GetPos()
    {
        return transform.position;
    }

    public Quaternion GetRot()
    {
        return transform.rotation;
    }

    public Vector3 GetScale()
    {
        return transform.localScale;
    }

    public void SetPos(Vector3 pos)
    {
        transform.position = pos;
    }

    public void SetOffset(float x , float y = 0 , float z = 0)
    {
        var pos = GetPos();
        SetPos( new Vector3(pos.x + x , pos.y + y , pos.z + z ) );
    }

    public void SetRot(Quaternion rot)
    {
        transform.rotation = rot;
    }

    public void SetScale(Vector3 scale)
    {
        transform.localScale = scale;
    }

    public void SetUnitScale(float x )
    {
        transform.localScale = new Vector3( x , x , x );
    }

    public static bool IsRange<T>(List<T> self , int index)
    {
        return 0 <= index && index < self.Count;
    }

    public void Log(object log)
    {
        Debug.Log( name + " , " + log );
    }

    public void Warn(object log)
    {
        Debug.LogWarning( name + " , " + log );
    }

}
