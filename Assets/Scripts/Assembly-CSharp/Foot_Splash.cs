using System.Collections;
using UnityEngine;

public class Foot_Splash : MonoBehaviour
{
	public ParticleSystem LeftFootSplashParticles;

	public ParticleSystem LeftFootRippleParticles;

	public ParticleSystem LeftFootDropletParticles;

	public ParticleSystem RightFootSplashParticles;

	public ParticleSystem RightFootRippleParticles;

	public ParticleSystem RightFootDropletParticles;

	public AudioSource SplashAudio_A;

	public AudioSource SplashAudio_B;

	private int leftIsSplashing;

	private int rightIsSplashing;

	private void LeftFootSplash()
	{
		if (leftIsSplashing == 0)
		{
			LeftFootSplashParticles.Play();
			LeftFootRippleParticles.Play();
			LeftFootDropletParticles.Play();
			SplashAudio_A.Play();
			StartCoroutine("LeftSplashPause");
		}
	}

	private void RightFootSplash()
	{
		if (rightIsSplashing == 0)
		{
			RightFootSplashParticles.Play();
			RightFootRippleParticles.Play();
			RightFootDropletParticles.Play();
			SplashAudio_B.Play();
			StartCoroutine("RightSplashPause");
		}
	}

	private IEnumerator LeftSplashPause()
	{
		leftIsSplashing = 1;
		yield return new WaitForSeconds(0.4f);
		leftIsSplashing = 0;
	}

	private IEnumerator RightSplashPause()
	{
		rightIsSplashing = 1;
		yield return new WaitForSeconds(0.4f);
		rightIsSplashing = 0;
	}
}
