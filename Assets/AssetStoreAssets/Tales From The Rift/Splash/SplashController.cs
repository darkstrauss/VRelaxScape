using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.VR;

public class SplashController : MonoBehaviour {

	public float minimumTimeToShowLogo = 5f;
	public string levelToLoad = "";
	
	public Animation animation;
    public AudioSource audio;
	public string introName = "SplashIn";
	public string outroName = "SplashOut";
	
	IEnumerator Start () {

        UnityEngine.XR.InputTracking.Recenter ();
		
		// intro
		
		animation.Play(introName);
        audio.Play();

        yield return new WaitForSeconds(animation.clip.length);

        // Create out Async operation
        AsyncOperation asyncLevelLoadOperation = null;


        // Assign what kind of operation it is going to be.
        asyncLevelLoadOperation = SceneManager.LoadSceneAsync(levelToLoad);

        // Stop the async from activating as soon as its ready and instead let it load while the animations are playing.
        asyncLevelLoadOperation.allowSceneActivation = false;

        // Let the whole intro animation play.
        while (animation.isPlaying)
        {
            yield return new WaitForEndOfFrame();
        }

        // This ensures that the application loads the entire next scene before activating the outro animation.
        while (true)
        {
            yield return new WaitForEndOfFrame();

            // Progress stops at 0.9f
            if (asyncLevelLoadOperation.progress >= 0.9f)
            {
                break;
            }
        }

        // Play the outro to make a smooth trasition to the next scene.
        animation.Play(outroName);

        while (true)
        {
            yield return new WaitForEndOfFrame();

            if (!animation.isPlaying)
            {
                break;
            }
        }

        // activate scene

        asyncLevelLoadOperation.allowSceneActivation = true;
	}
}
