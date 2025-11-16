using UnityEngine;

public class GrassSpawn : MonoBehaviour
{
    public void Start()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<MeshFilter>().mesh = null;
        gameObject.GetComponent<BoxCollider>().enabled = false;
    }
}
