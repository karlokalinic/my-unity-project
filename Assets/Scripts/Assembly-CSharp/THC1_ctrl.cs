using UnityEngine;

public class THC1_ctrl : MonoBehaviour
{
	private Animator anim;

	private CharacterController controller;

	private int battle_state;

	public float speed = 6f;

	public float runSpeed = 3f;

	public float turnSpeed = 60f;

	public float gravity = 20f;

	private Vector3 moveDirection = Vector3.zero;

	private float w_sp;

	private float r_sp;

	private void Start()
	{
		anim = GetComponent<Animator>();
		controller = GetComponent<CharacterController>();
		r_sp = runSpeed;
	}

	private void Update()
	{
		if (Input.GetKey("1"))
		{
			anim.SetInteger("battle", 0);
			battle_state = 0;
		}
		if (Input.GetKey("2"))
		{
			anim.SetInteger("battle", 1);
			battle_state = 1;
		}
		if (Input.GetKey("3"))
		{
			anim.SetInteger("battle", 2);
			battle_state = 2;
		}
		if (Input.GetKey("4"))
		{
			anim.SetInteger("battle", 3);
			battle_state = 3;
		}
		if (Input.GetKey("up"))
		{
			if (battle_state == 0)
			{
				anim.SetInteger("moving", 1);
				runSpeed = 1f;
			}
			if (battle_state == 1)
			{
				anim.SetInteger("moving", 2);
				runSpeed = r_sp;
			}
			if (battle_state == 2)
			{
				anim.SetInteger("moving", 3);
				runSpeed = 0.66f;
			}
			if (battle_state == 3)
			{
				runSpeed = 0f;
			}
		}
		else
		{
			anim.SetInteger("moving", 0);
		}
		if (Input.GetMouseButtonDown(0))
		{
			anim.SetInteger("moving", 4);
		}
		if (Input.GetMouseButtonDown(1))
		{
			anim.SetInteger("moving", 5);
		}
		if (Input.GetMouseButtonDown(2))
		{
			anim.SetInteger("moving", 6);
		}
		if (Input.GetKeyDown("i"))
		{
			anim.SetInteger("moving", 13);
		}
		if (Input.GetKeyDown("o"))
		{
			anim.SetInteger("moving", 12);
		}
		if (Input.GetKeyDown("u"))
		{
			if (Random.Range(0, 2) == 0)
			{
				anim.SetInteger("moving", 10);
			}
			else
			{
				anim.SetInteger("moving", 11);
			}
		}
		if (Input.GetKeyDown("p"))
		{
			anim.SetInteger("moving", 14);
		}
		if (Input.GetKeyUp("p"))
		{
			anim.SetInteger("moving", 15);
		}
		if (Input.GetKeyDown("z"))
		{
			anim.SetInteger("moving", 17);
		}
		if (Input.GetKeyUp("z"))
		{
			anim.SetInteger("moving", 0);
		}
		if (Input.GetKeyDown("x"))
		{
			anim.SetInteger("moving", 7);
		}
		if (Input.GetKeyDown("c"))
		{
			anim.SetInteger("moving", 8);
		}
		if (Input.GetKeyDown("space"))
		{
			anim.SetInteger("moving", 16);
		}
		if (Input.GetKeyDown("v"))
		{
			anim.SetInteger("moving", 18);
		}
		if (controller.isGrounded)
		{
			moveDirection = base.transform.forward * Input.GetAxis("Vertical") * speed * runSpeed;
			float axis = Input.GetAxis("Horizontal");
			base.transform.Rotate(0f, axis * turnSpeed * Time.deltaTime, 0f);
		}
		moveDirection.y -= gravity * Time.deltaTime;
		controller.Move(moveDirection * Time.deltaTime);
	}
}
