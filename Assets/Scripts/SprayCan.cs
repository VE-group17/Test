using UnityEngine;
using Ubiq.XR;
using Ubiq.Messaging;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

// Adds simple networking to the 3d pen. The approach used is to draw locally
// when a remote user tells us they are drawing, and stop drawing locally when
// a remote user tells us they are not.
public class SprayCan : MonoBehaviour, IGraspable, IUseable
{
    private NetworkContext context;
    private bool owner;
    private Hand controller;
    private Transform nozzel;
    private Material drawingMaterial;
    private GameObject currentDrawing;

    public Camera canvasCam, sceneCamera;
    public float brushSize=1.0f; //The size of our brush
    public Sprite cursorPaint;
    public GameObject brushContainer;
    Color brushColor;
    // public RenderTexture canvasTexture; // Render Texture that looks at our Base Texture and the painted brushes
    // Amend message to also store current drawing state
    private struct Message
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool isDrawing; // new
        
        public Message(Transform transform, bool isDrawing)
        {
            this.position = transform.position;
            this.rotation = transform.rotation;
            this.isDrawing = isDrawing; // new
        }
    }

    private void Start()
    {
        nozzel = transform.Find("SprayCan/Nozzel");
        context = NetworkScene.Register(this);
        var shader = Shader.Find("Particles/Standard Unlit");
        drawingMaterial = new Material(shader);
    }

    public void ProcessMessage (ReferenceCountedSceneGraphMessage msg)
    {
        var data = msg.FromJson<Message>();
        transform.position = data.position;
        transform.rotation = data.rotation;


        brushColor = Color.blue;
		if (Input.GetMouseButton(0)) {
			BeginDrawing();
		}
        // new
        // Also start drawing locally when a remote user starts
        if (data.isDrawing && !currentDrawing)
        {
            BeginDrawing();
        }
        if (!data.isDrawing && currentDrawing)
        {
            EndDrawing();
        }
    }

    private void FixedUpdate()
    {
        if (owner)
        {
            // new
            context.SendJson(new Message(transform,isDrawing:currentDrawing));
        }
    }

    private void LateUpdate()
    {
        if (controller)
        {
            transform.position = controller.transform.position;
            transform.rotation = controller.transform.rotation;
        }
    }

    void IGraspable.Grasp(Hand controller)
    {
        owner = true;
        this.controller = controller;
    }

    void IGraspable.Release(Hand controller)
    {
        owner = false;
        this.controller = null;
    }

    void IUseable.Use(Hand controller)
    {
        BeginDrawing();
    }

    void IUseable.UnUse(Hand controller)
    {
        EndDrawing();
    }

    private void BeginDrawing()
    {
        Vector3 uvWorldPosition=Vector3.zero;	
        Debug.Log("begin drawing outside");	
		if(HitTestUVPosition(ref uvWorldPosition)){
			// GameObject brushObj;
            Debug.Log("begin drawing");
            currentDrawing=(GameObject)Instantiate(Resources.Load("TexturePainter-Instances/BrushEntity")); //Paint a brush
            currentDrawing.GetComponent<SpriteRenderer>().color=brushColor; //Set the brush color
			brushColor.a=brushSize*2.0f; // Brushes have alpha to have a merging effect when painted over.
			currentDrawing.transform.parent=brushContainer.transform; //Add the brush to our container to be wiped later
			currentDrawing.transform.localPosition=uvWorldPosition; //The position of the brush (in the UVMap)
            currentDrawing.transform.localScale=Vector3.one*brushSize;//The size of the brush
		}
    }
    
    //Returns the position on the texuremap according to a hit in the mesh collider
    bool HitTestUVPosition(ref Vector3 uvWorldPosition){
		RaycastHit hit;
        GameObject nozzel = GameObject.Find("Cylinder");
		Vector3 cursorDir = nozzel.transform.forward;
		// Ray cursorRay=sceneCamera.ScreenPointToRay (cursorPos);
		Ray cursorRay= new Ray(nozzel.GetComponent<Transform>().position, cursorDir);
		if (Physics.Raycast(cursorRay,out hit,2000)){
            Debug.Log("Inside hitTest");
			MeshCollider meshCollider = hit.collider as MeshCollider;
			if (meshCollider == null || meshCollider.sharedMesh == null)
				return false;			
			Vector2 pixelUV  = new Vector2(hit.textureCoord.x,hit.textureCoord.y);
			uvWorldPosition.x=pixelUV.x-canvasCam.orthographicSize;//To center the UV on X
			uvWorldPosition.y=pixelUV.y-canvasCam.orthographicSize;//To center the UV on Y
			uvWorldPosition.z=0.0f;
			return true;
		}
		else{		
			return false;
		}
	}


    private void EndDrawing()
    {
        Debug.Log(currentDrawing);
        currentDrawing.transform.parent = null;
        // currentDrawing.GetComponent<TrailRenderer>().emitting = false;
        currentDrawing = null;
    }
}