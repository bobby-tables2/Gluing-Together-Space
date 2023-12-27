using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class GluingTogetherSpace : MonoBehaviour
{
    [Header("Main camera for the portal texture effect")]
    [SerializeField]
    Camera mainCamera;

    [Header("Make the planes work only on one side?")]
    [SerializeField]
    bool enableFrontSidePlane1 = true;
    [SerializeField]
    bool enableBackSidePlane1 = true;
    [SerializeField]
    bool enableFrontSidePlane2 = true;
    [SerializeField]
    bool enableBackSidePlane2 = true;

    [Header("When an object enters a plane, it exits via the other")]
    [SerializeField]
    private GameObject plane1;
    [SerializeField]
    private GameObject plane2;
    private string guid = null;

    private string plane1name = "Gluing Together Plane 1";
    private string plane2name = "Gluing Together Plane 2";
    private string bodydoublename = "Body Double";

    private Dictionary<Tuple<GameObject, GameObject>, GameObject> T_object_plane_bodydouble = 
        new Dictionary<Tuple<GameObject, GameObject>, GameObject>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Transform plane1tfm = plane1.transform;
        Transform plane2tfm = plane2.transform;
        Transform camtfm = mainCamera.gameObject.transform;
        
        // generates portal texture for plane 1
        Vector3 new_cam_position = plane1tfm.TransformPoint(plane2tfm.InverseTransformPoint(camtfm.position));
        Quaternion new_cam_rotation = camtfm.rotation * Quaternion.Inverse(plane1tfm.rotation) * plane2tfm.rotation;

        GameObject plane1camobj = plane1.transform.Find("Plane Camera").gameObject;
        plane1camobj.transform.position = new_cam_position;
        plane1camobj.transform.rotation = new_cam_rotation;

        RenderTexture plane1tex = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
        Camera plane1cam = plane1camobj.GetComponent<Camera>();
        RenderTexture.active = plane1tex;
        plane1cam.targetTexture = plane1tex;
        plane1cam.Render();
        RenderTexture.active = null;

        plane2.GetComponent<MeshFilter>().GetComponent<MeshRenderer>().material.mainTexture = plane1tex;
        
        // generates portal texture for plane 2
        new_cam_position = plane2tfm.TransformPoint(plane1tfm.InverseTransformPoint(camtfm.position));
        new_cam_rotation = camtfm.rotation * Quaternion.Inverse(plane2tfm.rotation) * plane1tfm.rotation;

        GameObject plane2camobj = plane2.transform.Find("Plane Camera").gameObject;
        plane2camobj.transform.position = new_cam_position;
        plane2camobj.transform.rotation = new_cam_rotation;

        RenderTexture plane2tex = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
        Camera plane2cam = plane2camobj.GetComponent<Camera>();
        RenderTexture.active = plane2tex;
        plane2cam.targetTexture = plane2tex;
        plane2cam.Render();
        RenderTexture.active = null;

        plane1.GetComponent<MeshFilter>().GetComponent<MeshRenderer>().material.mainTexture = plane2tex;
        
        plane1tex.Release();
        plane2tex.Release();

        Resources.UnloadUnusedAssets();

        //find all possible objects that could pass through one of the planes(portals)
        Rigidbody[] rigidbodies = GameObject.FindObjectsOfType<Rigidbody>();

        foreach(Rigidbody e in rigidbodies)
        {
            // get the gameobject associated with the rigidbody
            GameObject object_in_scene = e.gameObject;
            if(this == object_in_scene || object_in_scene.name.Contains(bodydoublename))
            {
                continue;
            }

            //Transform plane1tfm = plane1.transform;
            //Transform plane2tfm = plane2.transform;
            Transform objtfm = object_in_scene.transform;

            // is the GameObject inside either of the planes?
            bool inplane1 = false;
            bool inplane2 = false;

            Collider[] hitColliders = Physics.OverlapSphere(objtfm.position, objtfm.localScale.magnitude);
            foreach (var hitCollider in hitColliders)
            {
                if(hitCollider.gameObject == plane1)
                {
                    //Debug.Log(plane1tfm.InverseTransformPoint(objtfm.position).z);
                    inplane1 = true;
                    //Debug.Log("Hit plane 1");
                }
                if(hitCollider.gameObject == plane2)
                {
                    //Debug.Log(plane2tfm.InverseTransformPoint(objtfm.position).z);
                    inplane2 = true;
                    //Debug.Log("Hit plane 2");
                }
            }

            // if the real object is at plane 1
            if(inplane1){
                // move the body double (at plane 2) with the original object (at plane 1) relative to the planes
                if(T_object_plane_bodydouble.ContainsKey(new Tuple<GameObject, GameObject>(object_in_scene, plane1)))
                {
                    // obtain body double
                    T_object_plane_bodydouble.TryGetValue(new Tuple<GameObject, GameObject>(object_in_scene, plane1), 
                        out GameObject bodydouble);

                    // move body double
                    Vector3 new_position = plane2tfm.TransformPoint(plane1tfm.InverseTransformPoint(objtfm.position));
                    Quaternion new_rotation = objtfm.rotation * Quaternion.Inverse(plane1tfm.rotation) * plane2tfm.rotation;
                    bodydouble.transform.position = new_position;
                    bodydouble.transform.rotation = new_rotation;
                }
                // else create a body double (at plane 2) of the original object (at plane 1)
                else {
                    // create body double
                    Vector3 new_position = plane2tfm.TransformPoint(plane1tfm.InverseTransformPoint(objtfm.position));
                    Quaternion new_rotation = objtfm.rotation * Quaternion.Inverse(plane1tfm.rotation) * plane2tfm.rotation;
                    GameObject bodydouble = Instantiate(object_in_scene, new_position, new_rotation);

                    // to prevent the body double from being registered by this script
                    bodydouble.name += bodydoublename;
                    //Destroy(bodydouble.GetComponent<Rigidbody>());  
                    // keep track of body double
                    T_object_plane_bodydouble.Add(new Tuple<GameObject, GameObject>(object_in_scene, plane1), bodydouble);
                }
            // remove the body double and move the original object to the other plane
            } else if(T_object_plane_bodydouble.ContainsKey(new Tuple<GameObject, GameObject>(object_in_scene, plane1))){
                Debug.Log("Left plane 1");

                // obtain body double
                T_object_plane_bodydouble.TryGetValue(new Tuple<GameObject, GameObject>(object_in_scene, plane1), 
                        out GameObject bodydouble);

                // swap original object and body double
                if((enableFrontSidePlane1 && plane1tfm.InverseTransformPoint(objtfm.position).z > objtfm.lossyScale.magnitude) ||
                       (enableBackSidePlane1 && plane1tfm.InverseTransformPoint(objtfm.position).z <= objtfm.lossyScale.magnitude) )
                {
                    Transform bodydoubletfm = bodydouble.transform;
                    object_in_scene.transform.position = plane2tfm.TransformPoint(plane1tfm.InverseTransformPoint(objtfm.position));
                    object_in_scene.transform.rotation = bodydoubletfm.rotation;
                }

                // deregister body double
                T_object_plane_bodydouble.Remove(new Tuple<GameObject, GameObject>(object_in_scene, plane1));
                // remove body double
                Destroy(bodydouble);
                

                

                //bodydouble.transform.position = objtfm.position;
                //bodydouble.transform.position = objtfm.position;
            }

            // if the real object is at plane 2
            if(inplane2){
                // move the body double (at plane 1) with the original object (at plane 2) relative to the planes
                if(T_object_plane_bodydouble.ContainsKey(new Tuple<GameObject, GameObject>(object_in_scene, plane2)))
                {
                    // obtain body double
                    T_object_plane_bodydouble.TryGetValue(new Tuple<GameObject, GameObject>(object_in_scene, plane2), 
                        out GameObject bodydouble);

                    // move body double
                    Vector3 new_position = plane1tfm.TransformPoint(plane2tfm.InverseTransformPoint(objtfm.position));
                    Quaternion new_rotation = objtfm.rotation * Quaternion.Inverse(plane2tfm.rotation) * plane1tfm.rotation;
                    bodydouble.transform.position = new_position;
                    bodydouble.transform.rotation = new_rotation;
                }
                // else create a body double (at plane 1) of the original object (at plane 2)
                else {
                    // create body double
                    Vector3 new_position = plane1tfm.TransformPoint(plane2tfm.InverseTransformPoint(objtfm.position));
                    Quaternion new_rotation = objtfm.rotation * Quaternion.Inverse(plane2tfm.rotation) * plane1tfm.rotation;
                    GameObject bodydouble = Instantiate(object_in_scene, new_position, new_rotation);
                    
                    // to prevent the body double from being registered by this script
                    bodydouble.name += bodydoublename;
                    //Destroy(bodydouble.GetComponent<Rigidbody>()); 
                    // keep track of body double
                    T_object_plane_bodydouble.Add(new Tuple<GameObject, GameObject>(object_in_scene, plane2), bodydouble);
                }
            } else if(T_object_plane_bodydouble.ContainsKey(new Tuple<GameObject, GameObject>(object_in_scene, plane2))){
                Debug.Log("Left plane 2");

                // obtain body double
                T_object_plane_bodydouble.TryGetValue(new Tuple<GameObject, GameObject>(object_in_scene, plane2), 
                        out GameObject bodydouble);

                // swap original object and body double
                if((enableFrontSidePlane2 && plane2tfm.InverseTransformPoint(objtfm.position).z > objtfm.lossyScale.magnitude) ||
                       (enableBackSidePlane2 && plane2tfm.InverseTransformPoint(objtfm.position).z <= objtfm.lossyScale.magnitude) )
                {
                    Transform bodydoubletfm = bodydouble.transform;
                    object_in_scene.transform.position = plane1tfm.TransformPoint(plane2tfm.InverseTransformPoint(objtfm.position));
                    object_in_scene.transform.rotation = bodydoubletfm.rotation;
                }

                // deregister body double
                T_object_plane_bodydouble.Remove(new Tuple<GameObject, GameObject>(object_in_scene, plane2));
                // remove body double
                DestroyImmediate(bodydouble);
            }
        }
        

        
    }

    //creates the two planes as children when script added
    void OnEnable()
    {
        // give the pair of planes a unique identifier
        if(guid == null){
            string guid = System.Guid.NewGuid().ToString();
            plane1name += " " + guid;
            plane2name += " " +  guid;
        }

        //generates first plane only if not found
        if (plane1 == null)
        {
            plane1 = GameObject.CreatePrimitive(PrimitiveType.Quad);

            plane1.GetComponent<MeshCollider>().convex = true;
            plane1.GetComponent<MeshCollider>().isTrigger = true;

            plane1.name = plane1name;
            plane1.transform.parent = this.transform;

            GameObject plane1cam = new GameObject("Plane Camera");
            plane1cam.AddComponent<Camera>();
            plane1cam.transform.parent = plane1.transform;
        }

        //generates second plane only if not found
        if (plane2 == null)
        {
            plane2 = GameObject.CreatePrimitive(PrimitiveType.Quad);

            plane2.GetComponent<MeshCollider>().convex = true;
            plane2.GetComponent<MeshCollider>().isTrigger = true;

            plane2.name = plane2name;
            plane2.transform.parent = this.transform;

            GameObject plane2cam = new GameObject("Plane Camera");
            plane2cam.AddComponent<Camera>();
            plane2cam.transform.parent = plane2.transform;
        }
    }

    // remove the two planes when script is removed
    void OnDestroy()
    {
        DestroyImmediate(plane1);
        DestroyImmediate(plane2);
    }

    // draws the squares representing the two planes
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;

        Transform plane1tfm = plane1.transform;
        Transform plane2tfm = plane2.transform;

        //generates a primitive quad mesh
        Mesh quadMesh = new Mesh();
        Vector3[] vertices = new Vector3[4]
        {
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3(0.5f, -0.5f, 0),
                new Vector3(-0.5f, 0.5f, 0),
                new Vector3(0.5f, 0.5f, 0)
        };
        quadMesh.vertices = vertices;

        Vector3[] normals = new Vector3[4]
        {
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward
        };
        quadMesh.normals = normals;

        int[] tris = new int[6]
        {
                // lower left triangle
                0, 2, 1,
                // upper right triangle
                2, 3, 1
        };
        quadMesh.triangles = tris;

        Vector2[] uv = new Vector2[4]
        {
                  new Vector2(0, 0),
                  new Vector2(1, 0),
                  new Vector2(0, 1),
                  new Vector2(1, 1)
        };
        quadMesh.uv = uv;

        //draw the planes in the editor
        Gizmos.DrawLine(plane1tfm.position, plane2tfm.position);
        Gizmos.DrawWireMesh(quadMesh, plane1tfm.position, plane1tfm.rotation, plane1tfm.localScale);
        Gizmos.DrawWireMesh(quadMesh, plane2tfm.position, plane2tfm.rotation, plane2tfm.localScale);
    }
}

