using UnityEngine;
using System.Collections;

public class GraphicHelper : MonoBehaviour {

	public static GraphicHelper Instance;


	//Slowmo variables
	[Range(0.0f,1.0f)]
	public float slowmoFactor = 0.7f;
	[Range(0.0f,1.0f)]
	public float slowmoTime =0.2f;

    public Camera MainCamera;
    public float CameraShakeDuration = 0.5f;
    public float CameraShakeMagnitude = 0.1f;

	void Awake()
	{
		// Register the singleton
		if (Instance != null)
		{
			Debug.LogError("Multiple instances of SpecialEffectsHelper!");
		}

		Instance = this;
	}

	/// <summary>
	/// Create a particle effect at the given location
	/// </summary>
	/// <param name="position"></param>
	public void CreateParticleEffect(Vector3 position, ParticleSystem specialEffect)
	{
		//Set particles
		instantiate(specialEffect, position);
	}

	/// <summary>
	/// Instantiate a Particle system from prefab
	/// </summary>
	/// <param name="prefab"></param>
	/// <returns></returns>
	private ParticleSystem instantiate(ParticleSystem prefab, Vector3 position)
	{
		ParticleSystem newParticleSystem = Instantiate(
			prefab,
			position,
			Quaternion.identity
		) as ParticleSystem;

		// Make sure it will be destroyed
		Destroy(
			newParticleSystem.gameObject,
			newParticleSystem.startLifetime
		);

		return newParticleSystem;
	}


	//Slows down time by a set amount for a set period of time
	//Used for collision impact
	public void Slowmo()
	{
		//Debug.Log ("SLOWMO ENGAGED");
		StartCoroutine (CoSlowmo ());
	}



	//Slow down time for some time
	IEnumerator CoSlowmo()
	{
		//Slow down time by a predefine factor
		Time.timeScale = slowmoFactor;
		//Debug.Log ("SLOWMO ENGAGED");

		//Wait a set amount of time before going back to normal
		yield return new WaitForSeconds(slowmoTime); 

		//Debug.Log ("SLOWMO Done");
		//Go back to normal
		Time.timeScale = 1.0F;
		Time.fixedDeltaTime = 0.02F * Time.timeScale;
	}


	/// <summary>
	///  calls Coroutine to create a flash effect on all sprite renderers passed in to the function
	/// </summary>
	/// <param name="sprites">Sprites.</param>
	/// <param name="numTimes">Number times to flash</param>
	/// <param name="delay">Delay between flashes</param>
	/// <param name="disable">if you want to disable the renderer instead of change alpha</param>
	/// <param name="colors">A float array that describes color canges as (R,G,B) values </param>
	public IEnumerator FlashingSprites(SpriteRenderer[] sprites, int numTimes, float delay, bool disable = false)
	{
		return (CoFlashingSprites(sprites, numTimes, delay, disable));
	}

	/// <summary>
	///  Coroutine to create a flash effect on all sprite renderers passed in to the function
	/// </summary>
	/// <param name="sprites">Sprites.</param>
	/// <param name="numTimes">Number times to flash</param>
	/// <param name="delay">Delay between flashes</param>
	/// <param name="disable">if you want to disable the renderer instead of change alpha</param>
	IEnumerator CoFlashingSprites(SpriteRenderer[] sprites, int numTimes, float delay, bool disable) {
		// number of times to loop
		for (int loop = 0; loop < numTimes; loop++) {            
			// cycle through all sprites
			for (int i = 0; i < sprites.Length; i++) {                
				if (disable) {
					// for disabling
					sprites[i].enabled = false;
				} else {
					// for changing the alpha
					sprites[i].color = new Color(sprites[i].color.r, sprites[i].color.g, sprites[i].color.b, 0.5f);
				}
			}

			// delay specified amount
			yield return new WaitForSeconds(delay);

			// cycle through all sprites
            //for (int i = 0; i < sprites.Length; i++) {
            //    if (disable) {
            //        // for disabling
            //        sprites[i].enabled = true;
            //    } else {
            //        // for changing the alpha
            //        sprites[i].color = new Color(sprites[i].color.r, sprites[i].color.g, sprites[i].color.b, 1);
            //    }
            //}

            NormalizeSprites(sprites, disable);

			// delay specified amount
			yield return new WaitForSeconds(delay);
		}
	}

    public void NormalizeSprites(SpriteRenderer[] sprites, bool disable)
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            if (disable)
            {
                // for disabling
                sprites[i].enabled = true;
            }
            else
            {
                // for changing the alpha
                sprites[i].color = new Color(sprites[i].color.r, sprites[i].color.g, sprites[i].color.b, 1);
            }
        }
    }

    public void PlayShake()
    {
        //StopAllCoroutines();
        StartCoroutine("Shake");
    }

    IEnumerator Shake()
    {

        float elapsed = 0.0f;

        Vector3 originalCamPos = Camera.main.transform.position;

        while (elapsed < CameraShakeDuration)
        {

            elapsed += Time.deltaTime;

            float percentComplete = elapsed / CameraShakeDuration;
            float damper = 1.0f - Mathf.Clamp(4.0f * percentComplete - 3.0f, 0.0f, 1.0f);

            // map noise to [-1, 1]
            float x = Random.value * 2.0f - 1.0f;
            //float x = 1.0f * Mathf.PerlinNoise(Time.time * 1.0f, 0.0F);
            float y = Random.value * 2.0f - 1.0f;
            //float y = 1.0f * Mathf.PerlinNoise(Time.time * 1.0f, 0.0F);
            x *= CameraShakeMagnitude * damper;
            y *= CameraShakeMagnitude * damper;

            Camera.main.transform.position = new Vector3(x + originalCamPos.x, y + originalCamPos.y, originalCamPos.z);

            yield return null;
        }

        Camera.main.transform.position = originalCamPos;
    }
}
	