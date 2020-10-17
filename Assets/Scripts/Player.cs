using UnityEngine;

public class Player : MonoBehaviour, IDamagable {

    [SerializeField]
    private float hp, maxHP, speed, maxSpeed;

    [SerializeField]
    private int maxAmmo, currentAmmo, killCount;

    [SerializeField]
    private SizeController arena;

    private CameraShake shaker;

    private Rigidbody rb;

    [SerializeField]
    private Camera viewCamera;
    [SerializeField]
    private Spawner spawner;

    private bool dead;
    
    [Header("Audio")]
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip shotSound;

    [Header("Shooting")]
    [SerializeField]
    private Transform shotPos = null;
    [SerializeField]
    private Projectile projectile = null;
    [SerializeField]
    private float damage = 1f;
    [SerializeField]
    private float shootCD = .5f;
    private float nextShotTime;
    private bool canShoot = true;

    [SerializeField]
    private Mesh[] meshes;
    private MeshFilter meshFilter;
    [SerializeField]
    private GameObject hitParticle = null;

    public delegate void UpdateVariable(float value, float maxValue);
    public static event UpdateVariable OnHealthUpdate;
    public static event UpdateVariable OnAmmoUpdate;

    public delegate void FinishedLevel();
    public static event FinishedLevel OnLevelFinished;
    public static event FinishedLevel OnDeath;

    void Awake() {
        OnHealthUpdate = null;
        OnAmmoUpdate = null;
        OnLevelFinished = null;
        OnDeath = null;
    }

    private void Start()
    {

        Time.timeScale = 1f;
        meshFilter = GetComponent<MeshFilter>();
        arena = GameObject.FindGameObjectWithTag("Arena").GetComponent<SizeController>();
        shaker = Camera.main.GetComponent<CameraShake>();
        rb = GetComponent<Rigidbody>();

        Hp = maxHP;
        speed = maxSpeed;
        currentAmmo = maxAmmo;
        if(OnAmmoUpdate != null)
        {
            OnAmmoUpdate(currentAmmo, maxAmmo);
        }
        if(OnHealthUpdate != null)
        {
            OnHealthUpdate(hp, maxHP);
        }
        Spawner.endOfWaveEvent += DisableShooting;
    }

    private void Update()
    {
        if(!IsGrounded() && !dead){
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.None;
        }
        if (dead)
            return;

        InputHandle();
        if (Input.GetMouseButtonDown(0))
        {
            Fire();
        }

        MouseHandle();
    }

    bool IsGrounded() {
        if (Physics.Raycast(transform.position, transform.up * -1, 1f))
            return true;
        else
            return false;
    }

    private void InputHandle() {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical).normalized;

        Vector3 newPos = transform.position + (movement * speed * Time.deltaTime);

        transform.localPosition = newPos;

        //rb.MovePosition(newPos);
    }

    private void MouseHandle(){
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, 7, 0));
        float rayDistance;
        if (groundPlane.Raycast(ray, out rayDistance)) {
            Vector3 point = ray.GetPoint(rayDistance);
            Vector3 correctedPoint = new Vector3(point.x, transform.position.y, point.z);
            transform.LookAt(correctedPoint);
        }
    }

    public void Death()
    {
        Spawner.instance.StopAllCoroutines();
        dead = true;
        OnDeath();
        Debug.Log("You died");
    }

    public void TakeDamage(float damageToTake, IDamagable damager) {
        Hp -= damageToTake;
        Instantiate(hitParticle, transform.position, Quaternion.identity);
        if (Hp <= 0)
            Death();
    }

    public float Hp
    {
        set
        {
            if (value < 0)
            {
                hp = 0;
            }
            else if (value > maxHP)
            {
                hp = maxHP;
            }
            else
            {
                hp = value;
            }
            if(OnHealthUpdate != null)
                OnHealthUpdate(Hp, maxHP);
            if (Hp > 0 && Hp - 1 < meshes.Length)
                meshFilter.mesh = meshes[Mathf.RoundToInt(Hp) - 1];
        }
        get
        {
            return hp;
        }
    }

    public int CurrentAmmo
    {
        get
        {
            return currentAmmo;
        }

        set
        {
            if (value < 0)
            {
                currentAmmo = 0;
            }
            else if(value > maxAmmo)
            {
                currentAmmo = maxAmmo;
            } else
            {
                currentAmmo = value;
            }
            arena.ChangeSize(currentAmmo, maxAmmo);
            if(OnAmmoUpdate != null)
            {
                OnAmmoUpdate(currentAmmo, maxAmmo);
            }
        }
    }

    public bool CanShoot
    {
        get
        {
            return canShoot;
        }

        set
        {
            canShoot = value;
        }
    }

    public void Fire() {
        if (canShoot)
        {
            if (Time.time < nextShotTime)
                return;

            audioSource.PlayOneShot(shotSound);

            Projectile prj = Instantiate(projectile, shotPos.position, shotPos.rotation) as Projectile;
            prj.SetStats(damage, this);
            nextShotTime = Time.time + shootCD;

            shaker.shakeDuration = 0.1f;
            CurrentAmmo--;
        }
    }

    public void UpgradeHealth(int amount)
    {
        spawner.NewWave();
        Hp += amount;
        if(OnLevelFinished != null)
        {
            OnLevelFinished();
        }
        canShoot = true;
    }

    public void UpgradeSpeed(int amount)
    {
        spawner.NewWave();
        maxSpeed += amount;
        speed = maxSpeed;
        if (OnLevelFinished != null)
        {
            OnLevelFinished();
        }
        canShoot = true;
    }

    public void UpgradeAmmo(int amount)
    {
        spawner.NewWave();
        maxAmmo += amount;
        CurrentAmmo += amount;
        if (OnLevelFinished != null)
        {
            OnLevelFinished();
        }
        canShoot = true;
    }

    private void DisableShooting()
    {
        canShoot = false;
    }
}
