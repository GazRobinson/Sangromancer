using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTest : MonoBehaviour {
    private Animator animController;
    public Animator attackAnimController;
	// Use this for initialization
	void Start () {
        animController = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
        animController.SetBool( "Running", Mathf.Abs( Input.GetAxisRaw( "Horizontal" )) > 0 );
        if ( Input.GetButtonDown( "Jump" ) ) {
            animController.SetTrigger( "Jump" );

            animController.SetBool( "Grounded", false );
        }

        if ( Input.GetButtonUp( "Jump" ) )
            animController.SetBool( "Grounded", true );

        if ( Input.GetButtonDown( "Fire1" ) )
            attackAnimController.SetTrigger( "Atk");
	}
}
