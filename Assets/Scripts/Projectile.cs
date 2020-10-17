using UnityEngine;

public class Projectile : MonoBehaviour {

    private float damage = 0f;
    [SerializeField]
    private float speed = 20f;
    private IDamagable owner;

    [SerializeField]
    private LayerMask hitMask = 0;
    [SerializeField]
    private float maxDistance = 30f;
    private Vector3 startPos;

    public void SetStats(float _damage, IDamagable _owner){
        damage = _damage;
        owner = _owner;
    }

    void FixedUpdate() {
        CheckCollision();
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void Update() {
        if (Vector3.Distance(startPos, transform.position) > maxDistance) {
            Destroy(this.gameObject);
        }
    }

    void CheckCollision() {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, speed * Time.deltaTime, hitMask)) {
            IDamagable damagable = hit.collider.GetComponent<IDamagable>();
            if (damagable != null) {
                damagable.TakeDamage(damage, owner);

                //Можно удалить, если в игре будет прострел
                Destroy(this.gameObject);
            }
        }
    }
}
