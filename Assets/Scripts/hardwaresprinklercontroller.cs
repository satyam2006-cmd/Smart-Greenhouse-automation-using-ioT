
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class HardwareSprinklerController : MonoBehaviour
{
    public string url = "http://10.217.97.46/data";

    public float updateInterval = 1f;

    [System.Serializable]
    public class Sprinkler
    {
        public string sprinklerName;
        public Transform lanceur;
        public Transform pivot;
        public float rotationSpeed = 35f;
        public ParticleSystem[] waterJets;

        [HideInInspector]
        public bool isOn;
    }

    public Sprinkler[] sprinklers = new Sprinkler[4];

    [System.Serializable]
    public class SensorData
    {
        public bool sprinkler_on;
    }

    void Start()
    {
        foreach (Sprinkler s in sprinklers)
        {
            SetSprinkler(s, false);
        }

        StartCoroutine(ReadHardware());
    }

    IEnumerator ReadHardware()
    {
        while (true)
        {
            bool connected = false;

            using (UnityWebRequest req = UnityWebRequest.Get(url))
            {
                req.timeout = 5;

                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    SensorData data =
                        JsonUtility.FromJson<SensorData>(
                            req.downloadHandler.text
                        );

                    if (data != null)
                    {
                        foreach (Sprinkler s in sprinklers)
                        {
                            SetSprinkler(s, data.sprinkler_on);
                        }

                        connected = true;
                    }
                }
                else
                {
                    Debug.LogWarning("ESP8266: " + req.error);
                }
            }

            // Safety: stop spraying if connection is lost
            if (!connected)
            {
                foreach (Sprinkler s in sprinklers)
                {
                    SetSprinkler(s, false);
                }
            }

            yield return new WaitForSeconds(updateInterval);
        }
    }

    void Update()
    {
        foreach (Sprinkler s in sprinklers)
        {
            if (s == null) continue;

            if (s.isOn && s.lanceur != null && s.pivot != null)
            {
                s.lanceur.RotateAround(
                    s.pivot.position,
                    s.pivot.forward,
                    s.rotationSpeed * Time.deltaTime
                );
            }
        }
    }

    void SetSprinkler(Sprinkler s, bool state)
    {
        if (s == null || s.isOn == state) return;

        s.isOn = state;

        foreach (ParticleSystem jet in s.waterJets)
        {
            if (jet == null) continue;

            if (state)
                jet.Play();
            else
                jet.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmittingAndClear
                );
        }

        Debug.Log(
            s.sprinklerName +
            (state ? " ON - WATER SPRAYING" : " OFF")
        );
    }
}
