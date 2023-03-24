using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ubiq.XR;
using UnityEngine;

public class roller : MonoBehaviour, IGraspable, IUseable
{
    [SerializeField] private Transform _tip;
    [SerializeField] private int _penSize = 35;

    private Renderer _renderer;
    private Color[] _colors;
    private float _tipHeight;
    private RaycastHit _touch;
    private Whiteboard _whiteboard;
    private Vector2 _touchPos, _lastTouchPos;
    private bool _touchLastFrame;
    private Quaternion _lastTouchRot;
    // Start is called before the first frame update
   // private Collider my_collider;

    void Start()
    {
        _renderer = _tip.GetComponent<Renderer>();
        _colors = Enumerable.Repeat(_renderer.material.color, _penSize * _penSize).ToArray();
        _tipHeight = 0.1f;
       // my_collider = GetComponent<Collider>();

    }
    void IGraspable.Grasp(Ubiq.XR.Hand controller)
    {
        //GetComponent<Rigidbody>().useGravity = false;
    }

    void IGraspable.Release(Ubiq.XR.Hand controller)
    {
       // GetComponent<Rigidbody>().useGravity = true;
    }

    void IUseable.Use(Ubiq.XR.Hand controller)
    {


    }

    void IUseable.UnUse(Ubiq.XR.Hand controller)
    {

    }


    // Update is called once per frame
    void Update()
    {
        Draw();
    }

    private void Draw()
    {
        if (Physics.Raycast(_tip.parent.position, transform.forward, out _touch, _tipHeight))
        {
            if (_touch.transform.CompareTag("MixPaint"))
            {
               

                Color paint_color = _touch.transform.GetComponent<Renderer>().material.color;
                Transform brush = transform.GetChild(5).GetChild(0).GetComponent<Transform>();
                brush.GetComponent<Renderer>().material.color = paint_color;
                _colors = Enumerable.Repeat(paint_color, _penSize * _penSize).ToArray();
                // print(brush.name);
                // transform.GetComponent<Material>().color = paint_color;



            }
                if (_touch.transform.CompareTag("Whiteboard"))
                
            {
                GetComponent<Rigidbody>().isKinematic = true;
                if (_whiteboard == null)
                {
                    _whiteboard = _touch.transform.GetComponent<Whiteboard>();
                }
                _touchPos = new Vector2(_touch.textureCoord.x, _touch.textureCoord.y);


                var x = (int)(_touchPos.x * _whiteboard.textureSize.x - (_penSize / 2)) ;
                var y = (int)(_touchPos.y * _whiteboard.textureSize.y - (_penSize / 2));

                if (y < 0 || y > _whiteboard.textureSize.y || x < 0 || x > _whiteboard.textureSize.x) return;

                if (_touchLastFrame)
                {
                    _whiteboard.texture.SetPixels(x, y, _penSize, _penSize, _colors);
                    for (float f = 0.01f; f < 1.00f; f += 0.03f)
                    {
                        var lerpX = (int)Mathf.Lerp(_lastTouchPos.x, x, f);
                        var lerpY = (int)Mathf.Lerp(_lastTouchPos.y, y, f);
                        _whiteboard.texture.SetPixels(lerpX, lerpY, _penSize, _penSize, _colors);

                    }


                    _whiteboard.texture.Apply();



                }

                _lastTouchPos = new Vector2(x, y);
                _lastTouchRot = transform.rotation;
                _touchLastFrame = true;
                GetComponent<Rigidbody>().isKinematic = false;
                return;

                
            }


        }

        _whiteboard = null;
        _touchLastFrame = false;


    }
}
