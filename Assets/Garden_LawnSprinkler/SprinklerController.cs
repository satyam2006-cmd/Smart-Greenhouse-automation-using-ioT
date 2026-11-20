
using UnityEngine;

public class SprinklerController : MonoBehaviour
{
    [Header("Sprinkler Parts")]
    public Transform rotationPivot;
    public Transform rotatingHead;

    [Header("Water Particles")]
    public ParticleSystem[] waterJets;

    [Header("Settings")]
    public float rotationSpeed = 30f;
    public bool sprinklerOn = true;

    void Start()
    {
        SetSprinkler(sprinklerOn);
    }

    void Update()
    {
        if (sprinklerOn && rotationPivot != null
            && rotatingHead != null)
        {
            rotatingHead.RotateAround(
    rotationPivot.position,
    rotationPivot.forward,
    rotationSpeed * Time.deltaTime
);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetSprinkler(!sprinklerOn);
        }
    }

    public void SetSprinkler(bool state)
    {
        sprinklerOn = state;

        foreach (ParticleSystem jet in waterJets)
        {
            if (jet == null) continue;

            if (state)
                jet.Play();
            else
                jet.Stop(
                    false,
                    ParticleSystemStopBehavior.StopEmitting
                );
        }
    }
}
