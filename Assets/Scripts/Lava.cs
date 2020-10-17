using UnityEngine;
using UnityEngine.Audio;

public class Lava : MonoBehaviour {

    [SerializeField]
    private GameObject hitParticle = null;
    private Material mat;
    private float currOffset = 0;
    float speed = .2f;

    [SerializeField]
    private AudioClip lavaHit = null;
    [SerializeField]
    private AudioMixerGroup mixer = null;
    private AudioSource as1;    //Hit sound

    void Start(){
        mat = GetComponent<MeshRenderer>().material;

        as1 = gameObject.AddComponent<AudioSource>();
        as1.outputAudioMixerGroup = mixer;
        as1.clip = lavaHit;
    }

    void Update(){
        currOffset += Time.deltaTime * speed;
        //mat.SetTextureOffset("_MainTex", new Vector2(currOffset, currOffset));
    }

    private void OnCollisionEnter(Collision collision)
    {
        var obj = collision.gameObject.GetComponent<IDamagable>();
        if (obj != null)
        {
            Instantiate(hitParticle, collision.transform.position, Quaternion.identity);
            obj.Death();
            as1.Play();

        }
    }
}
