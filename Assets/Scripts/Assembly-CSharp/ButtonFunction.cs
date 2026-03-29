using UnityEngine;

public class ButtonFunction : MonoBehaviour
{
	public Animator animator;

	private void Start()
	{
		animator = GetComponent<Animator>();
	}

	private void Update()
	{
	}

	public void Idle()
	{
		animator = GetComponent<Animator>();
		animator.SetBool("Walk", false);
		animator.SetBool("SprintJump", false);
		animator.SetBool("SprintSlide", false);
	}

	public void Walk()
	{
		animator = GetComponent<Animator>();
		animator.SetBool("Walk", true);
		animator.SetBool("SprintJump", false);
		animator.SetBool("SprintSlide", false);
	}

	public void SprintJump()
	{
		animator = GetComponent<Animator>();
		animator.SetBool("Walk", false);
		animator.SetBool("SprintJump", true);
		animator.SetBool("SprintSlide", false);
	}

	public void SprintSlide()
	{
		animator = GetComponent<Animator>();
		animator.SetBool("Walk", false);
		animator.SetBool("SprintJump", false);
		animator.SetBool("SprintSlide", true);
	}
}
