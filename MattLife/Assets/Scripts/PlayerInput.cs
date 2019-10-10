using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Player))]
public class PlayerInput : MonoBehaviour
{
	private Player player;

    // Start is called before the first frame update
    void Start()
    {
		player = GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
		if (!GameMaster.Instance || GameMaster.Instance.state != GameMaster.States.game)
		{
			return;
		}
		Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
		player.SetDirectionalInput(directionalInput);

		if (Input.GetButtonDown("Jump"))
		{
			player.OnJumpInputDown();
		}
		if (Input.GetButtonUp("Jump"))
		{
			player.OnJumpInputUp();
		}

		if (Input.GetButtonDown("Fire1"))
		{
			player.OnFireInputDown();
		}
		if (Input.GetButtonUp("Fire1"))
		{
			player.OnFireInputUp();
		}
	}
}
