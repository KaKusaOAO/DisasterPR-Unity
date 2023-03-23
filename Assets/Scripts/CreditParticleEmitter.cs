using UnityEngine;
using Random = System.Random;

public class CreditParticleEmitter : MonoBehaviour
{
    public Vector3 desiredPos;
    private float _startTime;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = desiredPos;
        var sparklePrefab = Resources.Load<GameObject>("Prefabs/UIParticle/Sparkle");
        var dotPrefab = Resources.Load<GameObject>("Prefabs/UIParticle/LittleDot");

        var random = new Random();
        for (var i = 0; i < 20; i++)
        {
            var particle = Instantiate(random.NextDouble() >= 0.5 ? sparklePrefab : dotPrefab, transform);
            particle.transform.localPosition = Vector3.zero;
            particle.AddComponent<CreditParticle>();
        }

        _startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - _startTime > 2)
        {
            Destroy(gameObject);
        }
    }
}