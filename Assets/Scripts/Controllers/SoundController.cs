//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;

public class SoundController : MonoBehaviour {

    float soundCooldown = 0;

	void Start () {
        // Register a callback functions
        WorldController.Instance.World.RegisterInstalledObjectCreated(OnInstalledObjectCreated);
        WorldController.Instance.World.RegisterTileChanged(OnTileChanged);
	}

    void Update()
    {
        soundCooldown -= Time.deltaTime;
    }

    void OnTileChanged(Tile tile_Data)
    {
        // Don't play sound when cooldown is active.
        if (soundCooldown > 0)
            return;

        AudioClip audioClip = Resources.Load<AudioClip>("Audio/Heavy_Shot");
        AudioSource.PlayClipAtPoint(audioClip, Camera.main.transform.position);
        soundCooldown = .1f;
    }

    public void OnInstalledObjectCreated(InstalledObject installedObject)
    {
        // Don't play sound when cooldown is active.
        if (soundCooldown > 0)
            return;

        AudioClip audioClip = Resources.Load<AudioClip>("Audio/" + installedObject.ObjectType + "_OnCreated");
        // audioClip = Resources.Load<AudioClip>("Audio/Light_Shot");

        // Play some default sound when audio clip can not get loaded.
        if (audioClip == null)
            audioClip = Resources.Load<AudioClip>("Audio/Light_Shot");

        AudioSource.PlayClipAtPoint(audioClip, Camera.main.transform.position);
        soundCooldown = .1f;
    }
}
