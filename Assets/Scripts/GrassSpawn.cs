using UnityEngine;

public class GrassSpawn : MonoBehaviour
{
    public void Start()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<MeshFilter>().mesh = null;
        gameObject.GetComponent<BoxCollider>().enabled = false;
        transform.eulerAngles = new Vector3(0, Random.Range(0f, 360f), 90f);
        transform.localScale = Vector3.one * Random.Range(0.14f, 0.15f);
    }
}
