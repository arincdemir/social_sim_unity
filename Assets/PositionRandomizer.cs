using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionRandomizer : MonoBehaviour
{

    public float stdX = 1;
    public float stdZ = 1;
    // Start is called before the first frame update
    void Awake()
    {
	    if (!enabled)
	    {
		// Exit early if the script is disabled
		return;
	    }
    
        float newX = SampleGaussian(transform.position.x, stdX);
        float newY = 0;
        float newZ =  SampleGaussian(transform.position.z, stdZ);
        transform.position = new Vector3(newX, newY, newZ);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    static float SampleGaussian(float mean, float standardDeviation)
    {
    // Generate two uniform random numbers in (0, 1)
    float u1 = Random.value;
    float u2 = Random.value;

    // Apply the Box-Muller transform
    float z0 = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Cos(2.0f * Mathf.PI * u2);

    // Scale and shift to match the desired mean and standard deviation
    return mean + z0 * standardDeviation;
    }

}
