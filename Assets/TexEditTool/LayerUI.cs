using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LayerUI : MonoBehaviour
{
    //Button[] ButtonList;
    Button Self;
    // Use this for initialization
    void Start( )
    {
        var button = GetComponent<Button>( );
        button.onClick.AddListener( OnClicked );
        Self = button;
    }
//TreeViewがそのまま使えるのでいったん起き
    void OnClicked()
    {
        //Self.
    }

    // Update is called once per frame
    void Update( )
    {

    }
}
