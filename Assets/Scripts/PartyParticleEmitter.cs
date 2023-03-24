using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class PartyParticleEmitter : MonoBehaviour
{
    public Vector3 desiredPos;
    private float _startTime;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = desiredPos;
        var prefab = Resources.Load<GameObject>("Prefabs/UIParticle/PartyParticle");
        var sprites = Resources.LoadAll<Sprite>("Sprites/end_s-sheet0");

        var random = new Random();
        for (var i = 0; i < 20; i++)
        {
            var particle = Instantiate(prefab, transform);
            var image = particle.GetComponent<Image>();
            image.sprite = sprites[random.Next(sprites.Length)];
            particle.transform.localPosition = Vector3.zero;
            particle.AddComponent<PartyParticle>();
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