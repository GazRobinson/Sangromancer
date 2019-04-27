using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodManager : MonoBehaviour {
    public RenderTexture renderTex;
    public Renderer target;
    public Texture2D attackTex;
    public Texture2D attackTex2;
    public Sprite testSprite;
    Color[] pixels = new Color[64 * 64];
    Texture2D texture;
    public List<WaterCell> waterList = new List<WaterCell>();


    public float yMaxSpeed = 50f;

    int at = 0;
    bool sim = true;
    public float gravity = 40f;

    public class WaterCell {
        public float               life        = 2f;
        public Vector2   Position    = Vector2.zero;
        public Vector2   Velocity    = Vector2.zero;

        public WaterCell( ) {
        }
        public WaterCell( Vector2 pos, Vector2 vel ) {
            Position = pos;
            Velocity = vel;
        }
        public WaterCell( Vector2 pos, Vector2 vel, float l ) {
            Position = pos;
            Velocity = vel;
            life = l;
        }

    }

	// Use this for initialization
	void Awake () {
        texture = new Texture2D( 64, 64 );
        target.material.mainTexture = texture;
        texture.filterMode = FilterMode.Point;

	}
    void SpawnWater() {
        Vector2 offset = transform.position;
        Rect r = testSprite.textureRect;
        Color[] atkPix = testSprite.texture.GetPixels((int)r.x, ( int )r.y, ( int )r.width, ( int )r.height);//attackTex.GetPixels();
       
        int i = 0;
        Debug.Log( atkPix[0].ToString() );
        foreach ( Color c in atkPix ) {
            if ( c.a > 0 ) {

                waterList.Add( new WaterCell( offset + new Vector2( i % 16, i / 16 ), new Vector2( ( ( c.r - 0.5f ) * 2f ) * PixelSpeed(), ( ( c.g - 0.5f ) * 2f ) * PixelSpeed() ), Random.Range(1.5f,2.5f) ) );
                waterList.Add( new WaterCell( offset + new Vector2( i % 16, i / 16 ), new Vector2( ( ( c.r - 0.5f ) * 2f ) * PixelSpeed(), ( ( c.g - 0.5f ) * 2f ) * PixelSpeed() ), Random.Range( 1.5f, 2.5f ) ) );
            }
            i++;
        }
    }

    float PixelSpeed() {
        return Random.Range( yMaxSpeed - 5, yMaxSpeed + 5 );
    }

    void UpdateWater() {
        List<WaterCell> killList = new List<WaterCell>();
        float dt = Time.deltaTime;
        Vector2 newPos = Vector2.zero;
        Vector2 gravityVec = Physics2D.gravity * dt;
        foreach ( WaterCell w in waterList ) {
            w.life -= dt;   //Lose life
            if ( w.life > 0 ) {
                //If it's still alive
                //Apply gravity
                w.Velocity += gravityVec;
                Vector2 thisV = w.Velocity * dt;
                thisV = Vector2.ClampMagnitude( thisV, 0.99f );
                newPos = ( w.Position + thisV );

                //Work out the new desired position
                Vector2Int newPosI = new Vector2Int( Mathf.RoundToInt( newPos.x ), Mathf.RoundToInt( newPos.y ) );

                if ( newPosI.x >= 0 && newPosI.x < 64 && newPosI.y >= 0 && newPosI.y < 64 ) {
                    if ( Mathf.Approximately( pixels[newPosI.x + ( newPosI.y * 64 )].r, 0f ) ) {
                        Vector2 vector = newPos - w.Position;

                        Vector2 diff = new Vector2( Mathf.RoundToInt( newPos.x ), Mathf.RoundToInt( newPos.y ) ) - newPos;
                        //Collision
                        if ( Mathf.Abs( diff.y ) > Mathf.Abs( diff.x ) ) {
                            //flip y
                            newPos = w.Position;
                            w.Velocity.y = -Decay( w.Velocity.y );
                        }
                        else {
                            //flip x
                            newPos = w.Position;
                            w.Velocity.x = -Decay( w.Velocity.x );
                        }

                    }
                }
                else {
                    killList.Add( w );
                }


                w.Position = newPos;
            }
            else {
                killList.Add( w );
            }
        }
        if ( killList.Count > 0 ) {
            foreach ( WaterCell w in killList ) {
                waterList.Remove( w );
            }
            killList.Clear();
        }
    }
    float Decay( float v ) {
        return v * Random.Range( 0.4f, 0.8f );
    }
    Vector2 p;
	// Update is called once per frame
	void Update () {
        if ( Input.GetKeyDown( KeyCode.E ) ) {
            SpawnWater();
        }
        if ( Input.GetMouseButtonDown( 0 ) ) {
            p = Camera.main.ScreenToWorldPoint( Input.mousePosition );
        }
        if ( Input.GetMouseButton( 0 ) ) {

            Vector2 p2 = Camera.main.ScreenToWorldPoint( Input.mousePosition );

            waterList.Add( new WaterCell( p, fudgeDirection( p2 - p ) * yMaxSpeed, Random.Range( 0.5f, 1.0f ) ) );
            waterList.Add( new WaterCell( p, fudgeDirection( p2 - p ) * yMaxSpeed, Random.Range( 0.5f, 1.0f ) ) );
            waterList.Add( new WaterCell( p, fudgeDirection( p2 - p ) * yMaxSpeed, Random.Range( 0.5f, 1.0f ) ) );
            waterList.Add( new WaterCell( p, fudgeDirection( p2 - p ) * yMaxSpeed, Random.Range( 0.5f, 1.0f ) ) );
        }
        at++;

        RenderTexture.active = renderTex;
        //don't forget that you need to specify rendertexture before you call readpixels
        //otherwise it will read screen pixels.
        texture.ReadPixels( new Rect( 0, 0, renderTex.width, renderTex.height ), 0, 0 );
        pixels = texture.GetPixels();

        UpdateWater();
        foreach ( WaterCell w in waterList ) {
            int x = Mathf.RoundToInt(w.Position.x);
            int y = Mathf.RoundToInt( w.Position.y );
            if(x>=0 && x<64 && y>=0 && y < 64)
                pixels[x + ( y * 64 )] = Color.Lerp(Color.red, new Color(0.3f,0f,0f,0f), Mathf.InverseLerp(2f, 0f, w.life));
        }
       
        texture.SetPixels( pixels );
        texture.Apply();
        RenderTexture.active = null; //don't forget to set it back to null once you finished playing with it.
	}
    Vector2 fudgeDirection( Vector2 dir ) {
        return Quaternion.AngleAxis( Random.Range( -15f, 15f ), Vector3.forward ) * dir;
    }
}
