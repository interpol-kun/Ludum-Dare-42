using UnityEngine;

public class SizeController : MonoBehaviour {

    private Vector3 defaultScale = Vector3.one;
    private float newScale = 1;
    public static SizeController instance;

    void Awake(){
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        defaultScale = transform.localScale;
    }

    public void ChangeSize(int ammo, int maxAmmo)
    {
        newScale = ((float)ammo / maxAmmo)*defaultScale.x;
        transform.localScale = new Vector3(newScale, newScale, DefaultScale.z);
        //
        Spawner.instance.BakeNavmesh();
    }

    public Vector3 DefaultScale
    {
        get
        {
            return defaultScale;
        }
    }
}
