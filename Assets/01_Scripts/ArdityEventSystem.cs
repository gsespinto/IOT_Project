using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArdityEventSystem : MonoBehaviour
{
    private GameObject _currentSelectedObj;
    
    public GameObject CurrentSelectedObj
    {
        get { return _currentSelectedObj; }
        set { _currentSelectedObj = value; }
    }
}
