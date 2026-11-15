using System.Collections.Generic;
using UnityEngine;

public class WindZoneData : MonoBehaviour
{
    
    private List<WindZone>windZoneLists=new();
    private Vector3 force;
    private static WindZoneData manager;
    public static WindZoneData getInstance=>manager;
    public Vector3 getWindForce=>force;
    void Awake()
    {
        manager=this;
    }
    void Start()
    {
        WindZone currentWindZone;
        var windZonesSc= FindObjectsByType<WindZone>();
        if(windZonesSc==null)
            return;
        foreach(var windZones in windZonesSc){
            currentWindZone=windZones.GetComponent<WindZone>();
            if(currentWindZone.mode==WindZoneMode.Spherical)
                continue;
            windZoneLists.Add(currentWindZone);
        }
    }

    // Update is called once per frame
    void Update()
    {
        force=Vector3.zero;
        foreach(var windZone in windZoneLists)
        {
            force+=(windZone.windMain+Mathf.PerlinNoise1D(Time.time*windZone.windPulseFrequency)*windZone.windTurbulence)*windZone.gameObject.transform.forward;
        }
    }
}
